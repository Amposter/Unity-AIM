using UnityEngine;
using System.Collections;

public class NEAT_Controller : SimpleHeuristicController
{
	public NEAT_GroupController groupController;
	public bool finishedRoute = false;
	public bool NEATMode = false;
	public Vehicle vehicle;

	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update ();

		if (proximityWarning && !NEATMode)
		{
			Pause ();
			finalLayerMask = vehicleMask | pedestrianMask | boundaryMask;
			NEATMode = true;
			this.gameObject.GetComponent<Vehicle> ().enabled = true;
		}
		if (NEATMode) {
			Pause ();
		}

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
		}
	}

	public void OnCollisionEnter(Collision other)
	{
		if (NEATMode)
		{
			print ("COLLISION");
		}

	}


}
