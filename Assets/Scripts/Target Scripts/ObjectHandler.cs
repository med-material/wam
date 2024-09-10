using UnityEngine;
using System.Collections;

public class ObjectHandler : MonoBehaviour 
{
	#region Editor Publics
	[SerializeField] private ObjectTypes objectType = ObjectTypes.SingleTarget;
	[SerializeField] private float Lifetime = 2;
	[SerializeField] private Particles particles;
	#endregion

	#region Privates
	//Connectivity & References
	private GestureManager gManager;
	private SpawnManager sManager;
	private SoundManager soundManager;
	private HighscoreManager highScoreManager;
	private GameStateManager gameManager;
	private WriteToTXT txtWriter;

	//Object Information - Passed from spawner
	private int angle;
	public int objectID;
	private float distance;
	private float spawnTime;
	private float hitTime;
	private float reactiontime;
	private int angleID;

	private float artLifetime = 100.0f;

	//Animation times and punch count
	private float fadeInTime = 0.3f;
	private float fadeOutTime = 0.3f;
	private float punchTime = 0.5f;
	private int punches = 0;

	//Colors
	private Color InvisibleColor = new Color(0,1.0f,0,0);
	private Color FullGreenColor = new Color(0.23f, 1.0f, 0.0f, 1.0f);
	private Color FullBlueColor = Color.cyan; // new Color(0.0f, 0.0f, 1.0f, 1.0f);
	private Color DisabledColor = new Color(1,1,1,1);

	//Flags
	private bool playModeActive = true;
	private bool isPunching = true;

	[HideInInspector] public enum ObjectTypes {SingleTarget, SequentialTarget, MultiTarget, SequentialTarget2, MultiTarget2, MultiTarget3};
	[HideInInspector] public enum HitType {Calibration, Hit, LateHit, Expired};
	#endregion
	
	void Awake()
	{
		//Get references
		soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		if(soundManager == null)
			Debug.LogError("No SoundManager was found in the scene.");

		gManager = Camera.main.GetComponent<GestureManager>();
		if(gManager == null)
			Debug.LogError("No GestureManager was found on the main camera.");
		
		sManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
		if(sManager == null)
			Debug.LogError("No SpawnManager was found in the scene.");

		highScoreManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();
		if(highScoreManager == null)
			Debug.LogError("No HighscoreManager was found in the scene.");

		gameManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();

		txtWriter = GameObject.Find("WriteToTXT").GetComponent<WriteToTXT>();
		if(txtWriter == null)
			Debug.LogError("No txtWriter was found in the scene.");

	}

	void Start ()
	{
		//Initialise object at scale zero - invisible
		gameObject.transform.localScale = Vector3.zero;

		//Initialise object with default color.
		if(objectType == ObjectTypes.SingleTarget)
			GetComponent<Renderer>().material.color = FullGreenColor;
		else if(objectType == ObjectTypes.SequentialTarget)
			GetComponent<Renderer>().material.color = FullGreenColor;
			//renderer.material.color = FullBlueColor;

		//Subscribe to gestureManager
		gManager.OnTapBegan += Hit;

		//Calculate Punches
		CalculatePunches();

		//Subscribe to observable events
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");

		FadeIn();
	}

	void Update()
	{
		if(playModeActive)
		{
			//Should we punch?
			if(isPunching == false && Time.time < spawnTime + fadeInTime + punchTime * punches)
			{
				PunchObject();
			}
			//should we record a miss? 
			else if(isPunching == false && Time.time >= spawnTime + Lifetime - fadeOutTime)
			{
				Miss();
				print (Lifetime);
			}
		}
	}
	
	#region Public Methods
	public void SetAngle(int degrees)
	{
		angle = degrees;
	}
	public int GetAngle()
	{
		return angle;
	}
	
	public void SetID(int ID)
	{
		objectID = ID;
	}
	
	public void SetDistance(float _distance)
	{
		distance = _distance;
	}
	
	public void SetSpawnTime(float time)
	{
		spawnTime = time;
	}
	
	public void SetMultiplier(int multiplier)
	{
		angleID = multiplier;
	}
	public int GetAngleID()
	{
		return angleID;
	}

	public void SetMeanReactionTime(float reactionTime)
	{
		artLifetime = reactionTime;
	}
	#endregion

	#region Class Methods
	private void CalculatePunches()
	{
		punches = (int) ((Lifetime - fadeInTime - fadeOutTime) / (punchTime));
	}

	private void FadeIn()
	{
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutBack, "time", 0.3f, 
		                                       "oncomplete", "AllowPunching", "oncompletetarget", gameObject));
	}

	private void FadeOut(float fadeTime)
	{
		iTween.ColorTo(gameObject, iTween.Hash("color", InvisibleColor,"easetype", iTween.EaseType.easeInQuint, "time", fadeTime));
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.zero, "easetype", iTween.EaseType.easeInQuint, "time", fadeTime, 
		                                       "oncomplete", "DestroySelf", "oncompletetarget", gameObject));
	}

	private void PunchObject()
	{
		isPunching = true;

		Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

		iTween.PunchScale(gameObject, iTween.Hash("amount", scale, "time", punchTime, 
		                                          "oncomplete", "AllowPunching", "oncompletetarget", gameObject));
	}

	private void AllowPunching()
	{
		isPunching = false;
	}

	private void DestroySelf()
	{
		soundManager.PlayMissed();
		Destroy(gameObject);
	}

	private void Hit(Vector2 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
		RaycastHit hitInfo;
		if(Physics.Raycast(ray, out hitInfo))
		{
			if(hitInfo.collider == gameObject.GetComponent<Collider>() && playModeActive)
			{
				//soundManager.PlayTouchEnded();
				soundManager.PlayTargetSuccessHit();

				HitType hitType = HitType.Calibration;
				int multiCount = 0;
				
				//If we are not still calibrating
				if(artLifetime < 100.0f)
				{
					//If user didn't press within his average reaction time, note that it was a LateHit in logging
					if(Time.time - spawnTime <= artLifetime)
					{
						hitType = HitType.Hit;
					}
					else
					{
						hitType = HitType.LateHit;
					}
				}

				//Do stuff depending on object type
				if(objectType == ObjectTypes.SingleTarget || objectType == ObjectTypes.SequentialTarget2)
				{
					gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);

					//If we are not still calibrating
					if(artLifetime < 100.0f)
					{
						//If user didn't press within his average reaction time, Report a miss to the adaptation system
						if(Time.time - spawnTime <= artLifetime)
						{
							sManager.ReportHit(angleID, distance);
						}
						else
						{
							sManager.ReportMiss(angleID, distance);
						}
					}
					
					SpawnParticle(particles.SingleExplosion);
					SpawnParticle(particles.CenterChaser);
					gameManager.StartCoroutine("SpawnCenterExplosion", -transform.up);
				}
				else if(objectType == ObjectTypes.SequentialTarget)
				{
					sManager.Phase1Stage2(angleID);
					SpawnParticle(particles.SequentialExplosion);
				}
				else if(objectType == ObjectTypes.MultiTarget)
				{
					multiCount = gameManager.GetMultiTargetCount();

					if(multiCount == 1)
						objectType = ObjectTypes.MultiTarget2;
					else if(multiCount == 2)
						objectType = ObjectTypes.MultiTarget3;

					gameManager.IncreaseMultiTargetCounter();
					SpawnParticle(particles.SingleExplosion);
					SpawnParticle(particles.CenterChaser);
					gameManager.StartCoroutine("SpawnCenterExplosion", -transform.up);
				}

				CalculateReactionTime();

				txtWriter.LogData(objectType.ToString(), reactiontime, angle, distance, transform.position, screenPos, hitType.ToString(), objectID, angleID);

				gameManager.lastHitTime = Time.time;

				highScoreManager.AddHit(angleID, reactiontime, distance);

				Unsubscribe();
				Destroy(gameObject);
			}
		}
	}
	
	private void Miss()
	{
		//highScoreManager.AddMiss(angleID, distance);
		highScoreManager.AddMiss(angleID, distance, Lifetime-fadeOutTime);
		isPunching = true;
		Unsubscribe();

		if(objectType == ObjectTypes.MultiTarget)
			gameManager.IncreaseMultiTargetCounter();
		else
			gameManager.ChangeCenterState(GameStateManager.State.awaitCenterClick);

		CalculateReactionTime();

		txtWriter.LogData(objectType.ToString(), reactiontime, angle, distance, transform.position, new Vector2(0,0), HitType.Expired.ToString(), objectID, angleID);
		sManager.ReportMiss(angleID, distance);
		FadeOut(fadeOutTime);
	}

	public float CalculateReactionTime()
	{
		hitTime = Time.time;
		reactiontime = hitTime - spawnTime;

		return reactiontime;
	}

	private void SetTargetDisabled()
	{
		iTween.ColorTo(gameObject, iTween.Hash("color", DisabledColor, "time", 0.3f));
	}

	private void SpawnParticle(GameObject particleObject)
	{
		Instantiate(particleObject, transform.position, transform.rotation);
	}

	private void Unsubscribe()
	{
		gManager.OnTapBegan -= Hit;
	}

	private void NC_Restart()
	{
		Miss();
	}

	private void NC_Pause()
	{
		playModeActive = false;
	}

	private void NC_Unpause()
	{
		playModeActive = true;
	}
	#endregion
	
	#region Subclasses
	[System.Serializable]
	public class Particles
	{
		public GameObject CenterChaser;
		public GameObject SingleExplosion;
		public GameObject SequentialExplosion;
	}
	#endregion
}
