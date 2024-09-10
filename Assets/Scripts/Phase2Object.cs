using UnityEngine;
using System.Collections;

public class Phase2Object : MonoBehaviour 
{
	#region Editor Publics
	[SerializeField] private ObjectTypes objectType = ObjectTypes.P2_Right;
	[SerializeField] private int Lifetime = 2;
	[SerializeField] private GameObject ParticleObject;
	[SerializeField] private GameObject ParticleObject2;
	public enum ObjectTypes {P2_Right, P2_Left, P2_Both};
	#endregion


	#region Privates
	//Connectivity
	private GestureManager gManager;
	private SoundManager soundManager;
	private HighscoreManager highScoreManager;
	private GameStateManager gameManager;
	private Phase2Behavior phase2Center;
	private WriteToTXT txtWriter;

	//Object Information - Passed from spawner
	private int angle;
	private int objectID;
	private float distance;
	private float spawnTime;
	private float hitTime;
	public float reactiontime;
	private int angleID;
	private float artLifetime = 100.0f;

	//Variables
	private float disableTime = 0.3f;
	private float punchTime = 0.5f;
	private int punches = 0;

	//Colors
	private Color InvisibleColor = new Color(0,1.0f,0,0);
	private Color FullGreenColor = new Color(0.23f, 1.0f, 0.0f, 1.0f);
	private Color DisabledColor = new Color(1,1,1,1);

	//Bools
	private bool playModeActive = true;
	private bool isPunching = false;
	private bool activeTarget = false;

	//Enums
	private enum HitType {Calibration, Hit, LateHit, Expired};
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

		highScoreManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();
		if(highScoreManager == null)
			Debug.LogError("No HighscoreManager was found in the scene.");

		gameManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
		phase2Center = GameObject.Find("Phase2Center(Clone)").GetComponent<Phase2Behavior>();

		txtWriter = GameObject.Find("WriteToTXT").GetComponent<WriteToTXT>();
		if(txtWriter == null)
			Debug.LogError("No txtWriter was found in the scene.");
	}

	void Start ()
	{
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");

		CalculatePunches();

		gameObject.GetComponent<Renderer>().material.color = DisabledColor;

		gameObject.transform.localScale = Vector3.zero;

		gManager.OnTapBegan += Hit;

		FadeIn();
	}

	void Update()
	{
		if(playModeActive && activeTarget)
		{
			//Should we punch?
			if(isPunching == false && Time.time < spawnTime + punchTime * punches)
			{
				PunchObject();
			}
			//should we record a miss? 
			else if(isPunching == false && Time.time >= spawnTime + Lifetime - disableTime)
			{
				Miss();
			}
		}
	}

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

	public int GetMultiplier()
	{
		return angleID;
	}

	public void SetMeanReactionTime(float reactionTime)
	{
		artLifetime = reactionTime;
	}

	private void FadeIn()
	{
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutBack, "time", 0.3f));
	}

	#region Class Methods
	private void CalculatePunches()
	{
		punches = (int) ((Lifetime - punchTime - disableTime) / (punchTime));
	}

	private void Hit(Vector2 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
		RaycastHit hitInfo;
		if(Physics.Raycast(ray, out hitInfo))
		{
			if(hitInfo.collider == gameObject.GetComponent<Collider>() && activeTarget && playModeActive)
			{
				
				HitType hitType;

				//If user didn't press within his average reaction time, note that it was a LateHit in logging
				if(Time.time - spawnTime <= artLifetime)
				{
					hitType = HitType.Hit;
				}
				else
				{
					hitType = HitType.LateHit;
				}

				//soundManager.PlayTouchEnded();
				soundManager.PlayTargetSuccessHit();

				SpawnParticle();
				gameManager.StartCoroutine("SpawnCenterExplosion", -transform.up);

				SetTargetDisabled();

				CalculateReactionTime();

				highScoreManager.AddScore(13, true);
				highScoreManager.IncreaseMultiplier();
				
				txtWriter.LogData(objectType.ToString(), reactiontime, angle, distance, transform.position, screenPos, hitType.ToString(), objectID, angleID);
				highScoreManager.AddHit(angleID, reactiontime, distance);

				if(Time.time - spawnTime <= artLifetime * 2.5f) //Change to 2.5f before delivery!
				{
					phase2Center.SendHit();
				}
				else
				{
					SetTargetDisabled();
					phase2Center.SendMiss();
				}
			}
		}
	}

	public void SetObjectType(ObjectTypes _objecttype)
	{
		objectType = _objecttype;
	}

	public bool TargetIsActive()
	{
		return activeTarget;
	}

	public void CalculateReactionTime()
	{
		hitTime = Time.time;
		reactiontime = hitTime - spawnTime;
	}

	private void HideTarget()
	{
		iTween.ColorTo(gameObject, iTween.Hash("color", InvisibleColor, "time", 0.0f));
		Destroy(gameObject, 2);
	}

	public void SetTargetDisabled()
	{
		iTween.ColorTo(gameObject, iTween.Hash("color", DisabledColor, "time", disableTime));
		activeTarget = false;
	}

	public void SetActiveTarget()
	{
		iTween.ColorTo(gameObject, iTween.Hash("color", FullGreenColor, "time", 0.3f));
		activeTarget = true;
		SetSpawnTime(Time.time);
	}

	private void SpawnParticle()
	{
		Instantiate(ParticleObject, transform.position, transform.rotation);
		Instantiate(ParticleObject2, transform.position, transform.rotation);
	}

	private void Miss()
	{
		SetTargetDisabled();
		//highScoreManager.AddMiss(angleID, distance);
		highScoreManager.AddMiss(angleID, distance, Lifetime - disableTime);
		txtWriter.LogData(objectType.ToString(), 0, angle, distance, transform.position, new Vector2(0,0), HitType.Expired.ToString(), objectID, angleID);
		phase2Center.SendMiss();
	}

	private void Unsubscribe()
	{
		gManager.OnTapBegan -= Hit;
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

	private void NC_Restart()
	{
		Unsubscribe();
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
}
