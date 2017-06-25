using UnityEngine;
using System.Collections;

public class PedestrianEndBoundary : PedestrianBoundary {

	// Use this for initialization
	protected override void Start ()  
	{
		base.Start ();
			
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Pedestrian")
			Destroy (col.gameObject);
	}
}
