using UnityEngine;
using System.Collections;

public class SessionManager : MonoBehaviour {
	
	public float resetTime;
	
	private System.DateTime sessionStart;
	private System.DateTime lastInteraction;
	
	// Use this for initialization
	void Start () {
		
		lastInteraction = System.DateTime.Now;
	}
	
	// Update is called once per frame
	void Update () {

		if (System.DateTime.Now.Subtract (lastInteraction).TotalMinutes > resetTime) {
			
			Application.LoadLevel("NewMain");
			lastInteraction = System.DateTime.Now;
		}

		if(Input.GetMouseButtonDown(0)) {
			
			lastInteraction = System.DateTime.Now;
		}
		else {

			foreach(Touch touch in Input.touches) {

				if(touch.phase == TouchPhase.Began) {

					lastInteraction = System.DateTime.Now;
				}
			}
		}
	}
}
