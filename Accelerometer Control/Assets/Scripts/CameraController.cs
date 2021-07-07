using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public bool allowSpeedZoomMode;
	public bool allowSpeedOffsetMode;
	public bool rotateWithDownVector;								// This is set to true when not in mobile mode to simulate the rotating of a mobile device's screen.

	private const float defaultRectSize = 8f;						// Camera rect size by default.
	private const float defaultMobileRectSize = 10f;				// Default camera rect size in mobile mode.
	private const float maxRectSize = 30f;							// Maximum allowed camera rect size.
	private const float speedZoomFactor = 0.35f;					// Factor the current speed is multiplied by to determine rect size. (For speed based camera zoom).
	private const float speedMobileZoomFactor = 1.15f;				// Factor the current speed is multiplied by to determine rect size in mobile mode. (For speed based camera zoom).
	private const float speedOffsetFactor = 3.5f;					// Factor the accelerometer reading and current gravStrengthPercentage are multiplied by when setting the speed offset. (For speed based camera offset).
	private const float followSpeed = 10f;							// Lerped camera adjustments use this as t (mostly by deltaTime)

	private float localTiltSens;

	private GameController gameController;
	private GravityController targetGravity;

	private Vector3 defaultOffset = new Vector3 (0, 0, -10);
	private Vector3 offset;											// Current camera offset from the targetPosition.
	private Vector3 targetPosition;									// Current position of the cameraTarget object combined with the current offset.

	private GameObject cameraTarget;								// Focal object tagged with "CameraTarget".

	private Camera localCamera;


	void Start ()
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();
		cameraTarget = GameObject.FindWithTag ("CameraTarget");
		targetGravity = cameraTarget.GetComponentInParent<GravityController> ();

		localCamera = GetComponent<Camera> ();
	}

	void Update()
	{
		localTiltSens = gameController.tiltSensitivty;

		if (targetGravity != null)
		{
			SpeedModeManager ();									// Controls the camera rect size and offset based on the current speed.
		} 

		else 
		{
			Debug.LogWarning ("No 'GravityController' found on cameraTarget parent. Speed-based camera features disabled.");	

			if (!gameController.accelerometerMode) localCamera.orthographicSize = defaultRectSize;
			else localCamera.orthographicSize = defaultMobileRectSize;

			offset = defaultOffset;
		}
	}

	void LateUpdate () 
	{
		if (cameraTarget != null) FollowTarget ();
		else Debug.LogError ("No object tagged 'CameraTarget' found. Add this tag to the object you want to focus on.");

		if (!gameController.mobileMode) rotateWithDownVector = true;
		else rotateWithDownVector = false;

		if (rotateWithDownVector) RotateWithDownVector ();
		else transform.up = Vector3.up;
	}


	void FollowTarget()
	{
		targetPosition = cameraTarget.transform.position + offset;
		transform.position = targetPosition;
	}

	void SpeedModeManager()
	{
		float localDefaultRectSize = defaultRectSize;																// Set a local default rect size and use the global default rect size.
		if (gameController.mobileMode) localDefaultRectSize = defaultMobileRectSize;								// If in mobile mode, set the local default rect size to the mobile default rect size.

		if (allowSpeedZoomMode)
		{
			float speedRectSize = Mathf.Abs(targetGravity.currentGravStrength) * speedZoomFactor;					// Multiplies absolute currentGravStrength by the speedZoomFactor to calculate the rect size.
			if (gameController.mobileMode) speedRectSize *= speedMobileZoomFactor;									// If in mobile mode, multiply the calculated rect size by the speedMobileZoomFactor.
			if (speedRectSize >= maxRectSize) speedRectSize = maxRectSize;											// Limits calculated rect size to maxRectSize.

			if (speedRectSize > localDefaultRectSize) 																// If the calculated rect size is greater than the default rect size...
			{
				// ...lerp the local camera's orthagraphicSize to the calculated speedRectSize by followSpeed.
				localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, speedRectSize, followSpeed * Time.deltaTime);
			}

			if (speedRectSize <= localDefaultRectSize) 																// If the calculated rect size is less than the default rect size...
			{ 
				// ...lerp the local camera's orthagraphicSize from the current size to the default rect size by followSpeed.
				localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, localDefaultRectSize, followSpeed * Time.deltaTime);
			}
		} 

		else 		// If allowSpeedZoomMode is false, lerp localCamera.orthagraphicSize to defaultRectSize.
		{
			localCamera.orthographicSize = Mathf.Lerp (localCamera.orthographicSize, localDefaultRectSize, followSpeed * Time.deltaTime);
		}

		if (allowSpeedOffsetMode)
		{
			// Calculate a vector from the global 2D accelerometer vector (the real-world down), the speedOffsetFactor, and the current gravityStrengthPercentage.
			// Slerp the current offset to this plus the defaultOffset (mainly for the z offset) by followSpeed (and smoothDeltaTime to help with the constant rotating)
			offset = Vector3.Slerp (offset, (gameController.currentDownVector * speedOffsetFactor * targetGravity.gravStrengthPercentage) + new Vector3 (0, 0, defaultOffset.z), followSpeed * Time.smoothDeltaTime);
		}

		else 		// If allowSpeedOffsetMode is false...
		{
			// ...slerp to default offset by followSpeed and smoothDeltaTime.
			offset = Vector3.Slerp (offset, defaultOffset, followSpeed * Time.smoothDeltaTime);
		}
	}

	void RotateWithDownVector()
	{
		// Sets transform.up to negative GameController down vector (real world up), slerp by localTiltSens and smoothDeltaTime.
		transform.up = Vector3.Slerp (transform.up, -gameController.currentDownVector, localTiltSens * Time.smoothDeltaTime);
	}
}
