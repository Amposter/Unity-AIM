using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public class Vehicle : MonoBehaviour {

	public float Speed = 5f;
	private float MaxTurnAngle = Mathf.PI/4;
	public float distanceBetweenAxles = 1.258f;


	// Use this for initialization
	void Start ()
	{
		MaxTurnAngle = Mathf.PI/4;
	}

	public float gas = 0;
	public float steer = 0;


	void LateUpdate()
	{
		float turningRadius = distanceBetweenAxles / Mathf.Sin (Mathf.Clamp((steer*2-1)*MaxTurnAngle,-MaxTurnAngle,MaxTurnAngle));

		float currentTurnSpeed = gameObject.GetComponent<Rigidbody> ().velocity.magnitude / turningRadius;

		if (gas < 0)
		{
			currentTurnSpeed = -currentTurnSpeed;
		}

		gameObject.GetComponent<Rigidbody> ().angularVelocity = new Vector3 (0, currentTurnSpeed, 0);
		gameObject.GetComponent<Rigidbody>().velocity = transform.forward*(Mathf.Clamp((gas*2-1) * Speed,-Speed,Speed));
	}


}
