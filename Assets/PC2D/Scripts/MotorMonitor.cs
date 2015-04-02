using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MotorMonitor : MonoBehaviour 
{
	public Text fallText;
	public Text motorStateText;
	public Text prevMotorStateText;

	PlatformerMotor2D.MotorState savedMotorState;
	PlatformerMotor2D _motor;

	PlatformerMotor2D.MotorState MotorState
	{
		set
		{
			if(savedMotorState != value)
			{
				prevMotorStateText.text = string.Format("Prev Motor State: {0}", savedMotorState);
				motorStateText.text = string.Format("Motor State: {0}", value);
			}
			savedMotorState = value;
		}
	}

	// Use this for initialization
	void Start () 
	{
		_motor = GetComponent<PlatformerMotor2D> ();
		_motor.onFallFinished += OnFallFinished;
		fallText.color = Color.white;
	}

	public void OnFallFinished(float fallDist)
	{
		fallText.text = string.Format ("Fall Distance: {0:F}", fallDist);
		fallText.color = Color.green;
		_justFellTimer = 0.5f;
	}

	float _justFellTimer;

	// Update is called once per frame
	void Update () 
	{
		if (_justFellTimer > 0) 
		{
			_justFellTimer -= Time.deltaTime;
			if(_justFellTimer <= 0)
			{
				fallText.color = Color.white;
			}
		}

		MotorState = _motor.motorState;
	}
}
