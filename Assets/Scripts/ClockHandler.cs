using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClockHandler : MonoBehaviour {

	private float time = 2.0f;
	private float individualTime;
	private List<Transform> components = new List<Transform>();

	// Use this for initialization
	void Start () 
	{
		GetChildTransforms();
		CalculateIndividualTime();
		HideComponents();
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	#region Public Methods
	public void SetTime(float t)
	{
		time = t;
		CalculateIndividualTime();
	}

	public void BeginClock()
	{
		//Call the clock as a coroutine
		StartCoroutine(DoClock());
	}
	#endregion

	#region Class Methods
	private void HideComponents()
	{
		//Scale the clock to size 0 - objects "goes into" the center object
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "easetype", iTween.EaseType.easeInBack));

		foreach(Transform t in components)
		{
			//Change alpha value to 25% over 2ms
			iTween.FadeTo(t.gameObject, 0.25f, 0.2f);
		}
	}

	private IEnumerator DoClock()
	{
		//Scale object to size 1 - Object "comes out" from the center object
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutBack));

		foreach(Transform t in components)
		{
			//Change alpha value to 100% over the weighted time.
			iTween.FadeTo(t.gameObject, 1.0f, individualTime);
			//Wait until first object is done fading
			yield return new WaitForSeconds(individualTime);
		}
		//Hide all components when done
		HideComponents();
	}

	private void GetChildTransforms()
	{
		foreach(Transform child in transform)
		{
			components.Add(child);
		}
	}

	private void CalculateIndividualTime()
	{
		individualTime = (time - 0.5f)/components.Count;
	}
	#endregion
}
