using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AIMController : SimpleHeuristicController {

    //For Debugging
    public GameObject _colliderObject;
    public string _colliderName;
    public float _colliderDotAngle;

    //Private variables
    private float requestCooldown = 2.0f;
    private TrackWayPoint.Type nextWayPointType;
    private IntersectionManager nextIM;
    private bool requestGranted;
    private PathManager pm;
    private int VIN;
    private Action action;
    private BezierCurve currCurve;
    private IntersectionManager debugIM;
    private BezierCurve[] path;
    private int nextDir; //Current curve
    private int steps = 7; //Number of bezier points to sample 
    private float[] debugTime;

    //Public variables
    public TrackWayPoint start;
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
        path = GameObject.Find("PathManager").GetComponent<PathManager>().getStartPathCurves(start);//.getDebugPathCurves(debugIM.debugSpawnLocations[val], debugIM.debugSpawnLocations[val + 1]); //
        debugIM.debugSpawnCounter += 2;
        Vector3 startPoint = path[0].GetPointAt(0.0f);
        startPoint.y = 0.5f;
        transform.position = startPoint;
        transform.LookAt(startPoint);
        setCurve(path[nextDir]);
        path[nextDir].incr();
        nextDir++;
        controller = Controller.HEURISTIC;
        StartCoroutine("Drive");

    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Vehicle" && controller != Controller.AIM)
        {
            Vector3 otherPosCentre = col.gameObject.transform.position;
            Vector3 dir = otherPosCentre - transform.position;
            //Debug.Log(Vector3.Dot(dir, transform.forward));
            if (Vector3.Dot(dir, transform.forward) > 0)
            {
                _colliderDotAngle = Vector3.Dot(dir, transform.forward);
                _colliderObject = col.gameObject;
                _colliderName = col.gameObject.name;
                Pause();  //This assumes you won't collide while turning i.e., no pedestrian spawners going through intersections
            }
        }

        if (col.gameObject.tag == "IntersectionManager")
            nextIM = col.gameObject.GetComponent<IntersectionManager>();

        if (col.gameObject.tag == "WayPoint")
        {
            //Debug.Log(col.gameObject.GetComponent<TrackWayPoint>().type);
            nextWayPointType = col.gameObject.GetComponent<TrackWayPoint>().type;
        }
     }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Vehicle" && controller != Controller.AIM)
        {
            Vector3 otherPosCentre = col.gameObject.transform.position;
            Vector3 dir = otherPosCentre - transform.position;
            //Debug.Log(Vector3.Dot(dir, transform.forward));
            if (Vector3.Dot(dir, transform.forward) > 0)
            {
                _colliderDotAngle = Vector3.Dot(dir, transform.forward);
                _colliderObject = col.gameObject;
                _colliderName = col.gameObject.name;
                Pause();  //This assumes you won't collide while turning i.e., no pedestrian spawners going through intersections
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Vehicle")
        {
            Vector3 otherPosCentre = col.gameObject.transform.position;
            Vector3 dir = otherPosCentre - transform.position;
            //Debug.Log(Vector3.Dot(dir, transform.forward));
            if (Vector3.Dot(dir, transform.forward) > 0)
            {
                _colliderDotAngle = -999.0f;
                _colliderName = "";
                _colliderObject = new GameObject();
                Unpause();
            }
        }
    }

    protected override IEnumerator Drive()
    {
        yield return StartCoroutine(base.Drive());
        Act();
    }

    void Act()
    {
        if (nextWayPointType == TrackWayPoint.Type.END)
        {
            transform.position += transform.forward.normalized * 500; //To ensure OnTriggerExit is called of vehicles that may have been paused due to this one
            path[nextDir - 1].decr();
            Destroy(gameObject,0.1f);
        }

        else if (nextWayPointType == default(TrackWayPoint.Type))
        {
            Debug.LogError("ERROR: End of curve but no next way point set!");
        }

        else if (nextWayPointType == TrackWayPoint.Type.NORMAL)
        {
            controller = Controller.HEURISTIC;
            setCurve(path[nextDir]);
            currCurve = path[nextDir];
            ++nextDir;
            StartCoroutine("Drive");
        }
        else if (nextWayPointType == TrackWayPoint.Type.INTERSECTION_BORDER)
        {
            if (controller == Controller.AIM) //If we're leaving intersection
            {
                controller = Controller.HEURISTIC;
                setCurve(path[nextDir]);
                ++nextDir;
                StartCoroutine("Drive");
            }
            else //We're entering the intersection
            {
                KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = SimulatePath();
                object[] parameters = new object[] { requestPath, nextIM};
                StartCoroutine("MakeRequest", parameters);
            }
        }
   
        nextWayPointType = default(TrackWayPoint.Type);
    }

    //Compare the next path against 'curve'
    public bool ComparePath(BezierCurve curve)
    {
        Debug.Log(gameObject.name);
        if (controller == Controller.AIM)
            return curve == path[nextDir];
        else
            return curve == path[nextDir-1];
    }

    IEnumerator Turn() 
    {
        controller = Controller.AIM;
        BezierCurve curve = path[nextDir];
        currCurve = curve;
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
         /*   if (Mathf.Approximately((float)Math.Round(Time.time,2), (float)Math.Round(t + tOffset,2)))
            {
                Debug.Log(t + tOffset + " and " + Time.time);
                t = (float)Math.Round(t+tOffset,1);
                
            }*/
            float excessTime = 0f;
            float distanceToTravel = travelVector.magnitude;
            if (distanceToTravel <= speed*Time.deltaTime)
            {
                if (distanceToTravel / (speed * Time.deltaTime) > 0.75)
                { }
                else
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
                    //Debug.Log("Correct: " + debugTime[counter - 1] + " Curr time: " + Time.time + " Rounded: " + Math.Round(Time.time, 1));
                    if (Mathf.Approximately(debugTime[counter - 1], (float)Math.Round(Time.time, 1)))
                    {
                       // Debug.Log("Corrected");
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
               // Debug.Log(debugTime[counter - 1] + " " + (float)Math.Round(Time.time,1));
                ++counter;
                toPoint = curve.GetPointAt((float)counter/steps);
                toPoint.y = transform.position.y;
                transform.LookAt(toPoint);
            }
            yield return null;
        }

        Act();
        //Debug.Log("Positions correct: " + debugCorectPositions);
    }

 /*   KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] SimulatePath()
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
      /*  Vector3 curr = transform.position;
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
    } */

    KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] SimulatePath()
    {
        List<KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>> output = new List<KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>>();
        BezierCurve curve = path[nextDir];
        //Debug.Log("Simulating Path");

        float time = (float)Math.Round(Time.time,1);
        float timeOffset = 0.05f;
        float landmarkDistance = timeOffset * speed;

        Vector3 simulatedPos = transform.position;
        int counter = 1;
        Vector3 toPoint = curve.GetPointAt(counter / (float)steps);
        toPoint.y = simulatedPos.y;
        while (counter <= steps)
        {
            Vector3 travelVector = (toPoint - simulatedPos);
            Vector3 dir = travelVector.normalized;
            float distToPointStep = travelVector.magnitude;
            float distToTimeStep = timeOffset * speed;

            if (distToTimeStep < distToPointStep) //The position we are looking for is in the next segment of the curve
            {
                Quaternion rotation = Quaternion.LookRotation(toPoint - simulatedPos);
                simulatedPos += dir * distToTimeStep; //Move to the point of interest
                time = (float)Math.Round(time + timeOffset, 1);
                //Debug.Log(time);
                output.Add(new KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>(time, new KeyValuePair<Vector3, Quaternion>(simulatedPos,rotation)));
               /* GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = simulatedPos;
                cube.transform.rotation = rotation;*/

            }
            else
            {
                simulatedPos = toPoint;
                ++counter;
                toPoint = curve.GetPointAt(counter / (float)steps);
            }
        }
        //Debug.Log("Positions recorded: " + output.Count);
        return output.ToArray();
    }


    IEnumerator MakeRequest(object[] parameters)
    {
        //yield return new WaitForSeconds(0.5f);
        KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] requestPath = (KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[])parameters[0];
        IntersectionManager im = (IntersectionManager)parameters[1];

        while (true)
        {
  /*          BezierCurve nextLane = path[nextDir + 1];
            Vector3 lastPos = nextLane.GetPointAt(0) + (nextLane.GetPointAt(0.2f) - nextLane.GetPointAt(0)).normalized * (2.05f * 1.1f); //positions[positions.Length - 1].Value.Key;
            lastPos.y = 0.5f;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = lastPos;
            Destroy(cube,1.0f);
            Collider[] colliders = Physics.OverlapBox(lastPos, new Vector3(0.5f,0.5f,0.5f));//Physics.OverlapSphere(positions[positions.Length-1].Value.Key, 2);
            foreach (Collider col in colliders)
            {
                if (col.gameObject.tag == "Vehicle" && col.gameObject.GetComponent<AIMController>().Paused())
                {
                    yield return new WaitForSeconds(1.5f);
                    break;
                }
            }*/
            requestGranted = im.Reserve(requestPath);
            if (requestGranted && path[nextDir + 1].incr())
            {
                path[nextDir - 1].decr();
                break;
            }
            //Debug.Log("Request not granted");
            yield return new WaitForSeconds(requestCooldown);
            requestPath = SimulatePath(); //Update times if rejected
        }

        requestGranted = false;
        StartCoroutine("Turn");
        yield return null;
    }
}
