using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Phase2Behavior : MonoBehaviour {

	private GameStateManager gameManager;
	private SpawnManager spawnManager;
	private SoundManager soundManager;
	private GUIManager guiManager;

	[SerializeField] private GameObject SpawnObject;
	private GameObject[] Targets;

	private int currentAmountOfActiveTargets = 0;
	private int currentAmountOfHits = 0;
	private float startDistance = 10.0f;
	private float currentDistance;
	private bool missRecieved = false;

	private enum Stage {Right, Left, Both};
	private Stage stage = Stage.Right;

	private float artLifetime = 100.0f;
	private int objectCounter;

	void Awake()
	{
		gameManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
		spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
		soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		guiManager =  GameObject.Find("3DGUICamera").GetComponent<GUIManager>();
	}

	void Start () {
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		SpawnXTargets(10, 0);
		StoreTargets();
		StartCoroutine("StartStage");
		currentDistance = startDistance;

		//Determine curtain blocking
		if(stage == Stage.Right)
		{
			guiManager.BlockRightHalf(false);
			guiManager.BlockLeftHalf(true);
		}
		else if(stage == Stage.Left)
		{
			guiManager.BlockRightHalf(true);
			guiManager.BlockLeftHalf(false);
		}
		else if(stage == Stage.Both)
			guiManager.BlockAll(false);

	}
	
	// Update is called once per frame
	void Update () 
	{

	}
	
	public void SetMeanReactionTime(float reactionTime)
	{
		artLifetime = reactionTime;
	}
	public void SetObjectCounter(int count)
	{
		objectCounter = count;
	}

	private void StoreTargets()
	{
		Targets = GameObject.FindGameObjectsWithTag("Phase2Object");
	}

	public IEnumerator StartStage()
	{
		yield return new WaitForSeconds(1.0f);
		ChangeDistance(0);
		gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);
//		ResetActiveTargets();
	}

	public void ResetActiveTargets()
	{
		currentAmountOfActiveTargets = 2; //Random.Range(2,4); //Random range on int is exclusive max

		SetTargetsActive2();

		currentAmountOfHits = 0;
	}

	private List<int> RightSideTargets = new List<int>();
	private List<int> LeftSideTargets = new List<int>();
	private void SetTargetsActive2()
	{
		//Increase the targetID
		objectCounter++;

		List<int> targets = new List<int>();
		if(RightSideTargets.Count <= 1 || LeftSideTargets.Count <= 1)
		{
			RightSideTargets.Clear();
			LeftSideTargets.Clear();

			//Fill Lists
			for(int i = 1; i <= 10; i++)
			{
				if(i <= 5)
					RightSideTargets.Add(i);
				else
					LeftSideTargets.Add(i);
			}
		}

		if(stage == Stage.Right)
		{
			int angle = RightSideTargets[Random.Range(0, RightSideTargets.Count)];
			targets.Add(angle);
			RightSideTargets.Remove(angle);
		
			int secondAngle = RightSideTargets[Random.Range(0, RightSideTargets.Count)];
			targets.Add(secondAngle);
		}
		else if(stage == Stage.Left)
		{
			int angle = LeftSideTargets[Random.Range(0, LeftSideTargets.Count)];
			targets.Add(angle);
			LeftSideTargets.Remove(angle);
			
			int secondAngle = LeftSideTargets[Random.Range(0, LeftSideTargets.Count)];
			targets.Add(secondAngle);
		}
		else if(stage == Stage.Both)
		{
			int rightAngle = RightSideTargets[Random.Range(0, RightSideTargets.Count)];
			targets.Add(rightAngle);
			RightSideTargets.Remove(rightAngle);

			int leftAngle = LeftSideTargets[Random.Range(0, LeftSideTargets.Count)];
			targets.Add(leftAngle);
			LeftSideTargets.Remove(leftAngle);
		}

		soundManager.PlayNewTargetSpawned();

		for(int i = 0; i < targets.Count; i++)
		{
			GameObject go = Targets[targets[i]-1];
			go.GetComponent<Phase2Object>().SetID(objectCounter);
			go.GetComponent<Phase2Object>().SetActiveTarget();
		}
	}

	public void SendHit(){
		currentAmountOfHits++;
		if(currentAmountOfActiveTargets == currentAmountOfHits)
		{
			if(currentDistance + 10.0 <= 100.0f)
				gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);

			ChangeDistance(10);
		}
	}

	public void SendMiss()
	{
		StartCoroutine(WaitForAllMisses());
	}

	private IEnumerator WaitForAllMisses()
	{
		if (!missRecieved){
			missRecieved = true;
			currentAmountOfHits = 0;
			gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);
			ChangeDistance(-10);
		}
		yield return new WaitForSeconds(0.5f);
		missRecieved = false;
	}

	private void ChangeDistance(float distance)
	{
		currentDistance += distance;
		float minDistance = spawnManager.GetAbsMinDist(1);

		if(currentDistance < (10.0f * minDistance))
		{
			currentDistance = (10.0f * minDistance);
		}
		if(currentDistance > 100.0f)
		{
			currentDistance = (10.0f * minDistance);
   			StartCoroutine(ChangeStage());
		}

		foreach (GameObject target in Targets)
		{
			float maxDistanceForThisAngle = (spawnManager.GetAbsMaxDist(target.GetComponent<Phase2Object>().GetMultiplier()));

			float maxDistanceForShortestAngle = spawnManager.GetAbsMaxDist(1);

			float adjustedDistance = (((currentDistance/100.0f)*maxDistanceForShortestAngle)/2.0f)
									+(((currentDistance/100.0f)*maxDistanceForThisAngle)/2.0f);

			iTween.MoveTo(target, iTween.Hash("position", target.transform.up * adjustedDistance, "time", 0.5f, "easetype", iTween.EaseType.easeInBack));

			target.GetComponent<Phase2Object>().SetDistance(adjustedDistance);
		}
	}

	private IEnumerator ChangeStage()
	{
		if(stage == Stage.Right){
			stage = Stage.Left;
			ChangeTargetType(Phase2Object.ObjectTypes.P2_Left);
			yield return new WaitForSeconds(1.0f);
			guiManager.BlockRightHalf(true);
			yield return new WaitForSeconds(1.0f);
			guiManager.BlockLeftHalf(false);
			gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);
		}
		else if(stage == Stage.Left){
			stage = Stage.Both;
			ChangeTargetType(Phase2Object.ObjectTypes.P2_Both);
			yield return new WaitForSeconds(1.0f);
			guiManager.BlockAll(false);
			gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);
		}
		else if(stage == Stage.Both){
			NotificationCenter.DefaultCenter().PostNotification(this, "NC_GameOver");
		}
	}

	private void ChangeTargetType(Phase2Object.ObjectTypes type)
	{
		foreach(GameObject go in Targets)
		{
			go.GetComponent<Phase2Object>().SetObjectType(type);
		}
	}

	public void SetStageRight()
	{
		stage = Stage.Right;
		guiManager.BlockLeftHalf(true);
		guiManager.BlockRightHalf(false);
	}

	public void SetStageLeft()
	{
		stage = Stage.Left;
		guiManager.BlockLeftHalf(false);
		guiManager.BlockRightHalf(true);
	}

	public void SetStageBoth()
	{
		stage = Stage.Both;
		guiManager.BlockAll(false);
	}

	private void SpawnSpecific(int int1to10, float distance)
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
		GameObject go = (GameObject) Instantiate(SpawnObject, position, rotation);
		go.transform.parent = transform;
		//Set Object Parameters
		go.GetComponent<Phase2Object>().SetAngle((int) angle);
		//go.GetComponent<Phase2Object>().SetDistance(distance);
		//go.GetComponent<Phase2Object>().SetSpawnTime(Time.time);
		go.GetComponent<Phase2Object>().SetMultiplier(int1to10);
		go.GetComponent<Phase2Object>().SetTargetDisabled();
		go.GetComponent<Phase2Object>().SetMeanReactionTime(artLifetime);
		go.name = go.name+int1to10;
		//Set occupied
	}
	
	private void SpawnXTargets(int amountOfTargets, float distance)
	{
		for(int i = 1; i <= amountOfTargets; i++)
		{
			SpawnSpecific(i, distance);
		}
	}

	private int GetAngle(int int1to10)
	{
		int angle = (36 * int1to10) - 18;
		return angle;
	}
	
	private void RotateSelf(int angle)
	{
		transform.Rotate(0, 0, (float) -angle);
	}

	private void NC_Restart()
	{
		Destroy(gameObject, 0.5f);
	}

}