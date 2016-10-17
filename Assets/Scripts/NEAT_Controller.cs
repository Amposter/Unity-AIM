using UnityEngine;
using System.Collections;
using System;

public class NEAT_Controller : SimpleHeuristicController
{
	public NEAT_GroupController groupController;
	public bool finishedRoute = false;
	public bool NEATMode = false;
	public Vehicle vehicle;
	public bool hasBeenTriggered = false;
	float rewardTimer = 0;
	float rewardTimeInterval = 0.25f;
	float distanceRewardCutOff = 1f;

	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
	}

	public float prevAngle = float.MaxValue;
	public float prevDistance = float.MaxValue;
	public float prevMinObstacleDistance = float.MaxValue;
	public float initialDistanceToTarget = 1;
	public Vector3 prevPos;

	// Update is called once per frame
	protected override void Update()
	{
		base.Update ();

		UpdateNextPoint();

		//angle to targetWayPoint
		float angle = Vector3.Angle (curve.GetPointAt (1) - this.transform.position, transform.forward);
		Vector3 cross = Vector3.Cross (curve.GetPointAt (1) - this.transform.position, transform.forward);
		angle = (cross.y < 0)?angle:-angle;
		sensorInputs [10] = (float)Math.Round(((angle / 180)+1)/2,3);

		//distance to target wayPoint
		sensorInputs [11] = (float)Math.Round(Mathf.Clamp((curve.GetPointAt(1)-this.transform.position).magnitude/initialDistanceToTarget,0f,1f),3);
		//Debug.DrawLine (this.transform.position, curve.GetPointAt(1), Color.white);
		proximityWarning = true;

		//distance to goal
		if (distanceRewardCutOff - sensorInputs [11] >= 0.01f && sensorInputs [11] > 0)
		{
			groupController.groupFitness += 1000*((distanceRewardCutOff - sensorInputs [11])/0.01f);
			groupController.groupFitness += (1 - minObstacleRange) * 0.1f;
			distanceRewardCutOff -= (distanceRewardCutOff - sensorInputs [11]);
		}

		//if in NEAT_MODE issue time based rewards
		if (NEATMode)
		{
			rewardTimer += Time.deltaTime;
			if (rewardTimer >= rewardTimeInterval)
			{
				//angle to goal
				/*if (sensorInputs [10] < prevAngle)
				{
					groupController.groupFitness += 1;
					prevAngle = sensorInputs [10];
				}
				*/



				//groupController.groupFitness += (1 - minObstacleRange)*2;

				//penalize for not moving
				if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < 0.02)
				{
					groupController.groupFitness -= 0.1f;
				}
				//penalize for reversing or driving too slow
				if (gameObject.GetComponent<Vehicle> ().gas < 0.65)
				{
					groupController.groupFitness -= 0.1f;
				}
				//penalize for movDing further from target
				if ((curve.GetPointAt (1) - this.transform.position).magnitude > (curve.GetPointAt (1) - prevPos).magnitude)
				{
					groupController.groupFitness -= 0.1f;
				}


				prevPos = this.transform.position;
			}
		}

		if (!proximityWarning && NEATMode)
        {
            this.gameObject.GetComponent<Vehicle>().enabled = false;
			gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			gameObject.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
            NEATMode = false;
            finalLayerMask = /*vehicleMask |*/ pedestrianMask;
            UpdateNextPoint();
            Unpause();
			//groupController.groupFitness += 250;
        }
        else if (proximityWarning && !NEATMode)
		{
			Pause ();
			finalLayerMask = /*vehicleMask | */pedestrianMask | boundaryMask;

			prevPos = this.transform.position;
			prevAngle = float.MaxValue;
			prevDistance = 1;
			prevMinObstacleDistance = float.MaxValue;
			initialDistanceToTarget = (curve.GetPointAt(1)-this.transform.position).magnitude;
			NEATMode = true;
			if (hasBeenTriggered == false)
			{
				hasBeenTriggered = true;
				groupController.totalTriggered++;
			}

			this.gameObject.GetComponent<Vehicle> ().enabled = true;
        }



	}


	public void startDriving(BezierCurve[] curves)
	{
		setCurve (curves [0]);
		setCurves (curves);
		//StartCoroutine ("AutoDrive");
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<TrackWayPoint> () == null) {
			return;
		}
		if(other.gameObject.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.END)
		{
			finishedRoute = true;
			if(hasBeenTriggered)
			{
				groupController.groupFitness += 10000;
			}
          
		}
	}

	public void OnCollisionEnter(Collision other)
	{
		if (NEATMode)
		{
            groupController.groupFitness -= 50;
			//print ("COLLISION");
		}

	}


}
