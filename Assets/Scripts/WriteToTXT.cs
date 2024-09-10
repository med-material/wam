using UnityEngine;
using System.Collections;
using System.IO;

public class WriteToTXT : MonoBehaviour {

	private GUIManager gManager;
	private GameTimerManager gtManager;

	private string directorypath;
	private string filename;

	private string currentStringToWrite;

	private int userID;					
	private int sessionID;
	private int targetID;
	private int angleID;
	private string stage;				
	private string time;				
	private string reactiontime;
	private string angle;
	private string distance;
	private string targetpositionX;
	private string targetpositionY;
	private string touchpositionX;
	private string touchpositionY;
	private string hitType;

	private bool isFileCreated = false;

	// Use this for initialization
	void Start () {
		gManager = GameObject.Find("3DGUICamera").GetComponent<GUIManager>();
		gtManager = GameObject.Find("GameTimerManager").GetComponent<GameTimerManager>();
		directorypath = Application.persistentDataPath+"/Data/";
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private void CreateDataFolder()
	{
		if(!Directory.Exists(directorypath)){
			Directory.CreateDirectory(directorypath);
		}
	}

	private void CreateNewTXTFile(){
		/* string userPrefix = "User" + gManager.GetUserID();
		filename = userPrefix+" - "+System.DateTime.Now.ToString()+".txt";
		filename = filename.Replace("/","-");
		filename = filename.Replace(":","-");
		isFileCreated = true; */

//		using (StreamWriter writer = File.AppendText(directorypath + filename))
//		{
//			writer.WriteLine("userID;sessionID;targetID;stage;stageID;hitTime;reactionTime;angleID;angle;distance;targetPositionX;targetPositionY;touchPositionX;touchPositionY;hitType;hitTypeID");
//		}
	}

	public void UpdateSessionID(int _userID)
	{
		string userID = "User"+_userID;
		//Check for userID and sessionID in playerPrefs
		if(PlayerPrefs.HasKey(userID))
			sessionID = PlayerPrefs.GetInt(userID) + 1;
		
		//Update userID and sessionID in playerPrefs
		PlayerPrefs.SetInt(userID, sessionID);
	}

	public void LogData(string _stage, float _reactiontime, int _angle, float _distance, 
	                    Vector3 _targetposition, Vector2 _touchposition, string _hitType, int _targetID, int _angleID)
	{
		userID = gManager.GetUserID();
		targetID = _targetID;
		angleID = _angleID;
		stage = _stage;
		hitType = _hitType;
		time = gtManager.GetCurrentPlayTime().ToString("#.000");

		if(_reactiontime < 1)
		{
			reactiontime = "0"+_reactiontime.ToString("#.000");
		}
		else
		{
			reactiontime = _reactiontime.ToString("#.000");
		}

		angle = _angle.ToString("#.00");
		distance = _distance.ToString("#.00");

		Vector3 tempVector = Camera.main.WorldToViewportPoint(_targetposition);
		targetpositionX = tempVector.x.ToString();
		targetpositionY = tempVector.y.ToString();

		_touchposition = Camera.main.ScreenToViewportPoint (_touchposition);

		touchpositionX = _touchposition.x.ToString();
		touchpositionY = _touchposition.y.ToString();

		currentStringToWrite = ""+userID+";"+System.DateTime.Now.Date.ToString()+";"+System.DateTime.Now.TimeOfDay.ToString()+";"+sessionID+";"+targetID+";"+stage+";"+GetStageID(stage)+";"+time+";"+reactiontime+";"+angleID+";"+angle+";"+distance+";"+targetpositionX+";"+targetpositionY+";"+touchpositionX+";"+touchpositionY+";"+hitType+";"+GetHitTypeID(hitType);
		WriteTXT();
	}

	private int GetStageID(string stage)
	{
		switch(stage)
		{
		case "SingleTarget":
			return 0;
		case "SequentialTarget":
			return 1;
		case "SequentialTarget2":
			return 2;
		case "MultiTarget":
			return 3;
		case "MultiTarget2":
			return 4;
		case "MultiTarget3":
			return 5;
		case "P2_Right":
			return 6;
		case "P2_Left":
			return 7;
		case "P2_Both":
			return 8;
		case "Center":
			return 9;
		default:
			return -1;
		}
	}

	private int GetHitTypeID(string hitType)
	{
		switch(hitType)
		{
		case "Calibration":
			return 0;
		case "Hit":
			return 1;
		case "LateHit":
			return 2;
		case "Expired":
			return 3;
		case "CenterHit":
			return 4;
		case "CenterError":
			return -2;
		default:
			return -1;
		}
	}

	private void WriteTXT()
	{
		if(isFileCreated)
		{
			using (StreamWriter writer = File.AppendText(directorypath + filename))
			{
				writer.WriteLine(currentStringToWrite);
			}
		}
	}

	//Method that submits data to the mysql server. //TODO: test if works
	string phpScript_URL = "https://dl.dropboxusercontent.com/u/4419164/myScript.php";
	IEnumerator SubmitEntry()
	{
		Debug.Log("Trying to submit entry");
		// Create a form object for sending high score data to the server
		WWWForm form = new WWWForm();
		//Fill in information
		form.AddField("userID", userID);
		form.AddField("sessionID", sessionID);
		form.AddField("targetID", targetID);
		form.AddField("stage", stage);
		form.AddField("hitTime", time);
		form.AddField("reactionTime", reactiontime);
		form.AddField("angleID", angleID);
		form.AddField("angle", angle);
		form.AddField("distance", distance);
		form.AddField("targetpositionX", targetpositionX);
		form.AddField("targetpositionY", targetpositionY);
		form.AddField("touchpositionX", touchpositionX);
		form.AddField("touchpositionY", touchpositionY);
		form.AddField("hitType", hitType);
		//Submit the information
		WWW submit = new WWW(phpScript_URL, form);

		yield return submit;

		if(!string.IsNullOrEmpty(submit.error))
			Debug.Log("Error occured submitting data entry: "+submit.error);
		else
			Debug.Log("Succesfully submitted data entry");
	}

	private void NC_Play()
	{
		CreateDataFolder();
		CreateNewTXTFile();
	}
}
