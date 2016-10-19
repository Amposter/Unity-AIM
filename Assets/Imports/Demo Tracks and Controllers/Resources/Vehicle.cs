using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public class Vehicle : MonoBehaviour {

	public float Speed = 5f;
	private float MaxTurnAngle = Mathf.PI/4;
	public float distanceBetweenAxles = 1.258f;
	private float turnSpeed = 180f;


	// Use this for initialization
	void Start ()
	{
		MaxTurnAngle = Mathf.PI/4;
	}

	public float gas = 0.5f;
	public float steer = 0.5f;

	public void updateControls()
	{
		/*var moveDist = Mathf.Clamp((gas*2-1) * Speed,-Speed,Speed) * Time.deltaTime;
		var turnAngle = Mathf.Clamp((steer*2-1) * turnSpeed,-turnSpeed,turnSpeed) * Time.deltaTime * Mathf.Clamp((gas*2-1), -1, 1);

		transform.Rotate(new Vector3(0, turnAngle, 0));
		transform.Translate(Vector3.forward * moveDist);*/

		float turningRadius = distanceBetweenAxles / Mathf.Sin (Mathf.Clamp((steer*2-1)*MaxTurnAngle,-MaxTurnAngle,MaxTurnAngle));

		float currentTurnSpeed = gameObject.GetComponent<Rigidbody> ().velocity.magnitude / turningRadius;

		if (gas < 0)
		{
			currentTurnSpeed = -currentTurnSpeed;
		}

		if (float.IsNaN(gas) || float.IsNaN(currentTurnSpeed))
		{
			return;
		}

		gameObject.GetComponent<Rigidbody> ().angularVelocity = new Vector3 (0, currentTurnSpeed, 0);
		gameObject.GetComponent<Rigidbody>().velocity = transform.forward*(Mathf.Clamp((gas*2-1) * Speed,-Speed,Speed));
	}
}
