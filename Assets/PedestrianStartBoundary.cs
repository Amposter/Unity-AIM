using UnityEngine;
using System.Collections;

//Inherits from PedestrianBoundary
//PedestrianBoundary has the base methods for getting a random point and stores the extent information of the bounds
public class PedestrianStartBoundary : PedestrianBoundary {

	public GameObject endBounary;
	PedestrianEndBoundary endBoundaryScript;

	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();
		endBoundaryScript = endBounary.GetComponent<PedestrianEndBoundary> ();
	}

	// Returns a straight-line path for pedestrians denoted by a start and end point 
	public Vector3[] getPath()
	{
		return new Vector3[]{getRandomPoint(), endBoundaryScript.getRandomPoint()};
	}

}
