using UnityEngine;
using System.Collections;

public class ParticleObjectMovementStraight : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//Move to center over 0.5 seconds
		iTween.MoveTo(gameObject, iTween.Hash("position", Vector3.zero, "time", 0.5f, "easetype", iTween.EaseType.easeInBack));
		//Scale to 1.5 linearly over 0.2 seconds
		iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "easetype", iTween.EaseType.linear, "time", 0.2f));
		//Scale to 0.5 linearly after 0.2 seconds over 0.3 seconds
		iTween.ScaleTo(gameObject, iTween.Hash("delay", 0.2f, "scale", Vector3.one * 0.5f, "easetype", iTween.EaseType.linear, "time", 0.3f));
		//Destroy self
		Destroy(gameObject, 0.5f);
	}
}
