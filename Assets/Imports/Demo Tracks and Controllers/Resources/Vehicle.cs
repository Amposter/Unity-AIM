using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour
{

	public float maxSpeed;
	public float maxReverseSpeed;
	public float maxAcceleration;
	public float maxBrakeForce;
	public float maxSteeringAngle;
	public float distanceBetweenAxles;
	public float frictionForce;

	private float currentAcceleration = 0;
	private float currentTurnSpeed = 0;
	private float currentSteeringAngle = 0;
	private float currentBrakeForce = 0;
	private bool isMovingForward = true;

	// Use this for initialization
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void FixedUpdate()
	{
		if (isMovingForward)
		{
			if (gameObject.GetComponent<Rigidbody> ().velocity.magnitude > maxSpeed)
			{
				gameObject.GetComponent<Rigidbody> ().velocity = gameObject.GetComponent<Rigidbody> ().velocity.normalized * maxSpeed;
			}
		}
		else if(gameObject.GetComponent<Rigidbody> ().velocity.magnitude > maxReverseSpeed)
		{
			gameObject.GetComponent<Rigidbody> ().velocity = gameObject.GetComponent<Rigidbody> ().velocity.normalized * maxReverseSpeed;
		}
		// LIMIT REVERSE

		if (gameObject.GetComponent<Rigidbody> ().velocity.magnitude != 0)
		{
			if (Vector3.Angle (transform.forward, gameObject.GetComponent<Rigidbody> ().velocity.normalized) > 90f)
			{
				gameObject.GetComponent<Rigidbody> ().velocity = gameObject.GetComponent<Rigidbody> ().velocity.magnitude * -transform.forward;
				isMovingForward = false;
			}
			else
			{
				gameObject.GetComponent<Rigidbody> ().velocity = gameObject.GetComponent<Rigidbody> ().velocity.magnitude * transform.forward;
				isMovingForward = true;
			}

			float currentFriction = (frictionForce * gameObject.GetComponent<Rigidbody> ().velocity.magnitude);

			//if we accelerating ignore friction
			if (currentAcceleration > 0.1)
			{
				currentFriction = 0;
			}
				
			float finalFrictionForce = currentBrakeForce + currentFriction;
			gameObject.GetComponent<Rigidbody> ().AddForce(-gameObject.GetComponent<Rigidbody> ().velocity.normalized * finalFrictionForce, ForceMode.Acceleration);

			if (gameObject.GetComponent<Rigidbody> ().velocity.magnitude < 0.01)
			{
				gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			}
		}


		if (currentSteeringAngle != 0 && gameObject.GetComponent<Rigidbody> ().velocity.magnitude != 0)
		{
			float turningRadius = distanceBetweenAxles / Mathf.Sin (currentSteeringAngle);

			currentTurnSpeed = gameObject.GetComponent<Rigidbody> ().velocity.magnitude / turningRadius;

			if (!isMovingForward)
			{
				currentTurnSpeed = -currentTurnSpeed;
			}

			gameObject.GetComponent<Rigidbody> ().angularVelocity = new Vector3 (0, currentTurnSpeed, 0);
		}
		else
		{
			gameObject.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
		}

		gameObject.GetComponent<Rigidbody> ().AddForce(transform.forward*currentAcceleration, ForceMode.Acceleration);


	}

	//value between -1 and 1
	public void setSteeringMagnitude(float turning)
	{
		currentSteeringAngle = maxSteeringAngle * turning;
	}

	//value between 0 and 1
	public void setbrakingMagnitude(float braking)
	{
		currentBrakeForce = maxBrakeForce * braking;
	}

	//value between -1 and 1
	public void setAccelerationMagnitude(float acceleration)
	{
		currentAcceleration = maxAcceleration * acceleration;
	}

	//detect collisions!
	public void OnCollisionEnter(Collision collision)
	{
		print ("Collision with: " + collision.collider.gameObject.name);
	}
}
