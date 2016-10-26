using UnityEngine;
using System.Collections;
using System;

public class NEAT_Controller : SimpleHeuristicController
{
	public NEAT_GroupController groupController;
	public bool finishedRoute = false;
	public bool NEATMode = true;
	public Vehicle vehicle;
	public bool hasBeenTriggered = false;
	float rewardTimer = 0;
	float rewardTimeInterval = 0.2f;

    public enum Controller {HEURISTIC, NEURAL_NET};
    public Controller controller;

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

		rewardTimer += Time.deltaTime;

		if (rewardTimer >= rewardTimeInterval)
		{
			rewardTimer = 0;

			groupController.speedCheckCount++;
			groupController.totalSpeedAccumulator += getSpeedWeight ();
		}

	}

	public override void updateSensors()
	{
		base.updateSensors ();
	}

	public void startDriving(BezierCurve[] curves)
	{
		setCurve (curves [0]);
		setCurves (curves);

		resolution = 25;
		counter = 0;
		this.curve = curves[counter];
		offset = 1;
		toPoint = curve.GetPointAt(offset / (float)resolution);
		toPoint.y = transform.position.y;
		transform.LookAt(toPoint);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<TrackWayPoint> () == null)
		{
			return;
		}
		if(other.gameObject.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.END)
		{
			finishedRoute = true;
		}
    }

    public void OnCollisionEnter(Collision other)
	{
		groupController.collisionCount++;
		finishedRoute = true;
	}


}
