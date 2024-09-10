using UnityEngine;
using System.Collections;
//using System.Linq;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour 
{

	#region Editor Publics
	[SerializeField] SpawnObjects spawnObjects;
	[SerializeField] SpawnThresholds spawnThresholds;
	//Distance metrics for Phase1State1
	[SerializeField] private float incrementThreshold = 1.0f;
	[SerializeField] private float incrementValue = 1.0f;
	#endregion

	#region Privates
	private GestureManager gManager;
	private GameStateManager gStateManager;
//	private SoundManager sManager;


	public int objectCounter = 0;
	private float AverageRT = 100.0f;
	//Distance handling for Phase1State1
	private List<float> LongestHits = new List<float>();
	private List<float> ShortestFails = new List<float>();	
	private List<int> calibrationList = new List<int>();
	#endregion
	
	void Awake()
	{
		gManager = Camera.main.GetComponent<GestureManager>();
		if(gManager == null)
			Debug.LogError("No GestureManager was found on the main camera.");

//		sManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		gStateManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();

		//Initialise distance lists
		ResetLists();
	}

	void Start()
	{
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");
	}

	#region Public Methods
	public void SetAverageReactionTime(float reactionTime)
	{
		AverageRT = reactionTime;
	}

	//TEMPORARY: Method for manually adjusting succes distance;
	public void SetLongestHit(int int1to10)
	{
		var i = int1to10 - 1;
		LongestHits[i] = GetAbsMaxDist(int1to10);
	}


	//Methods for adjusting distance metrics.
	public void ReportHit(int int1to10, float distance)
	{
		var i = int1to10 - 1;

		//Record new longest hit
		if(distance > LongestHits[i])
			LongestHits[i] = distance;

		//Adjust shortest fails if best hit is longer
		if(LongestHits[i] > ShortestFails[i])
			ShortestFails[i] = LongestHits[i];
	}

	public void ReportMiss(int int1to10, float distance)
	{
		var i = int1to10 - 1;

		//Record new shortest fail
		if(distance < ShortestFails[i])
			ShortestFails[i] = distance;
	}
	//---------------------------------------

	//Methods for spawning
	public void SpawnSpecific(GameObject spawnObject, int int1to10)
	{
		//Get specific angle
		int angle = GetAngle(int1to10);
		//Get Random distance
		float distance = Random.Range(GetAbsMinDist(angle), GetAbsMaxDist(angle));
		//Rotate self by specific angle
		RotateSelf(angle);
		//Record spawn rotation & Position
		Quaternion rotation = transform.rotation;
		Vector3 position = gameObject.transform.up * distance;
		//Rotate self back by specific angle
		RotateSelf(-angle);
		//Instantiate game object
		GameObject go = (GameObject) Instantiate(spawnObject, position, rotation);
		//Set Object Parameters
		go.GetComponent<ObjectHandler>().SetAngle((int) angle);
		go.GetComponent<ObjectHandler>().SetID(objectCounter);
		go.GetComponent<ObjectHandler>().SetDistance(distance);
		go.GetComponent<ObjectHandler>().SetSpawnTime(Time.time);
		go.GetComponent<ObjectHandler>().SetMultiplier(int1to10);
		go.GetComponent<ObjectHandler>().SetMeanReactionTime(AverageRT);
		go.name = go.name+int1to10;
		//Set occupied
	}

	public void SpawnSpecific(GameObject spawnObject, int int1to10, float distance)
	{
		//Get specific angle
		int angle = GetAngle(int1to10);
		//Rotate self by specific angle
		RotateSelf(angle);
		//Record spawn rotation & Position
		Quaternion rotation = transform.rotation;
		Vector3 position = gameObject.transform.up * distance;
		//Rotate self back by specific angle
		RotateSelf(-angle);
		//Instantiate game object
		GameObject go = (GameObject) Instantiate(spawnObject, position, rotation);
		//Set Object Parameters
		go.GetComponent<ObjectHandler>().SetAngle((int) angle);
		go.GetComponent<ObjectHandler>().SetID(objectCounter);
		go.GetComponent<ObjectHandler>().SetDistance(distance);
		go.GetComponent<ObjectHandler>().SetSpawnTime(Time.time);
		go.GetComponent<ObjectHandler>().SetMultiplier(int1to10);
		go.GetComponent<ObjectHandler>().SetMeanReactionTime(AverageRT);
		go.name = go.name+int1to10;
		//Set occupied
	}

	public void SpawnRandom(GameObject spawnObject)
	{
		int multiplier = Random.Range(1,11);
		float distance = Random.Range(2.0f, 8.5f);

		SpawnSpecific(spawnObject, multiplier, distance);
	}

	public void SpawnCalibration()
	{
		if(calibrationList.Count <= 0)
		{
			//Fill array
			for(int i = 1; i <= 5; i++)
				calibrationList.Add(i);
		}

		//Get random identifier & get angleID from list
		int angleID = calibrationList[Random.Range(0, calibrationList.Count)];
		//Get abs distance value / 2
		float distance = (GetAbsMaxDist(angleID)/3) * 2;
		//Remove from List
		calibrationList.Remove(angleID);
		//SpawnSpecific calibrationObject
		SpawnSpecific(spawnObjects.SingleTarget, angleID, distance);
	}

	public void Phase1Stage1(int int1to10)
	{
		//Adjust the index for lists that begin at 0
		var index = int1to10 - 1;

		float distance;
		
		//Increment ObjectCounter
		objectCounter++;

		//Calculate distance. If space between hits and fails are to narrow, 
		//and we have more space to the border of the screen, increment by a factor of 1.0f on fails side
		if(ShortestFails[index] - LongestHits[index] < incrementThreshold)
		{
			if(ShortestFails[index] + incrementValue <= GetAbsMaxDist(int1to10))
				distance = Random.Range(LongestHits[index], ShortestFails[index]) + incrementValue;
			else
			{
				ShortestFails[index] = GetAbsMaxDist(int1to10);
				distance = Random.Range(LongestHits[index], ShortestFails[index]);
			}
		}
		else
		{
			distance = Random.Range(LongestHits[index], ShortestFails[index]);
		}

		//Check if distance is still below incremenent threshold
		if(GetAbsMaxDist(int1to10) - LongestHits[index] < incrementThreshold)
		{
			//Flag for state 2
			gStateManager.SetAngleState(int1to10, 0);

			//Spawn sequential target
			SpawnSpecific(spawnObjects.SequentialTarget, int1to10, distance);
		}
		else
		{
			//Spawn target normally
			SpawnSpecific(spawnObjects.SingleTarget, int1to10, distance);
		}
	}

	public void Phase1Stage2(int int1to10)
	{
		int index;
		//Calculate opposite index
		if(int1to10 + 5 > 10)
			index = int1to10 + 5 - 10;
		else
			index = int1to10 + 5;
		
		//Increment ObjectCounter
		objectCounter++;

		//Calculate distance based on opposite progress
		float distance = GetAbsMaxDist(int1to10) - LongestHits[index - 1];

		if(distance < GetAbsMinDist(int1to10))
		{
			//Spawn stage 2 item with minimum distance
			SpawnSpecific(spawnObjects.sequentialTarget2, int1to10, GetAbsMinDist(int1to10));
			//Flag this angle for multiple Targets
			gStateManager.SetAngleState(int1to10, 1); //Goes from Sequential to Multitarget
			gStateManager.CheckPhase1Ended();
		}
		else
		{
			//Spawn stage 2 item
			SpawnSpecific(spawnObjects.sequentialTarget2, int1to10, distance);
		}
	}

	public void Phase1Stage3(int int1to10)
	{
		
		//Increment ObjectCounter
		objectCounter++;

		float dist = Random.Range(GetAbsMinDist(int1to10), GetAbsMaxDist(1));

		for(int i = -1; i <= 1; i++)
		{
			int j;

			//Catch out of bound errors
			if(int1to10 + i > 10)
				j = int1to10 + i - 10;
			else if(int1to10 + i < 1)
				j = int1to10 + i + 10;
			else
				j = int1to10 + i;

			//Spawn Objects
			SpawnSpecific(spawnObjects.MultiTarget, j, dist);
		}
	}

	public void Phase2Stage1()
	{
		if(GameObject.Find("Phase2Center(Clone)") == null)
		{
			GameObject go = (GameObject) Instantiate(spawnObjects.Phase2Targets, Vector3.zero, Quaternion.identity);
			go.GetComponent<Phase2Behavior>().SetMeanReactionTime(AverageRT);
			go.GetComponent<Phase2Behavior>().SetObjectCounter(objectCounter);
		}
		else
		{
			GameObject go = GameObject.Find("Phase2Center(Clone)");
			go.GetComponent<Phase2Behavior>().ResetActiveTargets();
		}
	}
	#endregion

	#region Class Methods
	private int GetAngle(int int1to10)
	{
		int angle = (36 * int1to10) - 18;
		return angle;
	}

	private void RotateSelf(int angle)
	{
		transform.Rotate(0, 0, (float) -angle);
	}

	private void ResetLists()
	{
		LongestHits.Clear();
		ShortestFails.Clear();

		for(int i = 1; i <= 10; i++)
		{
			LongestHits.Add(2.0f);
			ShortestFails.Add(GetAbsMaxDist(i));
		}

		calibrationList.Clear();
	}

	//Method for checking if we are above maximum
	private bool CheckMax(int angle, float distance)
	{
		switch(angle)
		{
		case 1:
		case 5:
		case 6:
		case 10:
		{
			if(distance < spawnThresholds.MinMax_1_5_6_10.y)
				return true;
			else
				return false;
		}
		case 2:
		case 4:
		case 7:
		case 9:
		{
			if(distance < spawnThresholds.MinMax_2_4_7_9.y)
				return true;
			else
				return false;
		}
		case 3:
		case 8:
		{
			if(distance < spawnThresholds.MinMax_3_8.y)
				return true;
			else
				return false;
		}
		default:
				return false;
		}
	}

	//Method for checking if we are below minimum
	private bool CheckMin(int angle, float distance)
	{
		switch(angle)
		{
		case 1:
		case 5:
		case 6:
		case 10:
		{
			if(distance > spawnThresholds.MinMax_1_5_6_10.x)
				return true;
			else
				return false;
		}
		case 2:
		case 4:
		case 7:
		case 9:
		{
			if(distance > spawnThresholds.MinMax_2_4_7_9.x)
				return true;
			else
				return false;
		}
		case 3:
		case 8:
		{
			if(distance > spawnThresholds.MinMax_3_8.x)
				return true;
			else
				return false;
		}
		default:
			return false;
		}
	}
	
	//Method for getting min distances
	public float GetAbsMinDist(int int1to10)
	{
		switch(int1to10)
		{
		case 1:
		case 5:
		case 6:
		case 10:
		{
			return spawnThresholds.MinMax_1_5_6_10.x;
		}
		case 2:
		case 4:
		case 7:
		case 9:
		{
			return spawnThresholds.MinMax_2_4_7_9.x;
		}
		case 3:
		case 8:
		{
			return spawnThresholds.MinMax_3_8.x;
		}
		default:
			return 3;
		}
	}

	//Method for getting max distances
	public float GetAbsMaxDist(int int1to10)
	{
		switch(int1to10)
		{
		case 1:
		case 5:
		case 6:
		case 10:
		{
			return spawnThresholds.MinMax_1_5_6_10.y;
		}
		case 2:
		case 4:
		case 7:
		case 9:
		{
			return spawnThresholds.MinMax_2_4_7_9.y;
		}
		case 3:
		case 8:
		{
			return spawnThresholds.MinMax_3_8.y;
		}
		default:
			return 5;
		}
	}
	#endregion

	#region Notification Methods
	private void NC_Restart()
	{
		ResetLists();
		AverageRT = 100.0f;
		objectCounter = 0;
	}

	private void NC_Play()
	{
		ResetLists();
		AverageRT = 100.0f;
		objectCounter = 0;
	}
	#endregion

	#region Subclasses
	[System.Serializable]
	public class SpawnObjects
	{
		public GameObject SingleTarget;
		public GameObject SequentialTarget;
		public GameObject sequentialTarget2;
		public GameObject MultiTarget;
		public GameObject Phase2Targets;
	}

	[System.Serializable]
	public class SpawnThresholds
	{
		public Vector2 MinMax_1_5_6_10 = new Vector2(2.0f, 10.0f);
		public Vector2 MinMax_2_4_7_9 = new Vector2(2.0f, 16.0f);
		public Vector2 MinMax_3_8 = new Vector2(2.0f, 15.0f);
	}
	#endregion
}
