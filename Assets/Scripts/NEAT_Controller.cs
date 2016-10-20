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
	float rewardTimeInterval = 0.25f;
	float distanceRewardCutOff = 1f;

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

        //Rewards for driving faster when not in an intersection.
        if (controller == Controller.HEURISTIC)
        {
            float speedWeight = getSpeedWeight();

            if (speedWeight < 0.1)
            { //no reward
            }
            else if (speedWeight < 0.3)
            {
                groupController.groupFitness += 0.001f;
            }
            else if (speedWeight < 0.5)
            {
                groupController.groupFitness += 0.003f;
            }
            else if (speedWeight < 0.85)
            {
                groupController.groupFitness += 0.09f;
            }
            else if (speedWeight <= 0.99)
            {
                groupController.groupFitness += 0.1f;
            }
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
		StartCoroutine ("AutoDrive");
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
				groupController.groupFitness += 50;
			}       
		}

        //Update controller type
        if (other.gameObject.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.INTERSECTION_BORDER)
        {
            if (controller == Controller.HEURISTIC)
                controller = Controller.NEURAL_NET;
            else
                controller = Controller.HEURISTIC;
        }
    }

    public void OnCollisionEnter(Collision other)
	{

			finishedRoute = true;
			groupController.groupFitness -= 100;
			//print ("COLLISION");
	}


}
