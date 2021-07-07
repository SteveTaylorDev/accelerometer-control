using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour 
{
	[HideInInspector] public Vector3 currentDownVector;			// The x and y of the accelerometer reading, or the current down if using other controls. Acts as real world down.

	public bool setDefaultTiltSens;
	public bool setDefaultMouseSens;
	public bool setDefaultGravStrength;

	// When in mobile mode, camera uses mobile settings for zooming to acomodate for smaller screens (Linked with accelerometerMode for now).
	public bool mobileMode;
	// Camera uses world up as transform.up, as the player rotates device. When mobile mode is disabled, the camera rotates with the currentDownVector to simulate this.

	public bool accelerometerMode;								// currentDownVector is set by the accelerometer input.
	public bool keyboardRotateMode;								// currentDownVector is set by the keyboard input (WASD only for now).
	public bool mouseRotateMode;

	// How quickly the object orientation matches the down vector. Lower sens is smoother but less responsive, higher sens is more responsive but can be jittery. 
	[Range(0, 50f)]
	public float tiltSensitivty;
	// Acts as traditional rotation sensitivity in digital input modes.

	[Range(0, 10f)]
	public float mouseSensitivity;								// Mouse input is multiplied by this in mouse rotate mode to give a traditional mouse sensitivity modifier. 
	public float gravityStrength;								// Global gravity strength. This can be treated as the "planet" gravity; is fed to the GravityController so it can be altered locally (for local gravity effects).
																// (The local gravity controllers default to Vector3.down for global gravity direction).
	private const float defaultTiltSens = 20f;
	private const float defaultMouseSens = 2.5f;
	private const float defaultGravStrength = 10f;
	private const float controllerTiltAdjuster = 10f;			// Tilt sensitivity is multiplied by this in controller control schemes. 

	private float currentZRotation;								// This is affected by controller input to modify the currentDownVector when not using motion control.

	private GameObject playerObject;
	private GameObject spawnpoint;


	void Start () 
	{
		playerObject = GameObject.FindWithTag("Player");
		spawnpoint = GameObject.FindWithTag ("Spawnpoint");

		if (playerObject == null) Debug.LogError ("No object tagged 'Player' found.");
		if (spawnpoint == null && playerObject != null)	Debug.LogWarning ("No object tagged 'Spawnpoint' found. Placing player at world origin.");
		if (spawnpoint == null && playerObject == null) Debug.LogWarning ("No object tagged 'Spawnpoint' found.");

		// Sets playerObject to the spawnpoint position if both player and spawnpoint exist.
		if(playerObject != null && spawnpoint != null) playerObject.transform.position = spawnpoint.transform.position;	
	}

	void Update () 
	{
		if (setDefaultTiltSens) tiltSensitivty = defaultTiltSens;						// Sets default tilt sensitivity if enabled.
		if (setDefaultMouseSens) mouseSensitivity = defaultMouseSens;					// Sets default mouse sensitivity if enabled.
		if (setDefaultGravStrength) gravityStrength = defaultGravStrength;				// Sets default gravity strength if enabled.
		
		if (accelerometerMode) AccelerometerDownVector ();
		else ControllerDownVector ();
	}


	void ControllerDownVector()
	{
		mobileMode = false;

		if (keyboardRotateMode)
		{
			mouseRotateMode = false;		// Disable mouse rotate mode.

			// Multiply tilt sensitivity by controllerTiltAdjuster to increase sensitivity and accomodate controller input. 
			// Add to or subtract from currentZRotation by smoothDeltaTime to help with slerping below. 
			if (Input.GetKey (KeyCode.D)) currentZRotation += tiltSensitivty * controllerTiltAdjuster * Time.smoothDeltaTime;
			if (Input.GetKey (KeyCode.A)) currentZRotation -= tiltSensitivty * controllerTiltAdjuster * Time.smoothDeltaTime;
		}

		if (mouseRotateMode) 
		{
			// Add mouse horizontal input multiplied by mouseSensitivity to the currentZRotation.
			currentZRotation += Input.GetAxis ("Mouse X") * mouseSensitivity; 
		}
			
		// Find down vector by multiplying a Quaternion.Euler with currentZRotation as the z modifier by Vector3.down. Acts as a 360 degree rotation.
		// Slerp currentDownVector to this by tiltSensitivity as t. Time.smoothDeltaTime helps with any slerp jitter.
		currentDownVector = Vector3.Slerp(currentDownVector, Quaternion.Euler (0, 0, currentZRotation) * Vector3.down, tiltSensitivty * Time.smoothDeltaTime);
	}

	void AccelerometerDownVector()
	{
		// Reads the x and y of the accelerometer and creates a normalized vector.
		currentDownVector = new Vector3 (Input.acceleration.x, Input.acceleration.y, 0).normalized;

		mobileMode = true;
		mouseRotateMode = false;
		keyboardRotateMode = false;
	}
}
