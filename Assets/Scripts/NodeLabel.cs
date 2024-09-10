using UnityEngine;
using System.Collections;

public class NodeLabel : MonoBehaviour {

	[SerializeField] private GUIStyle style;
	[SerializeField] private LabelTypes labelType;
	private enum LabelTypes {GridLabel, AngleLabel};
	private string label = "";
	private Rect labelRect = new Rect(0, 0, 25, 20);
	private bool displayLabel = false;
	Vector2 screenPos;

	void Awake()
	{
		//Get object
		Camera cam = GameObject.FindWithTag("GuiCamera").GetComponent<Camera>();
		//Get the screenposition of the object
		Vector3 temp = cam.WorldToScreenPoint(transform.position);
		//Correct for camera / screen bullshit
		screenPos = new Vector2(temp.x, Screen.height - temp.y);
		labelRect = new Rect(temp.x, Screen.height - temp.y, 40, 15);

		if(labelType == LabelTypes.AngleLabel)
			labelRect.center = screenPos;
	}

	public void SetText(string text)
	{
		//Change string
		label = text;
		//Flag for display
		displayLabel = true;
	}

	void OnGUI()
	{
		if(displayLabel == true)
			GUI.Label(labelRect, label, style);
	}
}
