using UnityEngine;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    // Use this for initialization
    protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "IntersectionManager") //TODO: Store types in a config file
        {
            Debug.Log("Entered IM");
            driving = false;
            StartCoroutine("Turn");
        }
    }
    //TODO: Fix interpolation
    IEnumerator Turn() 
    {
        int steps = 5; //Number of points on the Bezier curve
        int counter = 1;

        while (counter <= steps)
        {
            Vector3 toPoint = BZ.GetComponent<BezierCurve>().GetPointAt((float)(steps - counter) / (steps));
            //Vector3 dir = toPoint - transform.position;
            // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // cube.transform.position = toPoint;
            //transform.LookAt(new Vector3(toPoint.x, transform.position.y, toPoint.z));
            //transform.position += dir * Time.deltaTime * speed;
            float step = speed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(transform.position, new Vector3(toPoint.x, transform.position.y, toPoint.z), step);
            transform.LookAt(newPos);
            transform.position = newPos; //transform.position = new Vector3(toPoint.x, transform.position.y, toPoint.z);
            if (transform.position == new Vector3(toPoint.x, transform.position.y, toPoint.z))
                ++counter;
            //Debug.Log(toPoint);
            yield return null;
        }
        driving = true;
        Debug.Log("Left IM");
        //transform.position = Vector3.Slerp
    }

}
