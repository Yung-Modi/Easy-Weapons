using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRotator : MonoBehaviour {
	
	// A mouselook behaviour with constraints which operate relative to
	// this gameobject's initial rotation.
	
	// Only rotates around local X and Y.
	
	// Works in local coordinates, so if this object is parented
	// to another moving gameobject, its local constraints will
	// operate correctly
	// (Think: looking out the side window of a car, or a gun turret
	// on a moving spaceship with a limited angular range)
	
	// to have no constraints on an axis, set the rotationRange to 360 or greater.

	public Vector2 rotationRange = new Vector3(70,70); 
	public float rotationSpeed = 10;
	public float dampingTime = 0.2f;
	public bool autoZeroVerticalOnMobile = true;
	public bool autoZeroHorizontalOnMobile = false;
	public bool relative = true;
	private float verticalInput;
	private float horizontalInput;
    Vector3 targetAngles;
	Vector3 followAngles;
	Vector3 followVelocity;
	Quaternion originalRotation;

	
	// Use this for initialization
	void Start () {
		originalRotation = transform.localRotation;
#if UNITY_IOS || UNITY_ANDROID
		rotationSpeed = 3f;
#else
if (Gamepad.current == null)
		rotationSpeed = 1.5f;
	else
		rotationSpeed = .9f;
#endif
    }

    // Update is called once per frame
    void Update () {
		
		// we make initial calculations from the original local rotation
		transform.localRotation = originalRotation;

		if (relative)
		{
			
			// wrap values to avoid springing quickly the wrong way from positive to negative
			if (targetAngles.y > 180) { targetAngles.y -= 360; followAngles.y -= 360; }
			if (targetAngles.x > 180) { targetAngles.x -= 360; followAngles.x-= 360; }
			if (targetAngles.y < -180) { targetAngles.y += 360; followAngles.y += 360; }
			if (targetAngles.x < -180) { targetAngles.x += 360; followAngles.x += 360; }

			// with mouse input, we have direct control with no springback required.
			targetAngles.y += horizontalInput * rotationSpeed;
			targetAngles.x += verticalInput * rotationSpeed;

			// clamp values to allowed range
			targetAngles.y = Mathf.Clamp ( targetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f );
			targetAngles.x = Mathf.Clamp ( targetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f );

		} else {

			horizontalInput = Input.mousePosition.x;
			verticalInput = Input.mousePosition.y;

			// set values to allowed range
			targetAngles.y = Mathf.Lerp ( -rotationRange.y * 0.5f, rotationRange.y * 0.5f, horizontalInput/Screen.width );
			targetAngles.x = Mathf.Lerp ( -rotationRange.x * 0.5f, rotationRange.x * 0.5f, verticalInput/Screen.height );



		}





		// smoothly interpolate current values to target angles
		followAngles = Vector3.SmoothDamp( followAngles, targetAngles, ref followVelocity, dampingTime );

		// update the actual gameobject's rotation
		transform.localRotation = originalRotation * Quaternion.Euler( -followAngles.x, followAngles.y, 0 );
		
	}

    public void OnLook(InputValue inputValue)
    {
        horizontalInput = inputValue.Get<Vector2>().normalized.x;
		verticalInput = inputValue.Get<Vector2>().normalized.y;
    }


    public void LookInput(Vector2 lookDirection)
    {
		horizontalInput = lookDirection.normalized.x;
		verticalInput = lookDirection.normalized.y;
    }

}