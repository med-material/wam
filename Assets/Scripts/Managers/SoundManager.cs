using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	#region Editor Publics
	[SerializeField] private AudioClip OnTapBeganAudioClip;
	[SerializeField] private AudioClip OnTapEndAudioClip;
	[SerializeField] private AudioClip OnTargetSuccessHit;
	[SerializeField] private AudioClip OnNewTargetSpawned;
	[SerializeField] private AudioClip OnMiss;
	[SerializeField] private AudioClip OnCenterPunch;
	#endregion
	
	private float hitPitch = 1.0f;
	private float lastHitTime = 0.0f;
	private float lastMissTime = 0.0f;
	private int missCounter = 1;

	#region Public Methods
	public void PlayTouchBegan()
	{
		PlaySound(OnTapBeganAudioClip);
	}

	public void PlayTouchEnded()
	{
		PlaySound(OnTapEndAudioClip);
	}

	public void PlayTargetSuccessHit()
	{
		if(Time.time - lastHitTime < 1.0f)
		{
			hitPitch *= 1.05f;
			PlaySound(OnTargetSuccessHit, hitPitch);
		}
		else
		{
			hitPitch = 1.0f;
			PlaySound(OnTargetSuccessHit);
		}

		lastHitTime = Time.time;
	}

	public void PlayNewTargetSpawned()
	{
		PlaySound(OnNewTargetSpawned);
	}

	public void PlayMissed()
	{
		/*if(Time.time - lastMissTime > 20.0f)
		{
			if(missCounter <= 0)
			{
				lastMissTime = Time.time;
				missCounter = 1;
				PlaySound(OnMiss);
			}
			else
			{
				missCounter--;
				PlaySound(OnMiss);
			}
		}*/
		PlaySound(OnMiss);
	}
	
	public void PlayCenterPunch()
	{
		var pitch = Random.Range(1.0f, 1.1f);
		PlaySound(OnCenterPunch, pitch);
	}
	#endregion


	private void AdjustPitch()
	{

	}

	private void PlaySound(AudioClip audioClipName)
	{
		AudioSource tempAudioSource;
		tempAudioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
	
		tempAudioSource.clip = audioClipName;
		tempAudioSource.Play();

		Destroy(tempAudioSource, tempAudioSource.clip.length);
	}

	private void PlaySound(AudioClip audioClipName, float pitch)
	{
		AudioSource tempAudioSource;
		tempAudioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
		tempAudioSource.pitch = pitch;
		
		tempAudioSource.clip = audioClipName;
		tempAudioSource.Play();
		
		Destroy(tempAudioSource, tempAudioSource.clip.length);
	}
}