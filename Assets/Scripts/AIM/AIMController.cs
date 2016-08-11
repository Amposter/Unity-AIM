using UnityEngine;
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
            StartCoroutine("Turn");
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
        int steps = 5; //Number of points on the Bezier curve
        int counter = 1;

        while (counter <= steps)
        {
            Vector3 toPoint = curve.GetPointAt((float)(steps - counter) / (steps));
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
    }

}
