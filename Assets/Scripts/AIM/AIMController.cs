using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    private float requestCooldown = 2.0f;
    private bool requestGranted;
    private PathManager pm;
    private int VIN;
    private Action action;
    public int debugStart;
    public int debugEnd;
    public GameObject debugIMObject;
    private IntersectionManager debugIM;

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
    private int steps = 7; //Number of bezier points to sample 
    private float[] debugTime;
    // Use this for initialization
    protected override void Start () {
        base.Start();
        debugIM = debugIMObject.GetComponent<IntersectionManager>();
        //VIN = ++Config.lastVIN;
        nextDir = 0;
        requestGranted = false;
        debugTime = new float[steps];
        action = Action.DRIVE;
        int val = debugIM.debugSpawnCounter;
        BezierCurve[] allPaths = GameObject.Find("PathManager").GetComponent<PathManager>().getDebugPathCurves(debugIM.debugSpawnLocations[val], debugIM.debugSpawnLocations[val+1]); //.getRandomPathCurves();
        debugIM.debugSpawnCounter += 2;
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
        if (nextDir > path.Length)
            Destroy(this);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Obstacle")
        {
            if (driving)
            {
                Debug.Log("Stopped for pedestrian/obstacle");
                driving = false;
            }
        }

        if (col.gameObject.tag == "IntersectionManager") //TODO: Store types in a config file
        {
            endOfCurve = true;
            driving = false;
            IntersectionManager im = col.gameObject.GetComponent<IntersectionManager>();
            KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = SimulatePath();
            Action action;
            action = Action.TURN;
            object[] parameters = new object[] {requestPath, im, action};
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
            StartCoroutine("Drive");
        }
    }
    IEnumerator Turn() 
    {
        bool debugCorectPositions = true;
        BezierCurve curve = path[nextDir]; 
        ++nextDir;

        int counter = 1;
        //Debug.Log("Initial Time Actual: " + Time.time);
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
            if (distanceToTravel <= speed*Time.deltaTime)
            {
               /* if (distanceToTravel / (speed * Time.deltaTime) > 0.75)
                { }
                else*/
                {
                    excessTime = speed * Time.deltaTime - distanceToTravel;
                    //Debug.Log(excessTime);
                    newPos = toPoint;
                }
            }
            else
            {
                if ((distanceToTravel - speed * Time.deltaTime) < speed * Time.deltaTime * 0.2f)
                {
                    Debug.Log("Correct: " + debugTime[counter - 1] + " Curr time: " + Time.time + " Rounded: " + Math.Round(Time.time, 1));
                    if (Mathf.Approximately(debugTime[counter - 1], (float)Math.Round(Time.time, 1)))
                    {
                        Debug.Log("Corrected");
                        newPos = toPoint;
                        //Debug.Log("Excess dist: " +  (distanceToTravel - speed * Time.deltaTime));
                    }
                }
            }
            transform.LookAt(newPos);
            transform.position = newPos;
            if (transform.position == toPoint)
            {
                //Debug.Log("CurrTime: " + Math.Round(Time.time,1) + " " + transform.position);
                Debug.Log(debugTime[counter - 1] + " " + (float)Math.Round(Time.time,1));
                if (!Mathf.Approximately(debugTime[counter - 1], (float)Math.Round(Time.time, 1)))
                    debugCorectPositions = false;
 
                ++counter;
                toPoint = curve.GetPointAt((float)counter/steps);
                toPoint.y = transform.position.y;
                transform.LookAt(toPoint);
            }
            yield return null;
        }
        driving = true;
        StartCoroutine("Drive");
        Debug.Log("Positions correct: " + debugCorectPositions);
    }

    KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] SimulatePath()
    {
        KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] output = new KeyValuePair<float, KeyValuePair< Vector3, Quaternion >>[steps];

        BezierCurve curve = path[nextDir]; 

        float initialTime = (float)Time.time;
        float distance = 0;
        //Debug.Log("Initial time simulation: " + initialTime);
       /* 
        Vector3 simulatedCar = transform.position;
        int counter = 1;
        Vector3 toPoint = curve.GetPointAt(counter / (float)steps);
        toPoint.y = simulatedCar.y;
        while (counter <= steps)
        {
            Vector3 travelVector = (toPoint - simulatedCar);
            Vector3 dir = travelVector.normalized;
            Vector3 newPos = simulatedCar + speed * dir;
            float incrDistance = speed;
            float distanceToTravel = travelVector.magnitude;
            if (distanceToTravel <= speed)
            {
                // Debug.Log(speed*Time.deltaTime + " " + distanceToTravel);
                newPos = toPoint;
                incrDistance = distanceToTravel;
            }
            //Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.deltaTime);
            simulatedCar = newPos;
            distance += incrDistance;
            if (simulatedCar == toPoint)
            {
                output[counter - 1] = new KeyValuePair<float, Vector3>((float)Math.Round(time, 1), simulatedCar);
                Debug.Log("Predicted time: " + time);//output[s-1].Key);
                ++counter;
                incrDistance = 0f;
                toPoint = curve.GetPointAt((float)counter / steps);
                toPoint.y = simulatedCar.y;
            }
        }
        return output;*/
        Vector3 curr = transform.position;
        Vector3 next;
        for (int s = 1; s < steps+1; ++s)
        {

            float segment = (s/ (float)steps);
            Vector3 point = curve.GetPointAt(segment); 
            point.y = transform.position.y;

            next = curve.GetPointAt(segment);
            next.y = curr.y;
            Quaternion rotation = Quaternion.LookRotation(next - curr);
            distance += (next - curr).magnitude;
            curr = next;

            float elapsedTime = distance / speed;
            float totalTime = initialTime + elapsedTime;
            output[s - 1] = new KeyValuePair<float, KeyValuePair<Vector3,Quaternion>>((float)Math.Round(totalTime, 1), new KeyValuePair<Vector3, Quaternion>(point,rotation));
            debugTime[s - 1] = output[s - 1].Key;
            Debug.Log("Predicted time: " + output[s-1].Key);
        }
        Debug.Log("*******************");
        return output;
    }


    IEnumerator MakeRequest(object[] parameters)
    {
        KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = (KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[])parameters[0];
        IntersectionManager im = (IntersectionManager)parameters[1];
        Action action = (Action)parameters[2];

        while (true)
        {
            requestGranted = im.Reserve(requestPath);
            if (requestGranted)
                break;
            Debug.Log("Request not granted");
            yield return new WaitForSeconds(requestCooldown);
            requestPath = SimulatePath(); //Update times if rejected
        }

        if (action == Action.TURN)
        {
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
