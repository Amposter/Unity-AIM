using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //Zooming
    public float zoomIncr = 5;
    public float zoomAmplify = 13;
    public float zoomAmplifyUpperBound = 22;
    public float zoomAmplifyLowerBound = 1;
    public float zoomAmplifyIncr = 0.8f;

    //Dragging
    private Vector3 oldPos;
    private Vector3 panOrigin;
    private bool dragging;
    public float dragSpeed = 5;
    void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Zooming
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        Camera camera = transform.GetComponent<Camera>();
        camera.fieldOfView += zoomAmplify * zoom * zoomIncr * Time.deltaTime;
        zoomAmplify -= zoom * zoomAmplifyIncr;
        if (zoomAmplify < zoomAmplifyLowerBound)
            zoomAmplify = 1;
        else if (zoomAmplify > zoomAmplifyUpperBound)
            zoomAmplify = 22;

        //Dragging
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            oldPos = transform.position;
            panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);                    //Get the ScreenVector the mouse clicked
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin;    //Get the difference between where the mouse clicked and where it moved
            transform.position = oldPos + -pos * dragSpeed;                                         //Move the position of the camera to simulate a drag, speed * 10 for screen to worldspace conversion
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;
    }
}
