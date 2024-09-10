using UnityEngine;
using System.Collections;

public class ParticleObjectMovement : MonoBehaviour {

	private Vector3[] waypointArray = new Vector3[4];
	private float curvefactor = 0.25f;
	private float firstPointOffset = 0.4f;
	private float secondPointOffset = 0.8f;


	void Start () {
		GenerateRandomPath();
		iTween.MoveTo(gameObject, iTween.Hash("path", waypointArray, "time", 0.8f, "easetype", iTween.EaseType.linear, "oncomplete", "DestroyObject"));
	}

	void OnDrawGizmos(){
		if (waypointArray != null) {
			if(waypointArray.Length>0){
				iTween.DrawPath(waypointArray);	
			}	
		}	
	}

	void GenerateRandomPath()
	{
		waypointArray[0] = transform.position;
		waypointArray[3] = new Vector3(0,0,0);
		waypointArray[2] = new Vector3(XOffset(2), YOffset(2), 0);
		waypointArray[1] = new Vector3(XOffset(1), YOffset(1), 0);
	}

	float XOffset(int WayPointNumber)
	{
		float distance = transform.position.magnitude;
		if(WayPointNumber == 2)
		{
			return transform.position.x*0.33f+(Random.Range(-secondPointOffset,secondPointOffset))*distance*curvefactor;
		}
		else if(WayPointNumber == 1)
		{
			return transform.position.x*0.66f+(Random.Range(-firstPointOffset,firstPointOffset))*distance*curvefactor;
		}
		return 0.0f;
	}

	float YOffset(int WayPointNumber)
	{
		float distance = transform.position.magnitude;
		if(WayPointNumber == 2)
		{
			return transform.position.y*0.33f+(Random.Range(-secondPointOffset,secondPointOffset))*distance*curvefactor;
		}
		else if(WayPointNumber == 1)
		{
			return transform.position.y*0.66f+(Random.Range(-firstPointOffset,firstPointOffset))*distance*curvefactor;
		}
		return 0.0f;
	}

	void DestroyObject()
	{
		Destroy (gameObject, 1.0f);
	}
}
