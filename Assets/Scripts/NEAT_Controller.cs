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
	public Vector2 prevPos;

	// Update is called once per frame
	void FixedUpdate()
	{


		if (getSpeedWeight () < 0.015)
		{
			groupController.idleTime += Time.deltaTime;
		}


		groupController.speedCheckCount++;
		groupController.totalSpeedAccumulator += getSpeedWeight ();
		groupController.minDistanceCheckCount++;
		groupController.totalDistanceAccumulator += (1f-minObstacleRange);

	}

	public override void updateSensors()
	{
		base.updateSensors ();
	}

	public void startDriving(BezierCurve[] curves)
	{
		setCurve (curves [0]);
		setCurves (curves);

		Vector2 startPoint = curves[0].GetPointAt(0f);
		transform.position = startPoint;

		string name = GameObject.FindGameObjectWithTag("Track").name;
		name = (name[name.Length - 2].ToString() + name[name.Length - 1]);
		int level = int.Parse(name);
		resolution = Config.samplingSteps[level-1];

		counter = 0;
		this.curve = curves[counter];
		offset = 1;
		updateResolution (curve.length);
		toPoint = curve.GetPointAt(offset / (float)resolution);
		wayPointDist = (toPoint - startPoint).magnitude;
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Pedestrian") 
		{
			groupController.collisionCount++;
			finishedRoute = true;
		}
	}
    public void OnCollisionEnter2D(Collision2D other)
	{
		groupController.collisionCount++;
		finishedRoute = true;
	}

	public float getDistToWayPoint()
	{
		return distToWayPoint;
	}

	public float getAngleToWayPoint()
	{
		return angleToWayPoint;
	}

	public float getMinObstacleRange()
	{
		return minObstacleRange;
	}

	public float getMinObstacleAngle()
	{
		return minAngle;
	}
}
