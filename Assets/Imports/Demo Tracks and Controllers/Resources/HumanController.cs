using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		//gameObject.GetComponent<Vehicle> ().setAccelerationMagnitude (Input.GetAxis ("Vertical"));
		//gameObject.GetComponent<Vehicle> ().setSteeringMagnitude (Input.GetAxis ("Horizontal"));
		//gameObject.GetComponent<Vehicle> ().setbrakingMagnitude (Input.GetAxis ("Jump"));
	}

	void LateUpdate()
	{
		Camera.main.transform.position = transform.position + transform.forward * -3 + transform.up * 1.5f;
		Camera.main.transform.LookAt (transform.position);
	}
}
