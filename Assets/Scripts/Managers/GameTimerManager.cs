using UnityEngine;
using System.Collections;

public class GameTimerManager : MonoBehaviour {

	#region Editor Publics
	[SerializeField] private bool EnableTimer = false;
	[SerializeField] private bool ForcePhase2 = true;
	[SerializeField] public float maxTimeInMinutes = 0.15f;
	#endregion

	#region Privates
	private GameStateManager gameManager;
	private SpawnManager sManager;
	private HighscoreManager hsManager;

	public float maxTime = 0;
	private float twoThirdsTime = 0;
	private float StartTime = 0;
	private float currentTimePlayed = 0;

	private float pauseOffset = 0;
	private float pauseStartTime = 0;
	private float pauseEndTime = 0;

	private bool gameOver = false;
	private bool gameRunning = false;
	private bool phaseChanged = false;
	#endregion

	// Use this for initialization
	void Start () {
		gameManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();

		sManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
		if(sManager == null)
			Debug.LogError("No SpawnManager was found in the scene.");

		hsManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();
		if(hsManager == null)
			Debug.LogError("No Highscore Manager was found in the scene.");

		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");

		maxTime = 60*maxTimeInMinutes;
		twoThirdsTime = (maxTime / 3.0f) * 2.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(gameRunning && !gameOver)
		{
			currentTimePlayed = Time.time - StartTime - pauseOffset;
		}

		if(currentTimePlayed > maxTime && !gameOver)
		{
			if(EnableTimer == true)
				OutOfTime();
		}

		if(ForcePhase2 == true)
		{
			if(phaseChanged == false && currentTimePlayed > twoThirdsTime && !gameOver)
			{
				gameManager.SetPhase(GameStateManager.Phases.Phase2);
				phaseChanged = true;
			}
		}
	}

	public void NewMaxTime(float minutes) {
		maxTime = 60*minutes;
		twoThirdsTime = (maxTime / 3.0f) * 2.0f;
	}

	public float GetCurrentPlayTime()
	{
		return currentTimePlayed;
	}

	private void OutOfTime()
	{
		NotificationCenter.DefaultCenter().PostNotification(this, "NC_Pause");
		NotificationCenter.DefaultCenter().PostNotification(this, "NC_GameOver");
		gameOver = true;
		gameRunning = false;
		//guiManager.OutOfTimeReturnToMenu();
	}

	private void StartGameTimer()
	{
		StartTime = Time.time;
		gameManager.lastHitTime = StartTime;
		gameRunning = true;
		phaseChanged = false;
		//sManager.objectCounter = 0;
	}
	
	private void PauseGameTimer()
	{
		pauseStartTime = Time.time;
		gameRunning = false;
	}
	
	private void UnpauseGameTimer()
	{
		pauseEndTime = Time.time;
		gameRunning = true;
		pauseOffset += pauseEndTime - pauseStartTime;
	}
	
	private void RestartGameTimer()
	{
		pauseOffset = 0;
		currentTimePlayed = 0;
		gameOver = false;
	}

	private float GetPauseOFfset()
	{
		return pauseOffset;
	}
	
	private void NC_Restart()
	{
		RestartGameTimer();
	}
	
	private void NC_Play()
	{
		StartGameTimer();
	}
	
	private void NC_Pause()
	{
		PauseGameTimer();
	}
	
	private void NC_Unpause()
	{
		UnpauseGameTimer();
	}

//	void OnGUI()
//	{
//		/*if(gameRunning)
//			GUI.Label(new Rect(50, 50, 200, 200), ""+currentTimePlayed.ToString("0"));*/
//
//		/*if(gameOver)
//			GUI.Label(new Rect(200, 200, 200, 200), "S");*/
//	}
}
