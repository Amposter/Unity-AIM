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
        Vector3 startPoint = curves[0].GetPointAt(0.0f);
        startPoint.y = 0.5f;
        transform.position = startPoint;
        transform.LookAt(startPoint);
        StartCoroutine("Drive");

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

        BezierCurve curve = path[nextDir]; 
        ++nextDir;

        int counter = 1;
        Debug.Log("Initial Time Actual: " + Time.time);
        Vector3 toPoint = curve.GetPointAt(counter/(float)steps);
        toPoint.y = transform.position.y;
        transform.LookAt(toPoint);
        while (counter <= steps)
        {
            Vector3 travelVector = (toPoint - transform.position);
            Vector3 dir = travelVector.normalized;
            Vector3 newPos = transform.position + speed * dir * Time.deltaTime;

            float excessTime = 0f;
            float distanceToTravel = travelVector.magnitude;
            if (distanceToTravel < speed*Time.deltaTime)
            {
                // Debug.Log(speed*Time.deltaTime + " " + distanceToTravel);
                excessTime = speed*Time.deltaTime - distanceToTravel;
                newPos = toPoint;
            }
            //Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.deltaTime);
            transform.LookAt(newPos);
            transform.position = newPos;
            if (transform.position == toPoint)
            {
               // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                 Debug.Log("CurrTime: " + Time.time + " " + transform.position);
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

        BezierCurve curve = path[nextDir]; 

        float initialTime = (float)Time.time;
        Debug.Log("Initial time simulation: " + initialTime);
        float distance = 0;
        Vector3 curr = transform.position;
        Vector3 next;
        int res = 15; //Number of points used to approximate length of curve segment
        for (int s = 1; s < steps+1; ++s)
        {

            float segment = (s/ (float)steps); // s -> (s-1)
            Vector3 point = curve.GetPointAt(segment); 
            point.y = transform.position.y;
          //  GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
          //  cube.GetComponent<BoxCollider>().isTrigger = true;
          //  cube.transform.position = point;
        
            next = curve.GetPointAt(segment);
            next.y = curr.y;
            distance += (next - curr).magnitude;
            curr = next;

            float elapsedTime = distance / speed;
            float totalTime = initialTime + elapsedTime;
         //   Debug.Log(elapsedTime + initialTime);
            output[s - 1] = new KeyValuePair<float, Vector3>((float)Math.Round(totalTime, 1), point);
            Debug.Log("Predicted time: " + totalTime);//output[s-1].Key);
        }
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
