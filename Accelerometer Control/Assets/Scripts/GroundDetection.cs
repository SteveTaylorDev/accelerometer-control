using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour 
{
	[HideInInspector] public bool isGrounded;
	[HideInInspector] public bool isColliding;


	void Start () 
	{
		
	}

	void Update () 
	{
		Debug.DrawRay (transform.position, -transform.up, Color.red);
	}

	void OnCollisionStay (Collision other)
	{
		isColliding = true;
	}

	void OnCollisionExit (Collision other)
	{
		isColliding = false;
	}

}
