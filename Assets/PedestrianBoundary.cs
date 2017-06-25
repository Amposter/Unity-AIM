using UnityEngine;
using System.Collections;

public class PedestrianBoundary : MonoBehaviour {

	//Extents of the collider for getting a random point in the bounds of the object
	protected float xLower;
	protected float xUpper;
	protected float zLower;
	protected float zUpper;

	// Use this for initialization
	protected virtual void Start () 
	{
		Vector3 position = transform.position;
		Bounds b = GetComponent<Collider> ().bounds;
		xLower = position.x - b.extents.x;
		xUpper = position.x + b.extents.x;
		zLower = position.z - b.extents.z;
		zUpper = position.z + b.extents.z;
	}

	//Return a random point that falls within the boundary
	//Calculate using the x and z upper and lower values, y is always equal to 1
	public Vector3 getRandomPoint()
	{
		float x = Random.Range (xLower, xUpper);
		float z = Random.Range (zLower, zUpper);
		return new Vector3(x,1,z);
	}

}
