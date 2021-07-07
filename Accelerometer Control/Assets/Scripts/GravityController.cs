using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour 
{
	public bool gravityWithDownVector;
	public bool rotateWithDownVector;
	public bool limitMaxGravStrength;

	public float currentGravStrength;
	[HideInInspector] public float gravStrengthPercentage;

	private GameController gameController;
	private Rigidbody localRB;
	private GroundDetection groundDetection;

	private Vector3 gravityDirection;										// Is set to the normalized global accelerometer vector in GameController. Slerped by localTiltSens (and smoothDeltaTime).

	private const float maxGravStrength = 50f;								// currentGravStrength is limited to this amount.
	private const float slowGravStartAmount = maxGravStrength - 20f;		// The current maxGravStrength minus an amount. If currentGravStrength is above this, slow gravity mode starts (Acts like air drag).
	private const float slowGravFactor = 0.3f;								// localGravFactor is set to this when currentGravStrength >= slowGravStartAmount (slow gravity 'air drag' mode). [Default is 0.3f]

	private const float wallFriction = 40f;

	private float localGravFactor = 1;										// Factor that global gravity strength is multiplied by before being applied to currentGravStrength.
	private float localTiltSens;											// Current rotation is slerped by this (by deltaTime) [Set via tiltSensitivity in GameController script.]


	void Start () 
	{
		gameController = GameObject.FindWithTag ("GameController").GetComponent<GameController> ();
		localRB = GetComponent<Rigidbody> ();
		groundDetection = GetComponent<GroundDetection> ();
	}

	void FixedUpdate()
	{
		if (localRB == null) Debug.LogError("Object with a GravityController must have a Rigidbody.");

		Physics.gravity = gameController.currentDownVector * gameController.gravityStrength;
	}

	void Update () 
	{
		// Test jetpack code
		if (Input.GetMouseButton(0)) currentGravStrength -= 30 * Time.deltaTime;

		// Test wall friction code
		if (groundDetection.isColliding && (currentGravStrength - wallFriction * Time.deltaTime) >= 0) currentGravStrength -= wallFriction * Time.deltaTime;




		localTiltSens = gameController.tiltSensitivty;												// Sets localTiltSens to global GameController tiltSensitivity

		SlowGravManager ();																			// localGravFactor is set to slowGravFactor after a certain speed. Acts like air drag.
		GravStrengthManager ();																		// Grav strength is calculated by multiplying global gravity strength by localGravFactor. Limits max grav strength if enabled.
		GravDirectionManager ();																	// Grav direction is calculated from accelerometer and applied by the localTiltSens
		if(rotateWithDownVector) RotateWithDownVector ();											// If rotateWithDevice is enabled, slerps the object's transform.up to -accelerometerVector2D (real world up).

		//localRB.velocity = gravityDirection * currentGravStrength;								// Create gravity vector by combining gravity direction and strength and set current rigidbody velocity to this. 
	}


	void SlowGravManager()
	{
		if (currentGravStrength >= slowGravStartAmount) localGravFactor = slowGravFactor;			// Set the current gravity factor to slowGravFactor grav strength is above slowGravStartAmount.
		else localGravFactor = 1;																	// Else, reset the gravity factor to 1.
	}

	void GravStrengthManager()
	{
		// Calculate the gravity strength next frame (with deltaTime)
		float calcGravAmount = currentGravStrength + (gameController.gravityStrength * localGravFactor) * Time.deltaTime;

		if(limitMaxGravStrength)		// If limitMaxGravStrength is enabled, limits the maximum gravity strength if it exceeds it when calculating the gravity this frame.
		{

			// If the calculated gravity strength is less than or equal to maximum gravity strength, apply the gravity calculation for realsies.
			if (calcGravAmount <= maxGravStrength) currentGravStrength += (gameController.gravityStrength * localGravFactor) * Time.deltaTime;
			
			// Else, set gravity strength to maxGravStrength. 
			else currentGravStrength = maxGravStrength;
			
			// Check absolute grav strength against max grav strength. If greater, set currentGravStrength to maxGravStrength with the sign of currentGravStrength.
			// (This limits the gravity strength to a range of -maxGravStrength to maxGravStrength)
			if (Mathf.Abs (currentGravStrength) > maxGravStrength) currentGravStrength = maxGravStrength * Mathf.Sign (currentGravStrength);
			
			// Percentage based on maxGravStrength.
			gravStrengthPercentage = currentGravStrength / maxGravStrength;
		} 

		else		// Else, just apply gravity with no max limit. Calculate the gravStrengthPercentage out of 100 (for camera zooming and offsetting etc.).
		{
			currentGravStrength += (gameController.gravityStrength * localGravFactor) * Time.deltaTime;
			gravStrengthPercentage = currentGravStrength / 100;
		}
	}

	void GravDirectionManager()
	{
		if (gravityWithDownVector) 		// If gravityWithDevice is enabled...
		{
			// ...slerps the current gravityDirection to the current GameController down vector by localTiltSens and smoothDeltaTime. This lets the global tiltSensitivity affect the gravity direction.
			gravityDirection = Vector3.Slerp (gravityDirection, gameController.currentDownVector, localTiltSens * Time.smoothDeltaTime).normalized;
		}

		else
		{
			// Else, set gravityDirection to game world down (Vector3.down).
			gravityDirection = Vector3.down;
		}
	}

	void RotateWithDownVector()
	{
		// Sets transform.up to negative GameController down vector (real world up), slerp by localTiltSens and smoothDeltaTime.
		transform.up = Vector3.Slerp (transform.up, -gameController.currentDownVector, localTiltSens * Time.smoothDeltaTime);
	}
}
