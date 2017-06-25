using UnityEngine;
using System.Collections;

public class Pedestrian : MonoBehaviour {

	private Vector3 startPoint;
	private Vector3 endPoint;
	private Vector3 direction;
	private float speed = 2f;
	private int layerMask = 1 << 10; //Vechiles only
	private float maxDistance = 1.5f;

	public bool showSensors = true;

	public void UpdatePosition()
	{
		Ray ray = new Ray (transform.position, direction);
		if (!Physics.Raycast (ray, maxDistance, layerMask)) 
		{
			transform.position += Time.fixedDeltaTime * speed * direction;
			if (showSensors)
				Debug.DrawLine (transform.position, transform.position + direction * maxDistance, Color.green);
		}
		else 
		{
			if (showSensors)
				Debug.DrawLine (transform.position, transform.position + direction * maxDistance, Color.red);
		}
	}

	public void SetPath(Vector3[] path)
	{
		transform.position = path [0];
		startPoint = path [0];
		endPoint = path [1];
		direction = (endPoint - startPoint).normalized;
	}

}
