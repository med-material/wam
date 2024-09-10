using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EndGameLines : MonoBehaviour {

	[SerializeField] private bool useAdaptiveRtFigure = false;
	[SerializeField] private GameObject Labels;
	[SerializeField] private SpawnObjects spawnObjects;

	private GameObject Grid;
	private GameObject GridWithLabels;
	private GameObject GridBG;
	
	private HighscoreManager hsManager;
	private SpawnManager spManager;

	private GameObject[] NodeArray;
	private GameObject[] ScatterArray;
	private List<GameObject> LabelListGrid = new List<GameObject>();
	private List<GameObject> LabelListAngles = new List<GameObject>();

	private List<float> reactionMeans = new List<float>();

	private Vector3 hitIconPos = new Vector3(-8f, 55.2f, 1.4f);
	private Vector3 missIconPos = new Vector3(-8f, 54.5f, 1.4f);

	private float timeBetweenSpawns = 0;
	[SerializeField] private float timeToSpawnAll = 0.5f;

	private bool showingRTScreen;
	public int scoreEntry = 0;
	private bool showingCoT = false;
	private float tailLength = 10;

	private int drawRTdelay = 0;

	// Use this for initialization
	void Start () {
		Grid = GameObject.Find("Grid");
		GridWithLabels = GameObject.Find("GridWithLabels");
		GridBG = GameObject.Find("GridBG");
		hsManager = GameObject.Find("HighscoreManager").GetComponent<HighscoreManager>();
		spManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

		Grid.SetActive(false);
		GridWithLabels.SetActive(false);
		GridBG.SetActive(false);
	}

	void Update() {
		drawRTdelay++;
		if(showingRTScreen && drawRTdelay == 2)
			DoReactionScreen();
	}

	private void EnableEndBackground()
	{
		GridBG.SetActive(true);
	}

	private void SpawnGridLabels(float incrementValue)
	{
		ClearLabelList(LabelListGrid);

		for(int i = 1; i <= 5; i++)
		{
			GameObject go = SpawnNodeReturn(2, 1 * i, spawnObjects.NodeLabel);
			//Set label text
			go.GetComponent<NodeLabel>().SetText("" + incrementValue * i + "s");
			go.transform.name += ""+i;
			//Set parent to keep hierachy clean
			go.transform.parent = Labels.transform;
			//Stuff in array for cleaning later
			LabelListGrid.Add(go);		
		}
	}

	private void SpawnAngleLabels()
	{
		ClearLabelList(LabelListAngles);
		
		for(int i = 1; i <= 10; i++)
		{
			//Spawn angleID labels
			GameObject go = SpawnNodeReturn(i, 5.5f, spawnObjects.AngleLabel);
			//Set label text
			go.GetComponent<NodeLabel>().SetText("" + (36 * i - 18));
			go.transform.name = "AngleLabel"+i;
			//Set parent to keep hierachy clean
			go.transform.parent = Labels.transform;
			//Stuff in array for cleaning later
			LabelListAngles.Add(go);
		}
	}

	private void ClearLabelList(List<GameObject> list)
	{
		foreach(GameObject go in list)
			Destroy(go);

		list.Clear();
	}

	public void DisableEndScreen()
	{
		Grid.SetActive(false);
		GridWithLabels.SetActive(false);
		GridBG.SetActive(false);
		ClearLabelList(LabelListGrid);
		ClearLabelList(LabelListAngles);
		DeleteOldNodes();
		DeleteOldScatters();
	}

	public void DoCurves()
	{
		Grid.SetActive(true);
		GridWithLabels.SetActive(false);
		GridBG.SetActive(true);
		ClearLabelList(LabelListGrid);
		ClearLabelList(LabelListAngles);
		DeleteOldNodes();
		DeleteOldScatters();

		showingCoT = true;

		for(int j = 0; j < hsManager.GetEntryTotal() - scoreEntry; j++){

			Vector3 centerOfHit = Vector3.zero;
			int totalHits = 0;

			for(int i = 1; i <= 10; i++)
			{
				List<float> angleHits = hsManager.GetHitDistances(i, scoreEntry + j);

				foreach (float hit in angleHits)
				{
					int angle = (36 * i) - 18;
					transform.Rotate(transform.forward, (float) -angle);
					Quaternion rotation = transform.rotation;
					Vector3 position = transform.position + gameObject.transform.up * (hit/spManager.GetAbsMaxDist(i))*5;
					transform.Rotate(transform.forward, (float) angle);
					centerOfHit += position;
					totalHits++;
				}
			}

			centerOfHit = centerOfHit/totalHits;
			centerOfHit = Vector3.Scale(centerOfHit, new Vector3(1,1,0)) + Vector3.Scale(transform.position, new Vector3(0,0,1));
			Transform tempNode = Instantiate(spawnObjects.SpawnHit, centerOfHit, Quaternion.identity).transform;

			float scale = (1-j*(1/tailLength));
			Debug.Log(scale);
			if(scale < 0)
				scale = 0;

			tempNode.localScale = tempNode.localScale*scale;
		}

		StoreScatters();
	}

	public void DoHitMissScreen()
	{	
		showingRTScreen = false;
		DeleteOldScatters();
		DeleteOldNodes();
		ClearLabelList(LabelListGrid);
		
		EnableEndBackground();
		Grid.SetActive(true);

		showingCoT = false;

		/* Instantiate(spawnObjects.SpawnHit, hitIconPos, Quaternion.identity);
		Instantiate(spawnObjects.SpawnMiss, missIconPos, Quaternion.identity); */

		/* if(LabelListAngles.Count == 0)
			SpawnAngleLabels(); */

		timeBetweenSpawns = timeToSpawnAll/(hsManager.GetHitCount(scoreEntry) + hsManager.GetMissCount(scoreEntry));
		
		for(int i = 1; i <= 10; i++)
		{
			List<float> angleHits = hsManager.GetHitDistances(i, scoreEntry);
			foreach (float hit in angleHits)
			{
				SpawnNode(i, (hit/spManager.GetAbsMaxDist(i))*5, spawnObjects.SpawnHit);
			}
			
			List<float> angleMisses = hsManager.GetMissDistances(i, scoreEntry);
			foreach (float miss in angleMisses)
			{
				SpawnNode(i, (miss/spManager.GetAbsMaxDist(i))*5, spawnObjects.SpawnMiss);
			}
		}
		
		StoreScatters();
	}

	public void PreviousScores() {
		if(hsManager.GetEntryTotal() > scoreEntry+1){
			scoreEntry++;

			if(showingCoT)
				DoCurves();
			else{
				if(showingRTScreen){
					DoReactionScreen();
					drawRTdelay = 0;
				}
				else{
					DoHitMissScreen();
				}
			}
		}
	}

	public void NextScores() {
		if(scoreEntry > 0) {
			scoreEntry--;

			if(showingCoT)
				DoCurves();
			else{
				if(showingRTScreen){
					DoReactionScreen();
					drawRTdelay = 0;
				}
				else{
					DoHitMissScreen();
				}
			}
		}
	}

	public void DoReactionScreen()
	{
		showingRTScreen = true;
		//StopCoroutine("DelayedScatterSpawning");
		StopAllCoroutines();
		StoreScatters();
		DeleteOldNodes();
		DeleteOldScatters();
		
		EnableEndBackground();
		Grid.SetActive(true);

		showingCoT = false;

		/* if(LabelListAngles.Count == 0)
			SpawnAngleLabels(); */
		
		reactionMeans = hsManager.GetAllReactionTimes(scoreEntry);

		float incrementValue = 0.0f;

		//Find the highest value reaction time
		foreach(float value in reactionMeans)
		{
			if(value != null && value > incrementValue)
				incrementValue = value;
		}
			
		//What is the grid values? 
		if(useAdaptiveRtFigure == true)
		{
			incrementValue = incrementValue/5.0f;
		}
		else if(incrementValue > 2.5f)
		{
			incrementValue = 1.0f;
		}
		else if(incrementValue > 1.25)
		{
			incrementValue = 0.5f;
		}
		else
			incrementValue = 0.25f;

		//Spawn grid labels
		SpawnGridLabels(incrementValue);

		//Spawn line nodes
		for(int i = 1; i <= reactionMeans.Count; i++)
		{
			int index = i -1;
			if(reactionMeans[index] > 0.1f)
			{
				SpawnNode(i, (reactionMeans[index]/(incrementValue*5.0f))*5.0f, spawnObjects.SpawnNode);
			}
			else{
				SpawnNode(i, 0.1f, spawnObjects.SpawnNode);
			}
		}

		StoreNodes();
		DrawLines();
	}

	private void SpawnNode(int int1to10, float distance, GameObject spawnObject)
	{
		int angle = (36 * int1to10) - 18;
		transform.Rotate(transform.forward, (float) -angle);
		Quaternion rotation = transform.rotation;
		Vector3 position = transform.position + gameObject.transform.up * distance;
		transform.Rotate(transform.forward, (float) angle);
		Instantiate(spawnObject, position, rotation);
	}

	private GameObject SpawnNodeReturn(int int1to10, float distance, GameObject spawnObject)
	{
		int angle = (36 * int1to10) - 18;
		transform.Rotate(transform.forward, (float) -angle);
		Quaternion rotation = transform.rotation;
		Vector3 position = transform.position + gameObject.transform.up * distance;
		transform.Rotate(transform.forward, (float) angle);
		GameObject go = (GameObject) Instantiate(spawnObject, position, rotation);
		return go;
	}

	private void StoreNodes()
	{
		NodeArray = GameObject.FindGameObjectsWithTag("Node");
	}

	private void StoreScatters()
	{
		ScatterArray = GameObject.FindGameObjectsWithTag("ScatterPlot");
	}

	private void DrawLines()
	{
		for (int i = 0; i < NodeArray.Length; i++) 
		{
			if(i == NodeArray.Length - 1)
				NodeArray[i].GetComponent<nodeBehaviour>().DrawLine(NodeArray[0].transform.position);
			else
				NodeArray[i].GetComponent<nodeBehaviour>().DrawLine(NodeArray[i+1].transform.position);
		}
	}

	private void DeleteOldNodes()
	{
		if(NodeArray != null)
		{
			foreach (GameObject node in NodeArray)
			{
				Destroy(node);
			}
		}
	}

	private void DeleteOldScatters()
	{
		if(ScatterArray != null)
		{
			foreach (GameObject scatter in ScatterArray)
			{
				Destroy(scatter);
			}
		}
	}
	
	#region Subclasses
	[System.Serializable]
	public class SpawnObjects
	{
		public GameObject SpawnNode;
		public GameObject SpawnHit;
		public GameObject SpawnMiss;
		public GameObject NodeLabel;
		public GameObject AngleLabel;

	}
	#endregion
	
}
