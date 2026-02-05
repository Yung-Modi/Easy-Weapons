using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class SlowMotion : MonoBehaviour
{
	public bool enableSloMo = true;
	public bool timeSlow = false;


    // Update is called once per frame
    void Update ()
	{
		if (enableSloMo)
		{
			if (timeSlow)
			{
				Time.timeScale = 0.25f;
			}
			else
			{
				Time.timeScale = 1.0f;
			}

			Time.fixedDeltaTime = 0.02F * Time.timeScale;
		}
	}

	public void OnTimeSlow(InputValue inputValue)
	{
        timeSlow = inputValue.isPressed;
    }

	public void SlowMotionInput(bool slowMotionState)
	{
		timeSlow = slowMotionState;
    }
}
