using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    //Private variables
    private float requestCooldown = 2.0f;
    private TrackWayPoint.Type nextWayPointType;
    private IntersectionManager nextIM;
    private bool requestGranted;
    private PathManager pm;
    private int VIN;
    private Action action;
    private IntersectionManager debugIM;
    private BezierCurve[] path;
    private int nextDir; //Current curve
    private int steps = 7; //Number of bezier points to sample 
    private float[] debugTime;

    //Public variables
    public int debugStart;
    public int debugEnd;
    public GameObject debugIMObject;
    public enum Controller {AIM, HEURISTIC};
    public Controller controller;
   
    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        debugIM = debugIMObject.GetComponent<IntersectionManager>();
        //VIN = ++Config.lastVIN;
        nextDir = 0;
        requestGranted = false;
        debugTime = new float[steps];
        int val = debugIM.debugSpawnCounter;
        path = GameObject.Find("PathManager").GetComponent<PathManager>().getDebugPathCurves(debugIM.debugSpawnLocations[val], debugIM.debugSpawnLocations[val+1]); //.getRandomPathCurves();
        debugIM.debugSpawnCounter += 2;
        Vector3 startPoint = path[0].GetPointAt(0.0f);
        startPoint.y = 0.5f;
        transform.position = startPoint;
        transform.LookAt(startPoint);
        setCurve(path[nextDir]);
        nextDir++;
        controller = Controller.HEURISTIC;
        base.driving = true;
        StartCoroutine("Drive");

    }

    // Update is called once per frame
    protected override void Update () {
        base.Update();
        Debug.Log(nextDir);
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

        if (col.gameObject.tag == "IntersectionManager")
        {
            nextIM = col.gameObject.GetComponent<IntersectionManager>();
        }
            /*  if (col.gameObject.tag == "IntersectionManager") //TODO: Store types in a config file
              {
                  endOfCurve = true;
                  driving = false;
                  IntersectionManager im = col.gameObject.GetComponent<IntersectionManager>();
                  KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = SimulatePath();
                  Action action;
                  action = Action.TURN;
                  object[] parameters = new object[] {requestPath, im, action};
                  StartCoroutine("MakeRequest", parameters);
              }*/
            if (col.gameObject.tag == "WayPoint")
        {
            nextWayPointType = col.gameObject.GetComponent<TrackWayPoint>().type;
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
            //StartCoroutine("Drive");
        }
    }

    protected override IEnumerator Drive()
    {
        yield return StartCoroutine(base.Drive());
        //Debug.Log("finished");
        Act();
    }

    void Act()
    {
        if (nextWayPointType == default(TrackWayPoint.Type))
        {
            Debug.LogError("ERROR: End of curve but no next way point set!");
        }
        else if (nextWayPointType == TrackWayPoint.Type.NORMAL)
        {
            controller = Controller.HEURISTIC;
            setCurve(path[nextDir]);
            ++nextDir;
            StartCoroutine("Drive");
        }
        else if (nextWayPointType == TrackWayPoint.Type.INTERSECTION_BORDER)
        {
            if (controller == Controller.AIM) //If we're leaving intersection
            {
                setCurve(path[nextDir]);
                ++nextDir;
                StartCoroutine("Drive");
            }
            else
            {
                controller = Controller.AIM;
                KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = SimulatePath();
                object[] parameters = new object[] { requestPath, nextIM};
                StartCoroutine("MakeRequest", parameters);
            }
        }
        else if (nextWayPointType == TrackWayPoint.Type.INTERSECTION_BORDER)
        {
            Destroy(this);
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
        //driving = true;
        //StartCoroutine("Drive");
        Act();
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

        while (true)
        {
            requestGranted = im.Reserve(requestPath);
            if (requestGranted)
                break;
            Debug.Log("Request not granted");
            yield return new WaitForSeconds(requestCooldown);
            requestPath = SimulatePath(); //Update times if rejected
        }

        requestGranted = false;
        StartCoroutine("Turn");

        yield return null;
    }
}
