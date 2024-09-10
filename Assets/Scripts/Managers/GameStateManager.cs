using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStateManager : MonoBehaviour 
{
	#region Publics
	#endregion

	#region Editor Publics
	[SerializeField] private float spawnTime = 3.0f;
	[SerializeField] private GameObject CenterExplosion;
	[SerializeField] private GameObject CenterPunch;
	[SerializeField] private int calibrationAmount = 15;
	#endregion

	#region Privates
	private Color FullGreenColor = new Color(0.23f, 1.0f, 0.0f, 1.0f);
	//Script connectivity
	private GestureManager gManager;
	private GUIManager guiManager;
	private SpawnManager sManager;
	private SoundManager soundManager;
	private ClockHandler clock;
	private HighscoreManager highscoreManager;

	//Time logs
	private float SpawnBegin;

	//Booleans
	private bool allowPunching = true;
	private bool playModeActive = false;

	//States
	[HideInInspector] public enum State {awaitCenterClick, awaitTargetSpawn, awaitTargetClick, awaitTargetReturnToCenter};
	[HideInInspector] public State state;
	private Phase1States phase1States = new Phase1States();
	public enum Phases {Phase0, Phase1, Phase2, Phase3};
	[HideInInspector] public Phases phase;

	//Counters
	private int calibrationCounter = 0;
	private int multiTargetCounter = 0;

	//Lists
	private List<int> targetList = new List<int>();

	private WriteToTXT txtWriter;
	public float lastHitTime;

	#endregion

	void Awake()
	{
		gManager = Camera.main.GetComponent<GestureManager>();
		if(gManager == null)
			Debug.LogError("No GestureManager was found on the main camera.");

		guiManager = GameObject.Find("3DGUICamera").GetComponent<GUIManager>();
		highscoreManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();

		sManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
		if(sManager == null)
			Debug.LogError("No SpawnManager was found in the scene.");

		soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		if(soundManager == null)
			Debug.LogError("No SoundManager was found in the scene.");

		clock = GameObject.Find("SpawnClock").GetComponent<ClockHandler>();
		clock.SetTime(spawnTime);

		txtWriter = GameObject.Find("WriteToTXT").GetComponent<WriteToTXT>();
		if(txtWriter == null)
			Debug.LogError("No txtWriter was found in the scene.");
	}

	void Start ()
	{
		//Begin at Phase 0 - Calibration
		phase = Phases.Phase0;
		//Subscribe to Tap Gesture
		gManager.OnTapBegan += HandleOnTapBegan;
		//Begin Punching
		InvokeRepeating("PunchCenter", 0, 1.25f);

		ChangeCenterState(State.awaitCenterClick);
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
	}

	void Update()
	{
		switch (state)
		{
		case State.awaitCenterClick:
			break;
		case State.awaitTargetSpawn:
			if(Time.time >= spawnTime + SpawnBegin)
			{
				//Change the state
				ChangeCenterState(State.awaitTargetClick);

				//Spawn according to state
				if(phase == Phases.Phase0)
				{
					if(calibrationCounter < calibrationAmount)
					{
						//Spawn calibration Target
						sManager.SpawnCalibration();
						soundManager.PlayNewTargetSpawned();
						//Increase Count
						calibrationCounter++;
					}
					else
					{
						float rt = highscoreManager.GetReactionMean();
						//Calibration done. Set user Mean reaction Time in SpawnManager
						sManager.SetAverageReactionTime(rt + 0.15f);
						//Flag for phase1
						phase = Phases.Phase1;
						sManager.SpawnCalibration();
						guiManager.BlockLeftHalf(false);
					}
				}
				else if(phase == Phases.Phase1)
				{
					//Get random target
					int int1to10 = GetWeightedRandomAngle();

					//Spawn target according to current state
					if(phase1States.GetAngleState(int1to10) == Phase1States.States.SingleTarget ||
					   phase1States.GetAngleState(int1to10) == Phase1States.States.SequentialTarget)
					{
						soundManager.PlayNewTargetSpawned();
						sManager.Phase1Stage1(int1to10);
					}
					else if(phase1States.GetAngleState(int1to10) == Phase1States.States.MultipleTargets)
					{
						soundManager.PlayNewTargetSpawned();
						sManager.Phase1Stage3(int1to10);
					}
				}
				else if(phase == Phases.Phase2)
				{
					sManager.Phase2Stage1();
				}
			}
			break;
		case State.awaitTargetClick:
			if(multiTargetCounter >= 3)
			{
				multiTargetCounter = 0;
				ChangeCenterState(State.awaitCenterClick);
			}
			break;
		case State.awaitTargetReturnToCenter:
			break;
		default:
			break;
		}
	}

	void HandleOnTapBegan (Vector2 screenPos)
	{
		if(playModeActive)
		{
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo))
			{
				if(hitInfo.collider == gameObject.GetComponent<Collider>())
				{
					if(state == State.awaitCenterClick) {

						txtWriter.LogData("Center", Time.time - lastHitTime, 0, 0, transform.position, screenPos, "CenterHit", 0, -1);
						//Change the state to currently awaiting a spawn
						ChangeCenterState(State.awaitTargetSpawn);
						//Begin clock rotation
						//clock.BeginClock();
						lastHitTime = Time.time;
						//soundManager.PlayTouchBegan();
						OnClickCenterPunching();
					}
					else {

						//Declare variables
						float dist = 0.0f;
						Vector3 pos = Vector3.zero;
						int angleID = 0;
						int angle = 0;
						List<GameObject> targetList = new List<GameObject>();

						//Search for targets and fill list
						if(GameObject.FindGameObjectsWithTag("Target") != null)
							targetList.AddRange(GameObject.FindGameObjectsWithTag("Target"));
						
						//Is there normal targets? 
						if(targetList.Count != 0)
						{
							//Go through all targets found
							foreach(GameObject target in targetList)
							{
								var position = Camera.main.WorldToScreenPoint(target.transform.position);
								var distance = Vector2.Distance(screenPos, position);
								
								if(dist == 0 || dist > distance)
								{
									pos = target.transform.position;
									dist = distance;
									angleID = target.GetComponent<ObjectHandler>().GetAngleID();
									angle = target.GetComponent<ObjectHandler>().GetAngle();
								}
							}
						}
						//if not, are there phase2objects? 
						else if(GameObject.FindGameObjectsWithTag("Phase2Object") != null)
						{
							targetList.AddRange(GameObject.FindGameObjectsWithTag("Phase2Object"));
							
							//Go through all targets found
							foreach(GameObject target in targetList)
							{
								//If they are active, check for distance
								if(target.GetComponent<Phase2Object>().TargetIsActive())
								{
									var position = Camera.main.WorldToScreenPoint(target.transform.position);
									var distance = Vector2.Distance(screenPos, position);
									
									if(dist == 0 || dist > distance)
									{
										pos = target.transform.position;
										dist = distance;
										angleID = target.GetComponent<Phase2Object>().GetMultiplier();
										angle = target.GetComponent<Phase2Object>().GetAngle();
									}
								}
							}
						}
						txtWriter.LogData("PrematureCenter", Time.time - lastHitTime, angle, dist, pos, screenPos, "CenterError", 0, angleID);
					}
				}
			}
		}
	}

	#region Public Methods
	public void IncreaseMultiTargetCounter()
	{
		multiTargetCounter++;
	}

	public int GetMultiTargetCount()
	{
		return multiTargetCounter;
	}

	public void SetPhase(Phases _phase)
	{
		phase = _phase;
	}

	public void SetAngleState(int int1to10, int state)
	{
		if(state == 0 && phase1States.GetAngleState(int1to10) == Phase1States.States.SingleTarget)
			phase1States.SetAngleState(int1to10, Phase1States.States.SequentialTarget);
		else if(state == 1 && phase1States.GetAngleState(int1to10) == Phase1States.States.SequentialTarget)
			phase1States.SetAngleState(int1to10, Phase1States.States.MultipleTargets);
		else if(state == 2 && phase1States.GetAngleState(int1to10) == Phase1States.States.MultipleTargets)
			phase = Phases.Phase2;
	}

//	public int GetAngleState(int int1to10)
//	{
//		return (int) phase1States.GetAngleState(int1to10);
//	}

	//Public Method for changing the center state.
	public void ChangeCenterState(State plate)
	{
		switch (plate)
		{
		case State.awaitCenterClick:
			state = State.awaitCenterClick;
			iTween.ColorTo(gameObject, iTween.Hash("color", FullGreenColor, "time", 0.2f, "includeChildren", false));
			gameObject.transform.position = new Vector3 (0,0,0);
			break;
		case State.awaitTargetSpawn:
			state = State.awaitTargetSpawn;
			SpawnBegin = Time.time;
			iTween.ColorTo(gameObject, iTween.Hash("color", Color.white, "time", 0.2f, "includeChildren", false));
			break;
		case State.awaitTargetClick:
			state = State.awaitTargetClick;
			iTween.ColorTo(gameObject, iTween.Hash("color", Color.white, "time", 0.4f, "includeChildren", false));
			break;
		case State.awaitTargetReturnToCenter:
			state = State.awaitTargetReturnToCenter;
			StartCoroutine(AwaitTargetToCenter());
			break;
		default:
			break;
		}
	}

	public void CheckPhase1Ended()
	{
		for(int i = 1; i <= 10; i++)
		{
			if(phase1States.GetAngleState(i) == Phase1States.States.SingleTarget || 
			   phase1States.GetAngleState(i) == Phase1States.States.SequentialTarget)
			{
				return;
			}
		}
		phase = Phases.Phase2;
	}
	#endregion

	#region Class Methods
	private void PunchCenter()
	{
		if(!(state == State.awaitTargetClick) && allowPunching && playModeActive)
		{
			GameObject PunchClone = Instantiate(CenterPunch, transform.position, transform.rotation) as GameObject;
			Destroy(PunchClone, 1.2f);
			//iTween.PunchPosition(gameObject, new Vector3(0f, 0.1f, 0.0f), 1.0f);
			iTween.PunchScale(gameObject, new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
			StartCoroutine(ResetGOScale());
			//			soundManager.PlayCenterPunch();
		}
	}

	private IEnumerator AwaitTargetToCenter()
	{
		yield return new WaitForSeconds(0.5f);
		iTween.ColorTo(gameObject, iTween.Hash("color", FullGreenColor, "time", 0.0f, "includeChildren", false));
		yield return new WaitForSeconds(0.3f);
		ChangeCenterState(State.awaitCenterClick);
	}

	public IEnumerator SpawnCenterExplosion(Vector3 angle)
	{
		/*if(allowPunching)
		{
			allowPunching = false;
			yield return new WaitForSeconds(0.5f);
			iTween.PunchPosition(gameObject, angle*1.01f, 0.5f);
			iTween.PunchScale(gameObject, new Vector3(0.4f, 1.0f, 0.4f), 0.5f);
			GameObject ExpClone = Instantiate(CenterExplosion, transform.position, transform.rotation) as GameObject;
			Destroy(ExpClone, 1.2f);
			ResetGOScale();
			yield return new WaitForSeconds(0.5f);
			allowPunching = true;
		}
		else
		{			
			yield return new WaitForSeconds(0.5f);
			GameObject ExpClone = Instantiate(CenterExplosion, transform.position, transform.rotation) as GameObject;
			Destroy(ExpClone, 1.2f);
		}*/
		yield return new WaitForSeconds(0.5f);
	}

	private void OnClickCenterPunching()
	{
		if(allowPunching)
		{
			iTween.PunchScale(gameObject, new Vector3(1.5f, 1.5f, 1.5f), 0.5f);
			StartCoroutine(ResetGOScale());
		}

		GameObject PunchClone = Instantiate(CenterPunch, transform.position, transform.rotation) as GameObject;
		Destroy(PunchClone, 1.2f);
	}

	private IEnumerator ResetGOScale()
	{
		yield return new WaitForSeconds(0.5f);
		iTween.Stop (gameObject, "scale");
		iTween.Stop (gameObject, "position");
		yield return new WaitForSeconds(0.5f);
		gameObject.transform.localScale = new Vector3 (2,2,2);
		gameObject.transform.position = new Vector3 (0,0,0);
	}

	private int GetWeightedRandomAngle()
	{
		if(targetList.Count <= 0)
		{
			//Fill list with angle id's, if singletarget add it twice.
			for(int i = 1; i <= 10; i++)
			{
				if(phase1States.GetAngleState(i) == Phase1States.States.SingleTarget)
				{
					targetList.Add(i);
					targetList.Add(i);
				}
				else
					targetList.Add(i);
			}
		}
		
		//Get random identifier & get angleID from list
		int angleID = targetList[Random.Range(0, targetList.Count)];
		//Remove from List
		targetList.Remove(angleID);
		//Return value
		return angleID;
	}
	
	//Begin at Specific Phase -----------------------------------
	private void BeginState_1()
	{
		phase1States.SetStates(Phase1States.States.SingleTarget);
		ChangeCenterState(State.awaitCenterClick);
	}
	
	private void BeginState_2()
	{
		for(int i = 1; i <= 10; i += 2)
		{
			sManager.SetLongestHit(i);
			phase1States.SetAngleState(i, Phase1States.States.SequentialTarget);
		}
		
		ChangeCenterState(State.awaitCenterClick);
	}
	
	private void BeginState_3()
	{
		for(int i = 1; i <= 10; i += 2)
		{
			phase1States.SetAngleState(i, Phase1States.States.MultipleTargets);
		}
		ChangeCenterState(State.awaitCenterClick);
	}
	
	private void BeginState_4()
	{
		phase = Phases.Phase2;
		ChangeCenterState(State.awaitTargetClick);
		sManager.Phase2Stage1();
		GameObject.Find("Phase2Center(Clone)").GetComponent<Phase2Behavior>().SetStageRight();
//		ChangeCenterState(State.awaitCenterClick);
	}

	private void BeginState_5()
	{
		phase = Phases.Phase2;
		ChangeCenterState(State.awaitTargetClick);
		sManager.Phase2Stage1();
		GameObject.Find("Phase2Center(Clone)").GetComponent<Phase2Behavior>().SetStageLeft();
	}

	private void BeginState_6()
	{
		phase = Phases.Phase2;
		ChangeCenterState(State.awaitTargetClick);
		sManager.Phase2Stage1();
		GameObject.Find("Phase2Center(Clone)").GetComponent<Phase2Behavior>().SetStageBoth();
	}

	private void BeginAtShortCut()
	{
		string stageToBeginAt = guiManager.GetStage();

		switch(stageToBeginAt)
		{
		case "Single Target":
			BeginState_1();
			break;
		case "Sequential Target":
			BeginState_2();
			break;
		case "Multi Target":
			BeginState_3();
			break;
		case "Identify Right":
			BeginState_4();
			break;
		case "Identify Left":
			BeginState_5();
			break;
		case "Identify Both":
			BeginState_6();
			break;
		}
	}
	//-----------------------------------------------------------

	private void NC_Play()
	{
		BeginAtShortCut();
		playModeActive = true;
	}

	private void NC_Pause()
	{
		playModeActive = false;
	}

	private void NC_Unpause()
	{
		playModeActive = true;
	}

	private void NC_Restart()
	{
		phase1States.ResetStates();
		calibrationCounter = 0;
		targetList.Clear();
		phase = Phases.Phase0;

		ChangeCenterState(State.awaitCenterClick);
	}		
	#endregion

	#region Subclasses
	[System.Serializable]
	public class Phase1States
	{
		public enum States {SingleTarget, SequentialTarget, MultipleTargets};

		private States[] stateArray = new States[10] {States.SingleTarget, States.SingleTarget, States.SingleTarget, 
													 States.SingleTarget, States.SingleTarget, States.SingleTarget, 
													 States.SingleTarget, States.SingleTarget, States.SingleTarget, 
													 States.SingleTarget};

		public void SetAngleState(int int1to10, States state)
		{
			var i = int1to10-1;
			stateArray[i] = state;
		}
		public States GetAngleState(int int1to10)
		{
			var i = int1to10-1;
			return stateArray[i];
		}

		public void ResetStates()
		{
			for(int i = 0; i < stateArray.Length; i++)
			{
				stateArray[i] = States.SingleTarget;
			}
		}

		public void SetStates(States state)
		{
			for(int i = 0; i < stateArray.Length; i++)
			{
				stateArray[i] = state;
			}
		}
	}
	#endregion
}