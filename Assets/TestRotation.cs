using UnityEngine;
using System.Collections;

public class TestRotation : MonoBehaviour {

	public GameObject other;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 dir = other.transform.position - transform.position;
		float angle = Vector2.Angle (transform.up, dir);
		//angle = Vector3.Cross(transform.up, dir).z < 0 ? 360 - angle : angle;
		Debug.Log ("Angle " + angle + ", Cross " + Vector3.Cross(transform.up, dir).z);
	}
}
