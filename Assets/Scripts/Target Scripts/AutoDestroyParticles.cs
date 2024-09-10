using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyParticles : MonoBehaviour 
{
	void Update()
	{
		if(GetComponent<ParticleSystem>().isPlaying == false)
			Destroy(gameObject);
	}
}
