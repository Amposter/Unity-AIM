using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    public float requestCooldown = 2.0f;
    private bool requestGranted;
    private PathManager pm;
    private Action action;

    public enum Direction //TODO: Put into a Util file
    {
        LEFT,
        RIGHT,
        STRAIGHT
    };

    public enum Action
    {
        IDLE,
        DRIVE,
        TURN
    };

    public BezierCurve[] path;
    public GameObject currLane;
    private int nextDir; //Next direction index in the 'path' list to take
    private int steps = 4; //Number of bezier points to sample 

    // Use this for initialization
    protected override void Start () {
        base.Start();
        nextDir = 0;
        requestGranted = false;
        action = Action.DRIVE;
        BezierCurve[] allPaths = GameObject.Find("PathManager").GetComponent<PathManager>().getRandomPathCurves();
        BezierCurve[] heuristicPath = new BezierCurve[allPaths.Length/2 + 1];
        BezierCurve[] AIMPath = new BezierCurve[allPaths.Length/2];
        for (int i = 0; i < allPaths.Length; ++i)
        {
            if (i % 2 == 0)
                heuristicPath[i / 2] = allPaths[i];
            else
                AIMPath[i / 2] = allPaths[i];
        }
        path = AIMPath;
        curves = heuristicPath;
        StartCoroutine("Drive");
        /*        Vector3 startPoint = path[0].GetPointAt(0.0f);
                startPoint.y = 0;
                transform.position = startPoint;
                transform.LookAt(startPoint); */
    }
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Obstacle")
        {
            Debug.Log("Stopped for pedestrian/obstacle");
            driving = false;
        }

        if (col.gameObject.tag == "IntersectionManager") //TODO: Store types in a config file
        {
            Debug.Log("Entered IM");
            endOfCurve = true;
            driving = false;
            IntersectionManager im = col.gameObject.GetComponent<IntersectionManager>();
            KeyValuePair<float, Vector3>[] requestPath = SimulatePath();
       //     Debug.Log(path[nextDir]);
            Action action;
       /*     if (path[nextDir] == Direction.STRAIGHT)
                action = Action.DRIVE;
            else*/
                action = Action.TURN;
            object[] parameters = new object[] {requestPath, im, action};
            //StartCoroutine(MakeRequest, path, im, Action.TURN);
            StartCoroutine("MakeRequest", parameters);
        }

        if (col.gameObject.tag == "SourcePoint")
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Obstacle")
        {
            driving = true;
            Debug.Log("Continued");
            StartCoroutine("Drive");
        }
    }
    //TODO: Fix interpolation, possible use of Tweens
    IEnumerator Turn() 
    {
       // Direction dir = path[nextDir];
       // ++nextDir;
        BezierCurve curve = path[nextDir]; //TODO: Fix unassigned ref error (not by making this null)
        ++nextDir;
  /*      if (dir == Direction.RIGHT)
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
        }*/

        int counter = 1;
        Debug.Log("Initial Time Actual: " + Time.time);
        Vector3 toPoint = curve.GetPointAt(counter/(float)steps);
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
               //  Debug.Log("CurrTime: " + Math.Round(Time.time,1) + " " + transform.position);
                //cube.transform.position = new Vector3(toPoint.x, transform.position.y, toPoint.z);
                ++counter;
                toPoint = curve.GetPointAt((float)counter/steps);
                toPoint.y = transform.position.y;
                transform.LookAt(toPoint);
            }
            yield return null;
        }
        driving = true;
        Debug.Log("Left IM");
        StartCoroutine("Drive");
    }

    KeyValuePair<float, Vector3>[] SimulatePath()
    {
        KeyValuePair<float, Vector3>[] output = new KeyValuePair<float, Vector3>[steps];
        //Direction dir = path[nextDir];
        //BezierCurve curve = null; //TODO: Fix unassigned ref error (not by making this null)

        BezierCurve curve = path[nextDir]; //TODO: Fix unassigned ref error (not by making this null)
                                           //++nextDir;
        Debug.Log(curve.GetPointAt(0) + ", " + curve.GetPointAt(1));

        /*if (dir == Direction.RIGHT)
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
        }*/

        float initialTime = (float)Time.time;
        Debug.Log("Initial time simulation: " + initialTime);
        float distance = 0;
        Vector3 curr = transform.position; // curve.GetPointAt(0.8f);
        Vector3 next;
        int res = 15; //Number of points used to approximate length of curve segment
        for (int s = 1; s < steps+1; ++s)
        {

            float segment = (s/ (float)steps); // s -> (s-1)
            Vector3 point = curve.GetPointAt(segment); 
            point.y = transform.position.y;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<BoxCollider>().isTrigger = true;
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
            float totalTime = initialTime + elapsedTime;
         //   Debug.Log(elapsedTime + initialTime);
            output[s - 1] = new KeyValuePair<float, Vector3>((float)Math.Round(totalTime, 1), point);
          //  Debug.Log("Predicted time: " + (Time.time + time));
        }
     /*   Debug.Log(output[0].ToString());
        Debug.Log(output[1].ToString());
        Debug.Log(output[2].ToString());
        Debug.Log(output[3].ToString());*/
       // Debug.Log(output[4].ToString());
        return output;
    }


    IEnumerator MakeRequest(object[] parameters)//(KeyValuePair<float, Vector3>[] path, IntersectionManager im, Action action)
    {
        KeyValuePair<float, Vector3>[] requestPath = (KeyValuePair<float, Vector3>[])parameters[0];
        IntersectionManager im = (IntersectionManager)parameters[1];
        Action action = (Action)parameters[2];

        while (true)
        {
            requestGranted = im.Reserve(requestPath);
            Debug.Log("FROM MR: " + requestGranted);
            if (requestGranted)
                break;
            yield return new WaitForSeconds(requestCooldown);
        }

        if (action == Action.TURN)
        {
            Debug.Log("turning");
            requestGranted = false;
            StartCoroutine("Turn");
        }
        else if (action == Action.DRIVE)
        {
            driving = true;
        }
        yield return null;
    }
}
