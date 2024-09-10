using UnityEngine;
using System.Collections;

public class BpmManager : MonoBehaviour 
{
	
	#region Privates
	private double bpm = 120.0f;
	private double waitTime;
	private double tempTime = 0;
	private bool isBeating = false;
	private int beatCounter = 0;
	#endregion
	
	#region Delegates & Events
	public delegate void OnBeatAction();
	//3 tact beats
	public event OnBeatAction OnBeat3rd1;
	public event OnBeatAction OnBeat3rd2;
	public event OnBeatAction OnBeat3rd3;
	//4 tact beats
	public event OnBeatAction OnBeat4th1;
	public event OnBeatAction OnBeat4th2;
	public event OnBeatAction OnBeat4th3;
	public event OnBeatAction OnBeat4th4;
	//6 tact Beats
	public event OnBeatAction OnBeat6th1;
	public event OnBeatAction OnBeat6th2;
	public event OnBeatAction OnBeat6th3;
	public event OnBeatAction OnBeat6th4;
	public event OnBeatAction OnBeat6th5;
	public event OnBeatAction OnBeat6th6;
	//8 tact Beats
	public event OnBeatAction OnBeat8th1;
	public event OnBeatAction OnBeat8th2;
	public event OnBeatAction OnBeat8th3;
	public event OnBeatAction OnBeat8th4;
	public event OnBeatAction OnBeat8th5;
	public event OnBeatAction OnBeat8th6;
	public event OnBeatAction OnBeat8th7;
	public event OnBeatAction OnBeat8th8;
	//Universal all tact beats
	public event OnBeatAction OnAll3Beats;
	public event OnBeatAction OnAll4Beats;
	public event OnBeatAction OnAll6Beats;
	public event OnBeatAction OnAll8Beats;
	#endregion

	#region Public Methods
	public void StartBeats()
	{
		isBeating = true;
		tempTime = AudioSettings.dspTime;
	}

	public void StopBeats()
	{
		isBeating = false;
		beatCounter = 0;
	}

	public void ToggleBeats()
	{
		isBeating = !isBeating;

		if(isBeating == true)
		{
			tempTime = AudioSettings.dspTime;
		}
	}
	#endregion

	void Awake ()
	{
		waitTime = 30.0f / bpm;
		tempTime = AudioSettings.dspTime;
	}
	
	void Update ()
	{
		if (isBeating) 
		{
			if (AudioSettings.dspTime - tempTime >= waitTime) 
			{
				tempTime += waitTime;

				UpdateBeatCounter();

				CheckBeats();
			}
		}
	}

	#region Class Methods
	//Primary beat counter
	private void UpdateBeatCounter()
	{		
		if(beatCounter +1 < 240)
			beatCounter++;
		else
			beatCounter = 0;
	}

	private void CheckBeats()
	{
		CheckBeat3rd();
		CheckBeat4th();
		CheckBeat6th();
		CheckBeat8th();
	}

	//3 tact event calls
	private void CheckBeat3rd()
	{
		switch(beatCounter%6)
		{
		case 0:
			//0ms & 2000ms?
			if(OnBeat3rd1 != null)
				OnBeat3rd1();
			if(OnAll3Beats != null)
				OnAll3Beats();
			break;
		case 2:
			//667ms
			if(OnBeat3rd2 != null)
				OnBeat3rd2();
			if(OnAll3Beats != null)
				OnAll3Beats();
			break;
		case 4:
			//1333ms
			if(OnBeat3rd3 != null)
				OnBeat3rd3();
			if(OnAll3Beats != null)
				OnAll3Beats();
			break;
		default:
			break;
		}
	}
	
	//4 tact event calls
	private void CheckBeat4th()
	{
		switch(beatCounter%8)
		{
		case 0:
			//0ms & 2000ms?
			if(OnBeat4th1 != null)
				OnBeat4th1();
			if(OnAll4Beats != null)
				OnAll4Beats();
			break;
		case 2:
			//500ms
			if(OnBeat4th2 != null)
				OnBeat4th2();
			if(OnAll4Beats != null)
				OnAll4Beats();
			break;
		case 4:
			//1000ms
			if(OnBeat4th3 != null)
				OnBeat4th3();
			if(OnAll4Beats != null)
				OnAll4Beats();
			break;
		case 6:
			//1500ms
			if(OnBeat4th4 != null)
				OnBeat4th4();
			if(OnAll4Beats != null)
				OnAll4Beats();
			break;
		default:
			break;
		}
	}
	
	//6 tact event calls
	private void CheckBeat6th()
	{
		switch(beatCounter%6)
		{
		case 0:
			//0ms & 2000ms?
			if(OnBeat6th1 != null)
				OnBeat6th1();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		case 1:
			//333ms
			if(OnBeat6th2 != null)
				OnBeat6th2();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		case 2:
			//667ms
			if(OnBeat6th3 != null)
				OnBeat6th3();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		case 3:
			//1000ms
			if(OnBeat6th4 != null)
				OnBeat6th4();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		case 4:
			//1333ms
			if(OnBeat6th5 != null)
				OnBeat6th5();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		case 5:
			//1667ms
			if(OnBeat6th6 != null)
				OnBeat6th6();
			if(OnAll6Beats != null)
				OnAll6Beats();
			break;
		default:
			break;
		}
	}
	
	//8 tact event calls
	private void CheckBeat8th()
	{
		switch(beatCounter%8)
		{
		case 0:
			//0ms & 2000ms?
			if(OnBeat8th1 != null)
				OnBeat8th1();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 1:
			//250ms
			if(OnBeat8th2 != null)
				OnBeat8th2();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 2:
			//500ms
			if(OnBeat8th3 != null)
				OnBeat8th3();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 3:
			//750ms
			if(OnBeat8th4 != null)
				OnBeat8th4();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 4:
			//1000ms
			if(OnBeat8th5 != null)
				OnBeat8th5();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 5:
			//1250ms
			if(OnBeat8th6 != null)
				OnBeat8th6();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 6:
			//1500ms
			if(OnBeat8th7 != null)
				OnBeat8th7();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		case 7:
			//1750ms
			if(OnBeat8th8 != null)
				OnBeat8th8();
			if(OnAll8Beats != null)
				OnAll8Beats();
			break;
		default:
			break;
		}
	}
	#endregion
}
