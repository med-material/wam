using UnityEngine;
using System.Collections;

public class GUIManager: MonoBehaviour {
	
	#region Editor Publics
	[SerializeField] private string[] Users = {"User 1", "User 2", "User 3", "User 4", "User 5", "User 6", "User 7", "User 8", "User 9", "User 10"};
	[SerializeField] private float curtainSpeed = 1.0f;
	[SerializeField] private iTween.EaseType curtainEasetype = iTween.EaseType.easeOutCirc;
	[SerializeField] private GUIStyle guiLook;
	[SerializeField] private GUIStyles guiStyles;
	[SerializeField] private GUIElements guiElements;
	[SerializeField] private Vector2 windowMetrics;
	#endregion
	
	#region Privates
	//Connectivity
	private HighscoreManager scoreManager;
	private WriteToTXT txtLogger;
	private EndGameLines endGameLines;
	private GameTimerManager timerManager;

	//Rects
	private Rect highscoreRect;
	private Rect windowRect;
	private Rect endWindowRect;
	private Rect countdownRect;
	private Rect userSelectRect;

	//Conditioning
	public GUIBools guiBools = new GUIBools();

	//Variables
	private Vector3 LeftCoverBeginPos;
	private Vector3 RightCoverBeginPos;

	//Strings
	private string currentUser;
	private int CurrentUserID = -1;
	private string currentCountdownNumber;
	private string currentStage;

	private int hits;
	private int misses;
	private string avgReactionTime;

	private bool gameOver = false;
	private bool showingReactionTimes = false;

	private string inputID = "Patient";
	private string inputPsw = "Kodeord";

	public GUIStyle nameStyle;
	public string psw;

	private float fontScale;

	private bool[] deleteData = {false,false,false};

	public int maxMinutes;
	public int minMinutes;
	public int defaultMinutes;
	private int nextSessionLength;
	private string displayMinutes;
	public	int coloumns = 2;

	private bool showCurves = false;

	#endregion

	void Start()
	{
//		PlayerPrefs.DeleteAll();
		//Connectivity
		scoreManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();
		endGameLines = GameObject.Find("EndGameLines").GetComponent<EndGameLines>();
		txtLogger = GameObject.Find("WriteToTXT").GetComponent<WriteToTXT>();
		timerManager = GameObject.Find("GameTimerManager").GetComponent<GameTimerManager>();
		//Rect initialization
		highscoreRect = new Rect(0, 0, 100, 80);
		highscoreRect.center = new Vector2(GetCenterWidth(), 20);
		windowRect = new Rect(0,0, Screen.width * windowMetrics.x, Screen.height * windowMetrics.y);
		windowRect.center = new Vector2(GetCenterWidth(), getCenterHeight());

		userSelectRect = new Rect(0,0, Screen.width * 0.5f, Screen.height * ((Users.Length/coloumns)*0.13f));
		userSelectRect.center = new Vector2(GetCenterWidth(), getCenterHeight());

		endWindowRect = new Rect(0,0, Screen.width * 0.52f, Screen.height * 0.88f);
		endWindowRect.center = new Vector2(GetCenterWidth(), getCenterHeight());
		countdownRect = new Rect(0,0,150,150);
		countdownRect.center = new Vector2(GetCenterWidth(), getCenterHeight());

		LeftCoverBeginPos = guiElements.LeftCover.transform.position;
		RightCoverBeginPos = guiElements.RightCover.transform.position;

		NotificationCenter.DefaultCenter().AddObserver(this, "NC_GameOver");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");

		fontScale = Screen.height / 768f;

		guiStyles.WindowStyle.fontSize = (int)(guiStyles.WindowStyle.fontSize * fontScale);
		guiStyles.WindowFrameStyle.fontSize = (int)(guiStyles.WindowFrameStyle.fontSize * fontScale);
		guiStyles.WindowEndFrameStyle.fontSize = (int)(guiStyles.WindowEndFrameStyle.fontSize * fontScale);
		guiStyles.WindowLabelStyle.fontSize = (int)(guiStyles.WindowLabelStyle.fontSize * fontScale);
		guiStyles.WindowScoreLabelStyle.fontSize = (int)(guiStyles.WindowScoreLabelStyle.fontSize * fontScale);
		guiStyles.HighscoreStyle.fontSize = (int)(guiStyles.HighscoreStyle.fontSize * fontScale);
		guiStyles.CountdownStyle.fontSize = (int)(guiStyles.CountdownStyle.fontSize * fontScale);
		guiStyles.SmallButton.fontSize = (int)(guiStyles.SmallButton.fontSize * fontScale);
		guiStyles.SmallButtonOn.fontSize = (int)(guiStyles.SmallButtonOn.fontSize * fontScale);

		for(int i = 0; i < Users.Length; i++){
			if(PlayerPrefs.HasKey("UserName" + i.ToString())) {
				Users[i] = PlayerPrefs.GetString("UserName" + i.ToString());
			}
			else {
				PlayerPrefs.SetString("UserName" + i.ToString(), "[Ny Bruger]");
				Users[i] = "[Ny Bruger]";
			}
		}
	}

	void OnGUI()
	{

		if(guiBools.displayUserSelection == true)
		{
			userSelectRect = GUILayout.Window(0, userSelectRect, DoUserSelectionWindow, "", guiStyles.WindowFrameStyle);
		}

		if(guiBools.displayStageSelection == true)
		{
			windowRect = GUILayout.Window(0, windowRect, DoStageSelectionWindow, "", guiStyles.WindowFrameStyle);
		}

		if(guiBools.displayHighscore == true)
		{			
			GUI.Label(highscoreRect, ""+scoreManager.GetScore(), guiLook);
		}

		if(guiBools.displayExitConfirmation == true)
		{
			windowRect = GUILayout.Window(0, windowRect, DoExitConfirmationWindow, "", guiStyles.WindowFrameStyle);
		}

		if(guiBools.displayPlayPrompt)
		{
			windowRect = GUILayout.Window(0, windowRect, DoPlayPromptWindow, "", guiStyles.WindowFrameStyle);

			inputID = GUI.TextField(new Rect(10*fontScale,10*fontScale,200*fontScale,50*fontScale), inputID, guiStyles.WindowStyle);

			//inputPsw = GUI.TextField(new Rect(10*fontScale,65*fontScale,200*fontScale,50*fontScale), inputPsw, guiStyles.WindowStyle);
			
			if(GUI.Button(new Rect(10*fontScale,65*fontScale,200*fontScale,50*fontScale), "Skift Navn", guiStyles.WindowStyle) && inputID != "") {

				PlayerPrefs.SetString("UserName" + CurrentUserID.ToString(), inputID);
				Users[CurrentUserID] = inputID;
			}

			if(GUI.Button(new Rect(10*fontScale,120*fontScale,200*fontScale,50*fontScale), "Slet Data", guiStyles.WindowStyle) && deleteData[0] && deleteData[1] && deleteData[2]) {

				scoreManager.ClearScore();
				deleteData[0] = false;
				deleteData[1] = false;
				deleteData[2] = false;
			}

			GUI.Label(new Rect(10*fontScale,200*fontScale,200*fontScale,50*fontScale), "Spilletid: " + nextSessionLength.ToString() + " minutter", guiStyles.WindowLabelStyle);
						//GUI.skin.horizontalSlider.normal.background = defaultSkin.horizontalScrollbar.normal.background;
			//nextSessionLength = (int)GUI.HorizontalSlider(new Rect(10*fontScale,255*fontScale,200*fontScale,50*fontScale),nextSessionLength, minMinutes, maxMinutes);
			timerManager.maxTimeInMinutes = nextSessionLength;

						int tempMinutes;

						displayMinutes = GUI.TextField (new Rect (10 * fontScale, 255 * fontScale, 200 * fontScale, 50 * fontScale), displayMinutes, guiStyles.WindowStyle);
			
						if (int.TryParse (displayMinutes, out tempMinutes)) {
								if (tempMinutes >= minMinutes)
										nextSessionLength = tempMinutes;
						}

						GUI.Label(new Rect(10*fontScale,310*fontScale,200*fontScale,50*fontScale), "(Minimum " + minMinutes.ToString() + " minutter)", guiStyles.WindowLabelStyle);
						//nextSessionLength = int.Parse(GUI.TextField(new Rect(10*fontScale,255*fontScale,200*fontScale,50*fontScale), nextSessionLength.ToString(), guiStyles.WindowStyle));

			timerManager.NewMaxTime(nextSessionLength);

			PlayerPrefs.SetInt("SessionLength"+(CurrentUserID).ToString(), nextSessionLength);
			

						deleteData[0] = GUI.Toggle(new Rect(10, Screen.height-50, 40,40), deleteData[0],"", guiStyles.SmallButton);
						deleteData[1] = GUI.Toggle(new Rect(Screen.width-50, Screen.height-50, 40,40), deleteData[1],"", guiStyles.SmallButton);
						deleteData[2] = GUI.Toggle(new Rect(Screen.width-50, 10, 40,40), deleteData[2],"", guiStyles.SmallButton);
		}

		if(guiBools.displayCountDown)
		{
			GUI.Label(countdownRect, ""+currentCountdownNumber, guiStyles.CountdownStyle);
		}

		if(guiBools.displayEndScreen)
		{
			endWindowRect = GUILayout.Window(0, endWindowRect, DoEndScreenWindow, "", guiStyles.WindowEndFrameStyle);
		}

	}

	#region GUI Windows
	private void DoUserSelectionWindow(int windowID)
	{
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label("V\u00E6lg Bruger", guiStyles.WindowLabelStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		int counter = 0;

		int rows = Users.Length;

		GUILayout.Space(5);
		for(int i = 0; i < Users.Length; i+=coloumns){
			GUILayout.BeginHorizontal();
			GUILayout.Space(5);
			for(int j = 0; j < coloumns; j++){
				if(i+j >= Users.Length){
					break;
				}

				if(PlaceButton(Users[i+j], 0.1f, 0.1f))
				{
					guiBools.displayUserSelection = false;
					guiBools.displayPlayPrompt = true;
					//guiBools.displayStageSelection = true;
					currentUser = Users[i+j];
					CurrentUserID = i+j;
					inputID = currentUser;
					deleteData[0] = false;
					deleteData[1] = false;
					deleteData[2] = false;

					if(PlayerPrefs.HasKey("SessionLength"+(i+j).ToString())){
						nextSessionLength = PlayerPrefs.GetInt("SessionLength"+(i+j).ToString());
					}
					else{
						PlayerPrefs.SetInt("SessionLength"+(i+j).ToString(), defaultMinutes);
						nextSessionLength = defaultMinutes;
					}
										displayMinutes = nextSessionLength.ToString();
				}
				GUILayout.Space(5);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
		}

		//selectionGrid = GUILayout.SelectionGrid(selectionGrid, Users, 2, guiStyles.WindowStyle);

		/*if(PlaceButton("Afslut spillet"))
		{
			Application.Quit();
		}*/

	}

	private void DoStageSelectionWindow(int windowID)
	{
		if(PlaceButton("Single Target"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Single Target";
		}
		
		if(PlaceButton("Sequential Target"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Sequential Target";
		}
		
		if(PlaceButton("Multi Target"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Multi Target";
		}
		
		if(PlaceButton("Identify Right"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Identify Right";
		}
		
		if(PlaceButton("Identify Left"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Identify Left";
		}
		
		if(PlaceButton("Identify Both"))
		{
			guiBools.displayStageSelection = false;
			guiBools.displayPlayPrompt = true;
			currentStage = "Identify Both";
		}
	}

	private void DoExitConfirmationWindow(int windowID)
	{
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Forts\u00E6t eller afslut?", guiStyles.WindowLabelStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();

		if(!gameOver)
		{
			if(PlaceButton("Forts\u00E6t"))
			{
				guiBools.displayExitConfirmation = false;
				NotificationCenter.DefaultCenter().PostNotification(this, "NC_Unpause");
			}
		}

		/*if(PlaceButton("Exit Game))
		{
			//TODO: Pause Game
			//TODO: Save, send, log Data
			Application.Quit();
		}*/

		GUILayout.Space(5);

		if(PlaceButton("Tilbage Til Menuen"))
		{
			guiBools.displayExitConfirmation = false;
			guiBools.displayUserSelection = true;
			inputPsw = "Kodeord";
			BlockAll(true);
			EnableHighscore(false);
			NotificationCenter.DefaultCenter().PostNotification(this, "NC_Restart");
		}
	}

	private void DoPlayPromptWindow(int windowID)
	{
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Hej "+Users[CurrentUserID], guiStyles.WindowLabelStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Er du klar?", guiStyles.WindowLabelStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();

		if(PlaceButton("Spil"))
		{
			guiBools.displayPlayPrompt = false;
			txtLogger.UpdateSessionID(CurrentUserID);
//			EnableHighscore(true);
			StartCoroutine(StartCountDown());
		}

		GUILayout.Space(5);

		if(PlaceButton("Resultater") && scoreManager.CheckScoreFile(CurrentUserID))
		{
			guiBools.displayPlayPrompt = false;
			guiBools.displayEndScreen = true;
			scoreManager.LoadScores();

			if(showCurves)
				endGameLines.DoCurves();
			else
				endGameLines.DoHitMissScreen();

			inputPsw = "Kodeord";
			hits = scoreManager.GetHitCount(endGameLines.scoreEntry);
			misses = scoreManager.GetMissCount(endGameLines.scoreEntry);
						avgReactionTime = scoreManager.GetReactionMeanFloat(endGameLines.scoreEntry)+"s";
						deleteData[0] = false;
						deleteData[1] = false;
						deleteData[2] = false;
		}

		GUILayout.Space(5);

		if(PlaceButton("V\u00E6lg Spiller"))
		{
			guiBools.displayPlayPrompt = false;
			guiBools.displayUserSelection = true;
			inputPsw = "Kodeord";
		}
	}

	private void DoEndScreenWindow(int windowID)
	{
		if(!showCurves){
		GUILayout.BeginHorizontal();

		if(!showingReactionTimes)
		{
			if(PlaceSmallToggleButton("Tr\u00E6ffere og Missere", guiStyles.SmallButtonOn))
			{
				showingReactionTimes = false;

				if(showCurves){

				}
				else {
					endGameLines.DisableEndScreen();
					endGameLines.DoHitMissScreen();
				}
			}
		}
		else if(showingReactionTimes)
		{
			if(PlaceSmallToggleButton("Tr\u00E6ffere og Missere", guiStyles.SmallButton))
			{
				showingReactionTimes = false;
				
				if(showCurves){

				}
				else {
					endGameLines.DisableEndScreen();
					endGameLines.DoHitMissScreen();
				}
			}
		}

		GUILayout.Space(5);

		if(!showingReactionTimes)
		{
			if(PlaceSmallToggleButton("Reaktionstid", guiStyles.SmallButton))
			{
				showingReactionTimes = true;
				
				if(showCurves){

				}
				else {
					endGameLines.DisableEndScreen();
					endGameLines.DoReactionScreen();
				}
			}
		}
		else if(showingReactionTimes)
		{
			if(PlaceSmallToggleButton("Reaktionstid", guiStyles.SmallButtonOn))
			{
				showingReactionTimes = true;
				
				if(showCurves){

				}
				else {
					endGameLines.DisableEndScreen();
					endGameLines.DoReactionScreen();
				}
			}
		}
		GUILayout.EndHorizontal();
		}
		else{
			GUILayout.Space(70);
		}

		GUILayout.Space(15);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();


		if(showCurves)
		{
			GUILayout.Label("Center of Hit", guiStyles.WindowLabelStyle);
		}
		else if(!showingReactionTimes)
		{
			GUILayout.Label("Tr\u00E6ffere og Missere", guiStyles.WindowLabelStyle);
		}
		else if(showingReactionTimes)
		{
			GUILayout.Label("Reaktionstid", guiStyles.WindowLabelStyle);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(15);

		
		GUILayout.Label("Dato og Tid:", guiStyles.WindowScoreLabelStyle);
		GUILayout.Space(5);
		GUILayout.Label(scoreManager.GetDate(endGameLines.scoreEntry), guiStyles.WindowScoreLabelStyle);

		GUILayout.Space(15);

		GUILayout.Label("Navn:", guiStyles.WindowScoreLabelStyle);
		GUILayout.Space(5);
		GUILayout.Label(scoreManager.GetName(endGameLines.scoreEntry), guiStyles.WindowScoreLabelStyle);

		if(!showCurves){
		GUILayout.Space(15);
	
		if(!showingReactionTimes)
		{
			GUILayout.Label("Tr\u00E6ffere: "+hits, guiStyles.WindowScoreLabelStyle);
			
			GUILayout.Space(5);
			
			GUILayout.Label("Missere: "+ misses, guiStyles.WindowScoreLabelStyle);
		}
		else if(showingReactionTimes)
		{
			GUILayout.Label("Gennemsnit: ", guiStyles.WindowScoreLabelStyle);
			GUILayout.Space(5);
			GUILayout.Label(avgReactionTime, guiStyles.WindowScoreLabelStyle);
		}
		}

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		if(PlaceButton("< Forrige", 0.08f, 0.15f))
		{
			endGameLines.PreviousScores();
			hits = scoreManager.GetHitCount(endGameLines.scoreEntry);
			misses = scoreManager.GetMissCount(endGameLines.scoreEntry);
			avgReactionTime = scoreManager.GetReactionMeanFloat(endGameLines.scoreEntry)+"s";
		}
		GUILayout.Space(5);

		if(showCurves){
			if(PlaceButton("Alle", 0.08f, 0.15f))
			{
				if(showingReactionTimes) {
					endGameLines.DoReactionScreen();
				}
				else {
					endGameLines.DoHitMissScreen();
				}
				showCurves = false;
			}
		}
		else {
			if(PlaceButton("Gennemsnit", 0.08f, 0.15f))
			{
				endGameLines.DoCurves();

				if(showingReactionTimes) {

				}
				else {

				}
				showCurves = true;
			}
		}
		
		GUILayout.Space(5);
		if(PlaceButton("N\u00E6ste >", 0.08f, 0.15f))
		{
			endGameLines.NextScores();
			hits = scoreManager.GetHitCount(endGameLines.scoreEntry);
			misses = scoreManager.GetMissCount(endGameLines.scoreEntry);
			avgReactionTime = scoreManager.GetReactionMeanFloat(endGameLines.scoreEntry)+"s";
		}
		GUILayout.EndHorizontal();


		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		if(PlaceButton("Tilbage", 0.08f, 0.15f))
		{
			EnableEndScreen(false);
			guiBools.displayPlayPrompt = true;
			inputPsw = "Kodeord";
			BlockAll(true);
			EnableHighscore(false);
			endGameLines.scoreEntry = 0;
			//showCurves = false;
			NotificationCenter.DefaultCenter().PostNotification(this, "NC_Restart");
		}
		/*GUILayout.Space(5);
		if(PlaceButton("Afslut Spillet"))
		{
			Application.Quit();
		}*/
		GUILayout.EndHorizontal();
	}

	private IEnumerator StartCountDown()
	{
		guiBools.displayCountDown = true;

		//Slide the countdown to the right side of the screen over 1.5 seconds
		iTween.ValueTo(gameObject, iTween.Hash("from", countdownRect.x, "to", countdownRect.x + 150, "time", 1.5f, 
		                                       "onupdate", "UpdateCountdownRect"));
		currentCountdownNumber = "3";
		yield return new WaitForSeconds(1.0f);
		currentCountdownNumber = "2";
		yield return new WaitForSeconds(1.0f);
		currentCountdownNumber = "1";
		yield return new WaitForSeconds(1.0f);
		currentCountdownNumber = "";
		guiBools.displayCountDown = false;
		BlockRightHalf(false);
		NotificationCenter.DefaultCenter().PostNotification(this, "NC_Play");

	}

	private void UpdateCountdownRect(float itweenChange)
	{
		countdownRect.x = itweenChange;
	}

	#endregion


	#region Public Methods
	public int GetUserID()
	{
		return CurrentUserID;
	}

	public string GetUserName()
	{
		return Users[CurrentUserID];
	}

	//Highscore Logic-----------------------
	public void EnableHighscore(bool state)
	{
		if(state == true && guiBools.displayHighscore == false)
		{
			guiElements.Highscore.SetActive(true);
			guiBools.displayHighscore = true;
		}
		else if(state == false && guiBools.displayHighscore == true)
		{
			guiElements.Highscore.SetActive(false);
			guiBools.displayHighscore = false;
		}
	}

	public void EnableEndScreen(bool state)
	{
		//BlockAll(true);
		if(state == true)
		{
			hits = scoreManager.GetHitCount(endGameLines.scoreEntry);
			misses = scoreManager.GetMissCount(endGameLines.scoreEntry);
			avgReactionTime = scoreManager.GetReactionMeanFloat(endGameLines.scoreEntry)+"s";
			BlockAll(true);
			StartCoroutine(EndGameDelayedScreen(true));		
		}
		else if (state == false)
		{
			BlockAll(false);
			StartCoroutine(EndGameDelayedScreen(false));
		}
	}

	private IEnumerator EndGameDelayedScreen(bool state)
	{
		if(state == true)
		{
			yield return new WaitForSeconds(1.0f);
			guiBools.displayEndScreen = true;
			endGameLines.DoHitMissScreen();
		}
		else if (state == false)
		{
			yield return new WaitForSeconds(0.0f);
			guiBools.displayEndScreen = false;
			endGameLines.DisableEndScreen();
		}
	}

	//Curtain Logic-------------------------
	public void BlockLeftHalf(bool state)
	{
		if(state == true && guiBools.displayLeftHalf == true)
		{
			guiBools.displayLeftHalf = false;
			var newPosition = new Vector3(LeftCoverBeginPos.x, 
			                              LeftCoverBeginPos.y, 
			                              LeftCoverBeginPos.z);
			iTween.MoveTo(guiElements.LeftCover, iTween.Hash("position", newPosition, 
		                                                 	 "time", curtainSpeed, 
			                                                 "easetype", curtainEasetype));
		}
		else if(state == false && guiBools.displayLeftHalf == false)
		{
			guiBools.displayLeftHalf = true;
			var newPosition = new Vector3(LeftCoverBeginPos.x - 18.0f, 
			                              LeftCoverBeginPos.y, 
			                              LeftCoverBeginPos.z);
			iTween.MoveTo(guiElements.LeftCover, iTween.Hash("position", newPosition, 
			                                                 "time", curtainSpeed, 
			                                                 "easetype", curtainEasetype));
		}
	}

	public void BlockRightHalf(bool state)
	{
		if(state == true && guiBools.displayRightHalf == true)
		{
			guiBools.displayRightHalf = false;
			var newPosition = new Vector3(RightCoverBeginPos.x, 
			                              RightCoverBeginPos.y, 
			                              RightCoverBeginPos.z);
			iTween.MoveTo(guiElements.RightCover, iTween.Hash("position", newPosition, 
			                                                  "time", curtainSpeed, 
			                                                  "easetype", curtainEasetype));
		}
		else if(state == false && guiBools.displayRightHalf == false)
		{
			guiBools.displayRightHalf = true;
			var newPosition = new Vector3(RightCoverBeginPos.x + 18.0f, 
			                              RightCoverBeginPos.y, 
			                              RightCoverBeginPos.z);
			iTween.MoveTo(guiElements.RightCover, iTween.Hash("position", newPosition, 
			                                                  "time", curtainSpeed, 
			                                                  "easetype", curtainEasetype));
		}
	}

	public void BlockAll(bool state){
		BlockLeftHalf(state);
		BlockRightHalf(state);
	}
	//-----------------------------------------------


	public void ExitConfirmation()
	{
		if(!gameOver){
			NotificationCenter.DefaultCenter().PostNotification(this, "NC_Pause");

			if(guiBools.displayExitConfirmation == false)
				guiBools.displayExitConfirmation = true;
		}
	}

	public string GetStage()
	{
		return currentStage;
	}

	#endregion

	
	#region Class Methods
	private int GetCenterWidth()
	{
		return Screen.width/2;
	}
	
	private int getCenterHeight()
	{
		return Screen.height/2;
	}
	
	private bool PlaceButton(string text)
	{
		if(GUILayout.Button(text, guiStyles.WindowStyle, GUILayout.MinHeight(Screen.height*0.15f), GUILayout.MinWidth(Screen.width*0.15f)))
			return (true);
		else
			return (false);
	}
	private bool PlaceButton(string text, float heightPercentage, float widthPercentage)
	{
		if(GUILayout.Button(text, guiStyles.WindowStyle, GUILayout.MinHeight(Screen.height*heightPercentage), GUILayout.MinWidth(Screen.width*widthPercentage)))
			return (true);
		else
			return (false);
	}

	private bool PlaceSmallToggleButton(string text, GUIStyle CurrentStyle)
	{
		if(GUILayout.Button(text, CurrentStyle, GUILayout.MinHeight(Screen.height*0.08f), GUILayout.MinWidth(Screen.width*0.15f)))
			return (true);
		else
			return (false);
	}

	private IEnumerator EndGame()
	{
		scoreManager.SaveScore();
		scoreManager.LoadScores();
		NotificationCenter.DefaultCenter().PostNotification(this, "NC_Pause");
		gameOver = true;
		BlockAll(true);
		yield return new WaitForSeconds(curtainSpeed);
		EnableEndScreen(true);
	}

	private void NC_GameOver()
	{
		StartCoroutine(EndGame());
	}

	private void NC_Restart()
	{
		countdownRect.center = new Vector2(GetCenterWidth(), getCenterHeight());
		gameOver = false;
		showingReactionTimes = false;
	}
	private void NC_Pause()
	{
		//Blocking here gives problems in phase2 since unpausing will remove both walls regardless of which stage.
		//BlockAll(true);
	}
	private void NC_Unpause()
	{
		//BlockAll(false);
	}

	#endregion

	#region Subclasses
	[System.Serializable]
	public class GUIElements
	{
		public GameObject Highscore;
		public GameObject LeftCover;
		public GameObject RightCover;
	}

	[System.Serializable]
	public class GUIBools
	{
		public bool displayUserSelection = true;
		public bool displayHighscore = false;
		public bool displayLeftHalf = false;
		public bool displayRightHalf = false;
		public bool displayStatistics = false;
		public bool displayPlayPrompt = false;
		public bool displayExitConfirmation = false;
		public bool displayCountDown = false;
		public bool displayStageSelection = false;
		public bool displayEndScreen = false;
	}

	[System.Serializable]
	public class GUIStyles
	{
		public GUIStyle WindowStyle;
		public GUIStyle WindowFrameStyle;
		public GUIStyle WindowEndFrameStyle;
		public GUIStyle WindowLabelStyle;
		public GUIStyle WindowScoreLabelStyle;
		public GUIStyle HighscoreStyle;
		public GUIStyle CountdownStyle;
		public GUIStyle SmallButton;
		public GUIStyle SmallButtonOn;
		public GUIStyle SliderStyle;
		public GUIStyle SliderThumbStyle;
	}
	#endregion
}
