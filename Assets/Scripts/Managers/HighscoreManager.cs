using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class HighscoreManager : MonoBehaviour {

	#region Editor Publics
	[SerializeField] private int calibrationSkipAmount = 3;
	#endregion

	#region Privates
	private int currentScore = 0;
	private int multiplier = 1;
	private int skipCounter = 0;

	private GUIManager guiManager;
	private int currentUser;

	//Reactiontime Lists
	private int angleCount = 10;
	private List<List<float>> rtList = new List<List<float>>();
	private List<List<float>> hitDistances = new List<List<float>>();
	private List<List<float>> missDistances = new List<List<float>>();
	private List<int> angleHits = new List<int>();
	private List<int> angleMisses = new List<int>();

	public List<float> TestList = new List<float>();
	private List<List<List<float>>> allRtList = new List<List<List<float>>>();
	private List<List<List<float>>> allHitDistances = new List<List<List<float>>>();
	private List<List<List<float>>> allMissDistances = new List<List<List<float>>>();
	private List<List<int>> allAngleHits = new List<List<int>>();
	private List<List<int>> allAngleMisses = new List<List<int>>();
	public List<string> scoreDates = new List<string>();
	public List<string> names = new List<string>();

	#endregion

	void Start()
	{
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");

		InitialiseLists(angleCount);

		guiManager = GameObject.Find("3DGUICamera").GetComponent<GUIManager>();
		if(guiManager == null)
			Debug.LogError("No GUI Manager was found in the scene.");
	}

	#region Public Methods	
	//Statistics------------------------------------------------------------------------------
	
	public void AddHit(int angleID, float _reactionTime, float _distance)
	{
		if(skipCounter >= calibrationSkipAmount)
		{
			//Increase count for hits
			angleHits[angleID - 1]++; // = angleHits[angleID - 1] + 1;
			//Add reaction time to angle list
			AddReactionTime(angleID, _reactionTime);
			//Add distance to hitDistance list
			AddHitDistance(angleID, _distance);
		}
		else
			skipCounter++;
	}
	
	public void AddMiss(int angleID, float _distance, float _reactionTime)
	{
		//Increase count for misses
		angleMisses[angleID - 1]++;
		//Add reaction time to angle list
		AddReactionTime(angleID, _reactionTime);
		//Add distance to missDistance list
		AddMissDistance(angleID, _distance);
	}

	//Method returning the total hit count
	public int GetHitCount(int entry)
	{
		int count = 0;

		//foreach(int value in angleHits)
		foreach(int value in allAngleHits[allAngleHits.Count - entry -1])
			count += value;

		return count;
	}

	//Method returning the hit count for a specific angleID
	/* public int GetHitCount(int angleID)
	{
		return angleHits[angleID - 1];
	} */

	//Method returning the total miss count
	public int GetMissCount(int entry)
	{
		int count = 0;
		
		//foreach(int value in angleMisses)
		foreach(int value in allAngleMisses[allAngleMisses.Count - entry -1])
			count += value;
		
		return count;
	}

	//Method returning the miss count for a specific angleID
	/* public int GetMissCount(int angleID)
	{
		return angleMisses[angleID - 1];
	} */

	public List<float> GetHitDistances(int angleID, int entry)
	{
		return allHitDistances[allHitDistances.Count - entry -1][angleID - 1];
		//return hitDistances[angleID - 1];
	}

	public List<float> GetMissDistances(int angleID, int entry)
	{
		return allMissDistances[allMissDistances.Count - entry -1][angleID - 1];
		//return missDistances[angleID - 1];
	}
		
	//Method returning a list containing all mean reaction times
	public List<float> GetAllReactionTimes(int entry)
	{
		List<float> reactionMeans = new List<float>();
		
		for(int i = 1; i <= angleCount; i++)
		{
			if(GetReactionMean(i, entry) != null)
			{
				reactionMeans.Add(GetReactionMean(i, entry));
			}
			else
			{
				reactionMeans.Add(0);
			}
		}
		
		return reactionMeans;
	}

	//Method returning the mean reaction of a specific angle
	public float GetReactionMean(int angleID, int entry)
	{
		int index = angleID - 1;
		
		float sum = 0;
		int count = 0;
		
		//foreach(var value in rtList[index])
		foreach(var value in allRtList[allRtList.Count - entry -1][index])
		{
			if(value != 0)
			{
				sum += value;
				count++;
			}
		}
		
		float mean = sum/(float) count;
		
		return mean;
	}
	
	//Method returning the mean reaction time based on all hits & latehits
	public float GetReactionMean()
	{
		float sum = 0;
		int count = 0;
		//Crawl through lists
		foreach (var sublist in rtList)
		{
			foreach (var value in sublist)
			{
				if(value != 0)
				{
					sum += value;
					count++;
				}
			}
		}
		//Calculate Mean Value
		float mean = sum/(float) count;
		return mean;
	}

	//Method returning the mean reaction time as a float
	public string GetReactionMeanFloat()
	{
		float avgReactiontime = GetReactionMean();
		
		string tempAvgReactString;
		
		if(avgReactiontime < 1)
		{
			tempAvgReactString = "0"+avgReactiontime.ToString("#.000");
		}
		else{
			tempAvgReactString = avgReactiontime.ToString("#.000");
		}
		
		return tempAvgReactString;
	}

	public string GetReactionMeanFloat(int entry)
	{
		float avgReactiontime = GetReactionMean(entry);
		
		string tempAvgReactString;
		
		if(avgReactiontime < 1)
		{
			tempAvgReactString = "0"+avgReactiontime.ToString("#.000");
		}
		else{
			tempAvgReactString = avgReactiontime.ToString("#.000");
		}
		
		return tempAvgReactString;
	}

	public float GetReactionMean(int entry)
	{
		float sum = 0;
		int count = 0;
		//Crawl through lists
		foreach (var sublist in allRtList[allRtList.Count - entry -1])
		{
			foreach (var value in sublist)
			{
				if(value != 0)
				{
					sum += value;
					count++;
				}
			}
		}
		//Calculate Mean Value
		float mean = sum/(float) count;
		return mean;
	}

	//Score Stuff-------------------------------------------------------------------------------
	public int GetScore()
	{
		return currentScore;
	}
	
	public int GetMultiplier()
	{
		return multiplier;
	}

	public void AddScore(int value, bool useMultiplier)
	{
		if(useMultiplier == true)
			currentScore += value * multiplier;
		else
			currentScore += value;
		
	}
	public void SubtractScore(int value)
	{
		if(currentScore - value >= 0)
			currentScore -= value;
		else
			currentScore = 0;
	}
	public void IncreaseMultiplier()
	{
		multiplier++;
	}
	public void IncreaseMultiplier(int value)
	{
		multiplier += value;
	}
	public void DecreaseMultiplier()
	{
		if(multiplier - 1 >= 1)
			multiplier -= 1;

	}
	public void DecreaseMultiplier(int value)
	{
		if(multiplier - value >= 1)
			multiplier -= value;
	}
	public void ResetScore()
	{
		currentScore = 0;
	}	
	public void ResetMultiplier()
	{
		multiplier = 1;
	}
	public void ResetScoreAndMultiplier()
	{
		skipCounter = 0;
		ResetScore();
		ResetMultiplier();
		ResetLists();
	}

	public int GetEntryTotal(){
		return names.Count;
	}

	public void ClearScore() {
		currentUser = guiManager.GetUserID();

		if(File.Exists(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt")){
			File.Delete(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt");
		}
	}

	public void SaveScore() {
		currentUser = guiManager.GetUserID();

		if(!Directory.Exists(Application.persistentDataPath+"/Data/")){
			Directory.CreateDirectory(Application.persistentDataPath+"/Data/");
		}

		using (StreamWriter writer = File.AppendText(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt"))
		{
			writer.WriteLine(System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
			writer.WriteLine(guiManager.GetUserName());

			foreach (var sublist in rtList){foreach (var value in sublist){
					writer.Write(value.ToString()+",");
				}
				writer.Write(";");
			}
			writer.WriteLine();

			foreach (var sublist in hitDistances){foreach (var value in sublist){
					writer.Write(value.ToString()+",");
				}
				writer.Write(";");
			}
			writer.WriteLine();

			foreach (var sublist in missDistances){foreach (var value in sublist){
					writer.Write(value.ToString()+",");
				}
				writer.Write(";");
			}
			writer.WriteLine();

			foreach (var value in angleHits){
				writer.Write(value.ToString()+";");
			}
			writer.WriteLine();

			foreach (var value in angleMisses){
				writer.Write(value.ToString()+";");
			}
			writer.Write("-");
		}
	}

	public bool CheckScoreFile(int currentUser) {
		return File.Exists(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt");
	}

	public string GetDate(int entry) {
		return scoreDates[scoreDates.Count - entry -1];
	}

	public string GetName(int entry) {
		return names[names.Count - entry -1];
	}

	public void LoadScores() {
		currentUser = guiManager.GetUserID();

		scoreDates.Clear();
		names.Clear();
		allRtList.Clear();
		allHitDistances.Clear();
		allMissDistances.Clear();
		allAngleHits.Clear();
		allAngleMisses.Clear();

		if(File.Exists(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt")){

			string scoreFile = File.ReadAllText(Application.persistentDataPath+"/Data/Scores"+currentUser.ToString()+".txt");
			string[] scoreEntries = scoreFile.Split('-');

			for(int i = 0; i < scoreEntries.Length; i++){
				if(scoreEntries[i].Length > 10){
					string[] scoreLists = scoreEntries[i].Split('\n');

					scoreDates.Add(scoreLists[0]);
					names.Add(scoreLists[1]);

					string[] rtLists = scoreLists[2].Split(';');
					List<List<float>> tempRtListList = new List<List<float>>();
					for(int j = 0; j < rtLists.Length; j++) {
						string[] allRt = rtLists[j].Split(',');
						List<float> tempRtList = new List<float>();
						foreach(string rt in allRt){
							float rtFloat;
							if(float.TryParse(rt, out rtFloat)) {
								tempRtList.Add(rtFloat);
							}
						}
						tempRtListList.Add(tempRtList);
					}
					allRtList.Add(tempRtListList);
					TestList = allRtList[0][0];

					string[] hitDistanceLists = scoreLists[3].Split(';');
					List<List<float>> temphitDistanceListList = new List<List<float>>();
					for(int j = 0; j < hitDistanceLists.Length; j++) {
						string[] allHitDistance = hitDistanceLists[j].Split(',');
						List<float> temphitDistanceList = new List<float>();
						foreach(string hd in allHitDistance){
							float parseFloat;
							if(float.TryParse(hd, out parseFloat)) {
								temphitDistanceList.Add(parseFloat);
							}
						}
						temphitDistanceListList.Add(temphitDistanceList);
					}
					allHitDistances.Add(temphitDistanceListList);

					string[] missDistanceLists = scoreLists[4].Split(';');
					List<List<float>> tempmissDistanceListList = new List<List<float>>();
					for(int j = 0; j < missDistanceLists.Length; j++) {
						string[] allmissDistance = missDistanceLists[j].Split(',');
						List<float> tempmissDistanceList = new List<float>();
						foreach(string md in allmissDistance){
							float parseFloat;
							if(float.TryParse(md, out parseFloat)) {
								tempmissDistanceList.Add(parseFloat);
							}
						}
						tempmissDistanceListList.Add(tempmissDistanceList);
					}
					allMissDistances.Add(tempmissDistanceListList);

					string[] angleHitLists = scoreLists[5].Split(';');
					List<int> tempAngleHitList = new List<int>();
					foreach(string ha in angleHitLists){
						int parseInt;
						if(int.TryParse(ha, out parseInt)) {
							tempAngleHitList.Add(parseInt);
						}
					}
					allAngleHits.Add(tempAngleHitList);

					string[] angleMissLists = scoreLists[6].Split(';');
					List<int> tempAngleMissList = new List<int>();
					foreach(string ma in angleMissLists){
						int parseInt;
						if(int.TryParse(ma, out parseInt)) {
							tempAngleMissList.Add(parseInt);
						}
					}
					allAngleMisses.Add(tempAngleMissList);
				}
			}
		}
	}
	#endregion


	#region Class Methods
	private void InitialiseLists(int amountOfLists)
	{
		for(int i = 1; i <= amountOfLists; i++)
		{
			//Add amountOfLists to Reaction Time Lists, hitDistances, & missDistances
			List<float> sublist = new List<float>();
			List<float> sublist2 = new List<float>();
			List<float> sublist3 = new List<float>();
			rtList.Add(sublist);
			hitDistances.Add(sublist2);
			missDistances.Add(sublist3);
			//Add amountOfLists entries to hits
			angleHits.Add(0);
			//Add amountOfLists entries to misses
			angleMisses.Add(0);
		}
	}

	private void AddReactionTime(int angleID, float reactionTime)
	{
		int index = angleID - 1;

		rtList[index].Add(reactionTime);
	}
	private void AddHitDistance(int angleID, float distance)
	{
		int index = angleID - 1;
		
		hitDistances[index].Add(distance);
	}
	private void AddMissDistance(int angleID, float distance)
	{
		int index = angleID - 1;
		
		missDistances[index].Add(distance);
	}

	private void ResetLists()
	{
		rtList.Clear();
		hitDistances.Clear();
		missDistances.Clear();
		angleHits.Clear();
		angleMisses.Clear();
		InitialiseLists(angleCount);
	}

	private void NC_Restart()
	{
		ResetScoreAndMultiplier();
	}
	
	private void NC_Play()
	{
		ResetScoreAndMultiplier();
	}
	#endregion
}
