using UnityEngine;
using System.Collections;

public class RotateCenter : MonoBehaviour {

	public float rotateSpeed = 0.5f;
	private Vector3 rotateAxis = new Vector3(0.5f, 0.3f, 0.2f);
	private float randomOffset = 0.0f;

	// Use this for initialization
	void Start () {
		rotateAxis = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
		randomOffset = Random.Range(-0.5f, 0.5f);

	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(transform.position, rotateAxis, rotateSpeed);
		rotateAxis = new Vector3(rotateAxis.x+randomOffset, rotateAxis.y+randomOffset, rotateAxis.z+randomOffset);
	}
}
