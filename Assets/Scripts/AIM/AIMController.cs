using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    public enum Direction //TODO: Put into a Util file
    {
        LEFT,
        RIGHT,
        STRAIGHT
    };

    public Direction[] path;
    public GameObject currLane;
    private int nextDir; //Next direction index in the 'path' list to take
    private int steps = 5; //Number of bezier points to sample 

    // Use this for initialization
    protected override void Start () {
        base.Start();
        nextDir = 0;
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
            SimulatePath();
            StartCoroutine("Turn");
        }

        if (col.gameObject.tag == "SourcePoint")
        {
            Destroy(gameObject);
        }
    }
    //TODO: Fix interpolation, possible use of Tweens
    IEnumerator Turn() 
    {
        Direction dir = path[nextDir];
        ++nextDir;
        BezierCurve curve = null; //TODO: Fix unassigned ref error (not by making this null)

        if (dir == Direction.RIGHT)
        {
            curve = currLane.GetComponent<Lane>().rightPath.GetComponent<BezierCurve>();
            currLane = currLane.GetComponent<Lane>().right;
        }
        else if (dir == Direction.LEFT)
        {
            curve = currLane.GetComponent<Lane>().leftPath.GetComponent<BezierCurve>();
            currLane = currLane.GetComponent<Lane>().left;
        }
        else if (dir == Direction.STRAIGHT)
        {
            currLane = currLane.GetComponent<Lane>().straight;
            driving = true;
            Debug.Log("Going Straight");
            yield break;
        }
        int counter = 1;
        Debug.Log("Initial Time Actual: " + Time.time);
        Vector3 toPoint = curve.GetPointAt((float)(steps - counter) / (steps));
        toPoint.y = transform.position.y;
        transform.LookAt(toPoint);
        while (counter <= steps)
        {
           // Vector3 dira = toPoint - transform.position;
            // GameObject cube = Gam4eObject.CreatePrimitive(PrimitiveType.Cube);
            // cube.transform.position = toPoint;
            //transform.LookAt(new Vector3(toPoint.x, transform.position.y, toPoint.z));
           // transform.position += dira * Time.deltaTime * speed;
              Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.deltaTime);
              transform.LookAt(newPos);
              transform.position = newPos; //transform.position = new Vector3(toPoint.x, transform.position.y, toPoint.z);*/
          //  transform.position += transform.forward * speed * Time.deltaTime;
            if (transform.position == toPoint)
            {
               // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                 Debug.Log("CurrTime: " + Time.time + " " + transform.position);
                //cube.transform.position = new Vector3(toPoint.x, transform.position.y, toPoint.z);
                ++counter;
                toPoint = curve.GetPointAt((float)(steps - counter) / (steps));
                toPoint.y = transform.position.y;
                transform.LookAt(toPoint);
            }
            //Debug.Log(toPoint);
            yield return null;
        }
        driving = true;
        Debug.Log("Left IM");
    }

    KeyValuePair<float, Vector3>[] SimulatePath()
    {
        KeyValuePair<float, Vector3>[] output = new KeyValuePair<float, Vector3>[steps];
        Direction dir = path[nextDir];
        BezierCurve curve = null; //TODO: Fix unassigned ref error (not by making this null)

        if (dir == Direction.RIGHT)
        {
            curve = currLane.GetComponent<Lane>().rightPath.GetComponent<BezierCurve>();
        }
        else if (dir == Direction.LEFT)
        {
            curve = currLane.GetComponent<Lane>().leftPath.GetComponent<BezierCurve>();
        }
        else if (dir == Direction.STRAIGHT) //Take this straight checkout, dont call method if straight
        {
            driving = true;
            Debug.Log("Going Straight");
            return null;
        }

        float initialTime = Time.time;
        Debug.Log("Initial time simulation: " + initialTime);
        float distance = 0;
        Vector3 curr = transform.position; // curve.GetPointAt(0.8f);
        Vector3 next;
        int res = 15; //Number of points used to approximate length of curve segment
        for (int s = 1; s < steps+1; ++s)
        {

            float segment = (1.0f - (s/ (float)steps)); // s -> (s-1)
            Vector3 point = curve.GetPointAt(segment); 
            point.y = transform.position.y;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = point;
            /*  for (int i = 1; i < res + 1; ++i)
              {
                  next = curve.GetPointAt(segment - ((i / (float)res) * (1/(float)steps)));
                  next.y = curr.y;
                  distance += (next - curr).magnitude;
                  curr = next;
              }*/
            next = curve.GetPointAt(segment);
            next.y = curr.y;
            distance += (next - curr).magnitude;
            curr = next;

            float elapsedTime = distance / speed;
         //   Debug.Log(elapsedTime + initialTime);
            output[s - 1] = new KeyValuePair<float, Vector3>(initialTime+elapsedTime, point);
          //  Debug.Log("Predicted time: " + (Time.time + time));
        }
        Debug.Log(output[0].ToString());
        Debug.Log(output[1].ToString());
        Debug.Log(output[2].ToString());
        Debug.Log(output[3].ToString());
        Debug.Log(output[4].ToString());
        return output;
    }
}
