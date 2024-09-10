using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestureManager : MonoBehaviour {
	
	#region Editor Publics
	private float tapThreshold = 3.5f;
	private float tapPrecision = 25f;
//	[SerializeField] private float pressThreshold = 0.51f;

	#endregion
	
	#region Privates
	private Dictionary<int,float> touchBeganTimes = new Dictionary<int, float>();
	private Dictionary<int,float> touchEndedTimes = new Dictionary<int, float>();
	private Dictionary<int,Vector2> touchBeganPositions = new Dictionary<int, Vector2>();
	private Dictionary<int,Vector2> touchEndedPositions = new Dictionary<int, Vector2>();
	private Dictionary<int,float> touchTravelDistance = new Dictionary<int, float>();
	private Dictionary<int,float> touchLifetime = new Dictionary<int, float>();
//	private Dictionary<int,GameObject> touchBeganObjects = new Dictionary<int, GameObject>();
	private  GUIManager guiManager;
	#endregion
	
	#region Delegates & Events
	public delegate void TapAction(Vector2 screenPosition);
	public event TapAction OnTapBegan;
	public event TapAction OnTapEnded;
	#endregion

	// Use this for initialization
	void Start () {
		guiManager = GameObject.FindWithTag("GuiCamera").GetComponent<GUIManager>();

		#if UNITY_ANDROID || UNITY_WP8
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.orientation = ScreenOrientation.AutoRotation;
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
		//Universal Quit Button
		if(Input.GetKey(KeyCode.Escape))
		{
			guiManager.ExitConfirmation();
		}
		
		foreach(Touch touch in Input.touches)
		{
			switch(touch.phase)
			{
			case TouchPhase.Began:
				TouchBegan(touch);
				//Log begin position
				touchBeganPositions[touch.fingerId] = touch.position;
				//Log begin time
				touchBeganTimes[touch.fingerId] = Time.time;
				break;

			case TouchPhase.Moved:
				break;

			case TouchPhase.Stationary:
				break;

			case TouchPhase.Canceled:
			case TouchPhase.Ended:
				//Log End Position
				touchEndedPositions[touch.fingerId] = touch.position;
				//Calculate the distance travelled by this touch
				calcTouchTravelDistance(touch.fingerId);
				//Log the time this touch ended
				touchEndedTimes[touch.fingerId] = Time.time;
				//Calculate the time the touch was alive
				calcTouchLifetime(touch.fingerId);

				if(touch.fingerId == 0)
				{
					if(touchLifetime[touch.fingerId] <= tapThreshold)
					{
						CheckSingleTap();
					}
				}
				break;

			default:
				Debug.Log("Incorrect touchphase in gesturemanager2");
				break;
			}
		}

		#if UNITY_WEBPLAYER || UNITY_EDITOR || UNITY_STANDALONE

		if(Input.GetMouseButtonDown(0))
		{			
			//Single Tap Event
			if(OnTapBegan != null)
				OnTapBegan(new Vector2(Input.mousePosition.x,Input.mousePosition.y));
		}

		if(Input.GetMouseButtonUp(0))
		{			
			if(OnTapEnded != null)
				OnTapEnded(new Vector2(Input.mousePosition.x,Input.mousePosition.y));
		}

	#endif
	}

	private void calcTouchTravelDistance(int fingerId)
	{
		touchTravelDistance[fingerId] = Vector2.Distance(touchBeganPositions[fingerId], 
		                                                 touchEndedPositions[fingerId]);
	}

	private void calcTouchLifetime(int fingerId)
	{
		touchLifetime[fingerId] = touchEndedTimes[fingerId] - touchBeganTimes[fingerId];
	}

	//Single Tap
	private void CheckSingleTap()
	{
		Touch tempTouch = Input.GetTouch(0);
		var tempDistance = Vector2.Distance(touchBeganPositions[0], tempTouch.position);
		
		if(tempTouch.tapCount  == 1 && tempDistance < tapPrecision && touchLifetime[0] <= tapThreshold)
		{
			//Single Tap Event
			if(OnTapEnded != null)
				OnTapEnded(tempTouch.position);
		}
	}

	private void TouchBegan(Touch touch)
	{

		if(OnTapBegan != null)
		{
			OnTapBegan(touch.position);
		}
	}
}
