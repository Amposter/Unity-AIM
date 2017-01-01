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
	protected override void Update()
	{
		base.Update ();

		rewardTimer += Time.deltaTime;

		if (getSpeedWeight () < 0.015)
		{
			groupController.idleTime += Time.deltaTime;
		}

		if (rewardTimer >= rewardTimeInterval)
		{
			rewardTimer = 0;

			groupController.speedCheckCount++;
			groupController.totalSpeedAccumulator += getSpeedWeight ();
			groupController.minDistanceCheckCount++;
			groupController.totalDistanceAccumulator += (1f-minObstacleRange);
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

		Vector2 startPoint = curves[0].GetPointAt(0f);
		transform.position = startPoint;
		float angle = 360 - Vector2.Angle (transform.up, (Vector2)(curves[0].GetPointAt(0.1f) - curves[0].GetPointAt(0f))); //If the rotation goes wonky, these 2 lines might be why
		transform.rotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward);
		
		string name = GameObject.FindGameObjectWithTag("Track").name;
		name = (name[name.Length - 2].ToString() + name[name.Length - 1]);
		int level = int.Parse(name);
		resolution = Config.samplingSteps[level-1];

		counter = 0;
		this.curve = curves[counter];
		offset = 1;
		toPoint = curve.GetPointAt(offset / (float)resolution);
		//transform.rotation *= Quaternion.FromToRotation(transform.up, (toPoint - (Vector2)transform.position).normalized);

	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<TrackWayPoint> () == null)
		{
			return;
		}
		if(other.gameObject.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.END)
		{
			finishedRoute = true;
			groupController.carsThrough++;
		}
    }

    public void OnCollisionEnter2D(Collision2D other)
	{
		groupController.collisionCount++;
		finishedRoute = true;
	}


}
