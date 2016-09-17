using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // Use this for initialization
    public float zoomIncr = 5.0f;
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        if (zoom != 0f)
            transform.forward += transform.forward.normalized * zoomIncr * Time.deltaTime;
	}
}
