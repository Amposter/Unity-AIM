﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class AIMController : SimpleHeuristicController {

    //For Debugging
    public bool _nextLaneFull;
    public float[] _times;
    public Vector3[] _positions;
    public int _bookedPositions;
    public GameObject _colliderObject;
    public string _colliderName;
    public float _colliderDotAngle;

    //Private variables
    private float requestCooldown = 2.0f;
    private TrackWayPoint.Type nextWayPointType;
    private IntersectionManager nextIM;
    private TrackWayPoint[] wayPoints;
    private bool requestGranted;
    private PathManager pm;
    private int VIN;
    private Action action;
    private BezierCurve currCurve;
    private IntersectionManager debugIM;
    private BezierCurve[] path;
    private int nextDir; //Current curve
    private int steps; //Number of bezier points to sample
    private float reservationOffset;
    private int decimalRound; 
    private float[] debugTime;

    //Public variables
    public TrackWayPoint start;
    public int debugStart;
    public int debugEnd;
    public GameObject debugIMObject;
    public enum Controller {AIM, HEURISTIC};
    public Controller controller;
   
	float updateInterval = 0.2f;
	float updateTimer = 0;
	public CarSpawner carSpawner;
	public Vector2 sp; //Remove!

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        string name = GameObject.FindGameObjectWithTag("Track").name;
        name = (name[name.Length - 2].ToString() + name[name.Length - 1]);
        int level = int.Parse(name);
        steps = Config.samplingSteps[level-1];
        decimalRound = Config.decimalPlaces[level-1];
        reservationOffset = Config.reserverationOffsets[level-1];
        debugIM = debugIMObject.GetComponent<IntersectionManager>();
        //VIN = ++Config.lastVIN;
        nextDir = 0;
        requestGranted = false;
        debugTime = new float[steps];
        int val = debugIM.debugSpawnCounter;
        //TODO
        //call public TrackWayPoint[] getRandomPathNodesFromStartNode(TrackWayPoint start)
        //store that and then use result and call
        //public BezierCurve[] getCurvesFromPathNodes(TrackWayPoint[] waypointList)
        wayPoints = GameObject.Find("PathManager").GetComponent<PathManager>().getRandomPathNodesFromStartNode(start);//.getDebugPathCurves(debugIM.debugSpawnLocations[val], debugIM.debugSpawnLocations[val + 1]); //
        path = GameObject.Find("PathManager").GetComponent<PathManager>().getCurvesFromPathNodes(wayPoints); debugIM.debugSpawnCounter += 2;
        Vector2 startPoint = path[0].GetPointAt(0f);
        //startPoint.z = -10f; //so it appears in front of the 3d objects in scene, i.e., the tracks and ground
        transform.position = startPoint;
		float angle = 360 - Vector2.Angle (transform.up, (Vector2)(path[0].GetPointAt(0.1f) - path[0].GetPointAt(0f))); //If the rotation goes wonky, these 2 lines might be why
		transform.rotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward);
		setCurve(path[nextDir]);

		//Vector3 dir = path [0].GetPointAt (0.1f) - path [0].GetPointAt (0);
		//float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
		//transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
       
		path[nextDir].incr();
        nextDir++;
        controller = Controller.HEURISTIC;
        StartCoroutine("Drive");

        //To auto drive:  comment out StartCoroutine("Drive") and uncomment these below, need to handle pausing differently ofc
		//Debug.DrawLine(transform.position,transform.position+transform.up.normalized*5,Color.red,10f);
		//setCurves(path);
        //StartCoroutine("AutoDrive");
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();

		updateTimer += Time.deltaTime;

		if (paused)
		{
			carSpawner.idleTime += Time.deltaTime;
		}

		if (updateTimer >= updateInterval)
		{
			updateTimer = 0;

			if (!paused)
			{
				carSpawner.totalSpeedAccumulator += speed;
			}
			carSpawner.totalSpeedCheckCount++;

		}
    }

    void OnTriggerEnter2D(Collider2D col)
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
            transform.position += transform.forward.normalized * 500; //To ensure OnTriggerExit is called of vehicles that may have been paused due to this one.
            path[nextDir - 1].decr(transform.name);
			carSpawner.carsThrough++;
            Destroy(gameObject,0.1f);
        }

        else if (nextWayPointType == default(TrackWayPoint.Type))
        {
            Debug.LogError("ERROR: End of curve but no next way point set!");
        }

        else if (nextWayPointType == TrackWayPoint.Type.NORMAL)
        {
            controller = Controller.HEURISTIC;
            path[nextDir].incr();
            path[nextDir - 1].decr(transform.name);
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
                KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[] requestPath = SimulatePath();
                object[] parameters = new object[] { requestPath, nextIM};
                StartCoroutine("MakeRequest", parameters);
            }
        }
   
        nextWayPointType = default(TrackWayPoint.Type);
    }

    //Compare the next path against 'curve'
    public bool ComparePath(BezierCurve curve)
    {
        if (controller == Controller.AIM)
            return curve == path[nextDir];
        else
            return curve == path[nextDir-1];
    }

    IEnumerator Turn() 
    {
        controller = Controller.AIM;


        List<BezierCurve> curves = new List<BezierCurve>();
        curves.Add(path[nextDir++]);
        while (wayPoints[nextDir].type != TrackWayPoint.Type.INTERSECTION_BORDER)
            curves.Add(path[nextDir++]);

        currCurve = curves[0];

        foreach (BezierCurve curve in curves)
        {
            int counter = 1;
            //Debug.Log("Initial Time Actual: " + Time.time);
            Vector2 toPoint = curve.GetPointAt(counter / (float)steps);
			transform.rotation *= Quaternion.FromToRotation(transform.up, (toPoint - (Vector2)transform.position).normalized);


            while (counter <= steps)
            {
				Vector2 travelVector = (toPoint - (Vector2)transform.position);
                Vector2 dir = travelVector.normalized;
				Vector2 newPos = (Vector2)transform.position + speed * dir * Time.fixedDeltaTime;
                /*   if (Mathf.Approximately((float)Math.Round(Time.time,2), (float)Math.Round(t + tOffset,2)))
                   {
                       Debug.Log(t + tOffset + " and " + Time.time);
                       t = (float)Math.Round(t+tOffset,1);

                   }*/
				float distanceToTravel = travelVector.magnitude;
				if (distanceToTravel <= speed * Time.fixedDeltaTime) //Newpos oversteps the next waypoint
				{ 
					++counter;
					toPoint = curve.GetPointAt((float)counter/steps);
					transform.rotation *= Quaternion.FromToRotation(transform.up, dir);
				}
				transform.position = newPos; 
				yield return new WaitForFixedUpdate();
            }
        }
        Act();
    }
		

    KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[] SimulatePath()
    {
        Color colour = new Color(UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f));
        List<KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>> output = new List<KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>>();
        List<BezierCurve> curves = new List<BezierCurve>();
        int temp = nextDir;
        curves.Add(path[temp++]);
        while (wayPoints[temp].type != TrackWayPoint.Type.INTERSECTION_BORDER)
            curves.Add(path[temp++]);
              

        //BezierCurve[] curves = { path[nextDir] };
        float timeOffset = reservationOffset;
        float distToTimeStep = timeOffset * speed;
        float time = (float)Math.Round(Time.time, decimalRound);
		GameObject bounds = GameObject.Instantiate(gameObject);
        Vector2 simulatedPos = transform.position;
        //simulatedPos.y = 0.5f;

        foreach (BezierCurve curve in curves)
        {
            int counter = 1;
            Vector2 toPoint = curve.GetPointAt(counter / (float)steps);
            //toPoint.y = simulatedPos.y;

            while (counter <= steps)
            {
                Vector2 travelVector = (toPoint - simulatedPos);
                Vector2 dir = travelVector.normalized;
                float distToPointStep = travelVector.magnitude;

                if (distToTimeStep <= distToPointStep) //The position we are looking for is in this segment of the curve
                {
					bounds.transform.rotation *= Quaternion.FromToRotation(bounds.transform.up, dir);// Quaternion.LookRotation(toPoint - simulatedPos);
                    simulatedPos += dir * distToTimeStep; //Move to the point of interest
                    time = (float)Math.Round(time + timeOffset, decimalRound);
					bounds.transform.position = simulatedPos;
					//Debug.Log(time);
                    //distToPointStep -= distToTimeStep;
                    distToTimeStep = timeOffset * speed;
                    //Debug.Log(time);
					output.Add(new KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>(time, new KeyValuePair<Vector2, Quaternion>(simulatedPos, bounds.transform.rotation)));
                 /*   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = simulatedPos;
                    cube.transform.rotation = rotation;
                    cube.name = time.ToString();
                    cube.GetComponent<MeshRenderer>().material.color = colour;
                    cube.transform.localScale = transform.GetComponent<BoxCollider>().size * 1.15f;
                    Destroy(cube, 1.0f);*/

                }
                else
                {
                    simulatedPos = toPoint;
					bounds.transform.position = simulatedPos;
                    distToTimeStep = (distToTimeStep - distToPointStep);
                    ++counter;
                    toPoint = curve.GetPointAt(counter / (float)steps);
                    //toPoint.y = simulatedPos.y;
                }
            }
        }
		Destroy (bounds);
        _bookedPositions = output.Count;
        _times = new float[output.Count];
        _positions = new Vector3[output.Count];
        int i = 0;
        foreach (KeyValuePair<float, KeyValuePair<Vector2, Quaternion>> item in output)
        {
            _times[i] = item.Key;
            _positions[i] = item.Value.Key;
            ++i;   
        }
        return output.ToArray();
    }


    IEnumerator MakeRequest(object[] parameters)
    {
        int temp = nextDir;
        int offset = 1;
        while (wayPoints[temp+offset].type != TrackWayPoint.Type.INTERSECTION_BORDER)
            ++offset;


        KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[] requestPath = (KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[])parameters[0];
        IntersectionManager im = (IntersectionManager)parameters[1];

        while (true)
        {

            requestGranted = im.Reserve(requestPath);
            if (requestGranted && path[nextDir + offset].incr())
            {
                //path[nextDir+offset].drawColor = Color.red;
                //SceneView.RepaintAll();
                path[nextDir - 1].decr(transform.name);
                _nextLaneFull = path[nextDir + offset].full();
                break;
            }
            _nextLaneFull = path[nextDir + offset].full();
            yield return new WaitForSeconds(requestCooldown);
            requestPath = SimulatePath(); 
        }

        requestGranted = false;
        StartCoroutine("Turn");
        yield return null;
    }
}
