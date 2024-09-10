using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissClick : MonoBehaviour 
{
	
	#region Publics
	#endregion
	
	#region Editor Publics
	#endregion
	
	#region Privates
	//Script connectivity
	private GestureManager gManager;
	private SoundManager soundManager;
	private WriteToTXT txtWriter;
	private GameStateManager gameManager;

	private bool logMissClicks = false;

	private string missClick = "MissClick";
	#endregion
	
	void Awake()
	{
		gManager = Camera.main.GetComponent<GestureManager>();
		soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		txtWriter = GameObject.Find("WriteToTXT").GetComponent<WriteToTXT>();
		gameManager = GameObject.Find("GameStateManager").GetComponent<GameStateManager>();
	}

	void Start () 
	{
		//Subscribe to Tap Gesture
		gManager.OnTapBegan += HandleOnTapBegan;

		//Begin Observing for default notifications
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Play");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Pause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Unpause");
		NotificationCenter.DefaultCenter().AddObserver(this, "NC_Restart");
	
	}

	void HandleOnTapBegan (Vector2 screenPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
		RaycastHit hitInfo;
		if(Physics.Raycast(ray, out hitInfo) == false)
		{				
			//Play touch sound
			//soundManager.PlayMissed();

			//Could we skip all this stuff? ;D 
			if(logMissClicks != false)
			{
				soundManager.PlayMissed();

				//Declare variables
				float dist = 0.0f;
				Vector3 pos = Vector3.zero;
				int angleID = 0;
				int angle = 0;
				float reactionTime = 0;
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
						var distance = Vector2.Distance(screenPosition, position);

						if(dist == 0 || dist > distance)
						{
							pos = target.transform.position;
							dist = distance;
							angleID = target.GetComponent<ObjectHandler>().GetAngleID();
							angle = target.GetComponent<ObjectHandler>().GetAngle();
							reactionTime = target.GetComponent<ObjectHandler>().CalculateReactionTime();
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
							var distance = Vector2.Distance(screenPosition, position);
							
							if(dist == 0 || dist > distance)
							{
								pos = target.transform.position;
								dist = distance;
								angleID = target.GetComponent<Phase2Object>().GetMultiplier();
								angle = target.GetComponent<Phase2Object>().GetAngle();
								target.GetComponent<Phase2Object>().CalculateReactionTime();
								reactionTime = target.GetComponent<Phase2Object>().reactiontime;
							}
						}
					}

					//Was there no active targets, then calculate distance to center
					if(dist == 0)
						dist = Vector2.Distance(screenPosition, Camera.main.WorldToScreenPoint(pos));
						reactionTime = gameManager.lastHitTime - Time.time;
				}
				else
				{
					//Closest target is center/Vector3.zero
					dist = Vector2.Distance(screenPosition, Camera.main.WorldToScreenPoint(pos));
					reactionTime = gameManager.lastHitTime - Time.time;
				}

				//Add to file
				txtWriter.LogData(missClick, reactionTime, angle, dist, pos, screenPosition, missClick, 0, angleID);
			}
		}
	}
	
	private void NC_Play()
	{
		logMissClicks = true;
	}
	
	private void NC_Pause()
	{
		logMissClicks = false;
	}
	
	private void NC_Unpause()
	{
		logMissClicks = true;
	}
	
	private void NC_Restart()
	{
		logMissClicks = false;
	}	
}
