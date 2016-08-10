using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : MonoBehaviour {

    public GameObject BZ;
    private bool hasControl;
    public float speed = 10f;
	// Use this for initialization
	void Start () {
        hasControl = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (hasControl)
            Move();
        else Turn();
	}

    void Move ()
    {
        transform.position += transform.forward * speed * Time.deltaTime; //Too naive
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "IntersectionManager") //TODO: Store types in a config file
        {
            Debug.Log("Entered IM");
            hasControl = false;
            StartCoroutine("Turn");
        }
    }

    IEnumerator Turn() //TODO: Separate, move to AIMController
    {
        int steps = 30; //Number of points on the Bezier curve 
        for(int i = 0; i < steps; ++i)
        {
            Vector3 toPoint = BZ.GetComponent<BezierCurve>().GetPointAt((float)(steps-1-i) /(steps-1));
            Vector3 dir = toPoint - transform.position;
            // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // cube.transform.position = toPoint;
            transform.LookAt(toPoint);
            transform.position += dir * Time.deltaTime * speed;
            //Debug.Log(toPoint);
            yield return null;
        }
        hasControl = true;
        //transform.position = Vector3.Slerp
    }
}
