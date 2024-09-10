using UnityEngine;
using System.Collections;

public class nodeBehaviour : MonoBehaviour {

	private LineRenderer lineRenderer;

	void Awake()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>();

	}

	void Start () 
	{
	}

	public void DrawLine(Vector3 endPoint)
	{
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, endPoint);
	}
}
