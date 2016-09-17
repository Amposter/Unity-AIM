using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


public class PathManager : MonoBehaviour
{
	public List<TrackWayPoint> startPoints = new List<TrackWayPoint>();
	public List<TrackWayPoint> endPoints = new List<TrackWayPoint>();

	//a list of curves to display as a path
	BezierCurve[] displayPath = null;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void findStartEndPoints()
	{
		startPoints.Clear ();
		endPoints.Clear ();

		foreach(TrackWayPoint point in gameObject.GetComponentsInChildren<TrackWayPoint>())
		{
			if (point.type == TrackWayPoint.Type.START)
			{
				startPoints.Add (point);
			}
			else if (point.type == TrackWayPoint.Type.END)
			{
				endPoints.Add (point);
			}
		}
	}

	public void showRandomPath()
	{
		clearDisplayPath ();
		displayPath = getRandomPathCurves ();

		foreach (BezierCurve curve in displayPath)
		{
			curve.drawColor = Color.blue;
		}
		SceneView.RepaintAll ();
	}

    public BezierCurve[] getDebugPathCurves(int start, int end)
    {
        TrackWayPoint pathStart = startPoints[start]; 
        TrackWayPoint pathEnd = endPoints[end]; 

        Transform[] transformPathList = getPathBetweenNodes(pathStart, pathEnd);
        BezierCurve[] curvePathList = new BezierCurve[transformPathList.Length - 1];

        for (int i = 0; i < transformPathList.Length - 1; ++i)
        {

            for (int j = 0; j < transformPathList[i].gameObject.GetComponent<TrackWayPoint>().nextPoints.Length; j++)
            {
                if (transformPathList[i].gameObject.GetComponent<TrackWayPoint>().nextPoints[j] == transformPathList[i + 1].gameObject.GetComponent<TrackWayPoint>())
                {
                    curvePathList[i] = transformPathList[i].gameObject.GetComponent<TrackWayPoint>().curveList[j];
                }
            }
        }
            /***/
            foreach (BezierCurve curve in curvePathList)
            {
                curve.drawColor = Color.blue;
            }
            SceneView.RepaintAll();

            /***/
            return curvePathList;
    }

    public BezierCurve[] getRandomPathCurves()
	{

        Transform[] transformPathList;
        do
        {
            TrackWayPoint pathStart = startPoints[Random.Range(0, startPoints.Count)]; //get random start node
            TrackWayPoint pathEnd = endPoints[Random.Range(0, endPoints.Count)]; //get random end node
            transformPathList = getPathBetweenNodes(pathStart, pathEnd);
        }
        while (transformPathList == null); //if there is no path between the start and end nodes then choose new start/end points
		
        
		BezierCurve[] curvePathList = new BezierCurve[transformPathList.Length-1];

		for (int i = 0; i < transformPathList.Length-1; ++i)
		{
			
			for(int j = 0; j < transformPathList[i].gameObject.GetComponent<TrackWayPoint>().nextPoints.Length; j++)
			{
				if (transformPathList [i].gameObject.GetComponent<TrackWayPoint> ().nextPoints [j] == transformPathList [i + 1].gameObject.GetComponent<TrackWayPoint> ())
				{
					curvePathList [i] = transformPathList [i].gameObject.GetComponent<TrackWayPoint> ().curveList [j];
				}
			}

		}

        /***/
        foreach (BezierCurve curve in curvePathList)
        {
            curve.drawColor = Color.blue;
        }
        SceneView.RepaintAll();

        /***/
        return curvePathList;
	}

    //A random path starting at start
    public BezierCurve[] getStartPathCurves(TrackWayPoint start)
    {
        TrackWayPoint pathStart = start; 
        TrackWayPoint pathEnd = endPoints[Random.Range(0, endPoints.Count)]; //get random end node

        Transform[] transformPathList = getPathBetweenNodes(pathStart, pathEnd);

        if (transformPathList == null)
        {
            return null; // return null if theres no path from start to finish
        }

        BezierCurve[] curvePathList = new BezierCurve[transformPathList.Length - 1];

        for (int i = 0; i < transformPathList.Length - 1; ++i)
        {

            for (int j = 0; j < transformPathList[i].gameObject.GetComponent<TrackWayPoint>().nextPoints.Length; j++)
            {
                if (transformPathList[i].gameObject.GetComponent<TrackWayPoint>().nextPoints[j] == transformPathList[i + 1].gameObject.GetComponent<TrackWayPoint>())
                {
                    curvePathList[i] = transformPathList[i].gameObject.GetComponent<TrackWayPoint>().curveList[j];
                }
            }

        }

        /***/
        foreach (BezierCurve curve in curvePathList)
        {
            curve.drawColor = Color.blue;
        }
        SceneView.RepaintAll();

        /***/
        return curvePathList;
    }

    public void clearDisplayPath()
	{
		if (displayPath == null) {
			return;
		}
		foreach (BezierCurve curve in displayPath)
		{
			curve.drawColor = Color.yellow;
		}
		displayPath = null;
		SceneView.RepaintAll ();
	}
		
	//crude implementation of dijkstras shortest path algorithm hammered into submission to work with our data structures 
	public Transform[] getPathBetweenNodes(TrackWayPoint pathStart,TrackWayPoint pathEnd)
	{
		List<TrackWayPoint> unvisitedPoints = new List<TrackWayPoint> (); // keep track of unvisited points
		Dictionary<TrackWayPoint, float> distances = new Dictionary<TrackWayPoint, float> (); //keep track of tentative distances from start point for all points

		foreach (TrackWayPoint point in gameObject.GetComponentsInChildren<TrackWayPoint>())
		{
			distances.Add (point, float.MaxValue);
			unvisitedPoints.Add (point);
		}
		distances [pathStart] = 0;

		TrackWayPoint current = pathStart;

		while (true)
		{
			if (unvisitedPoints.Contains (current))
			{
				foreach (TrackWayPoint nextPoint in current.nextPoints)
				{
					float distanceToNextNode = (current.transform.position - nextPoint.transform.position).magnitude;
					if (distances [current] + distanceToNextNode < distances [nextPoint])
					{
						distances [nextPoint] = distances [current] + distanceToNextNode;
						nextPoint.prevPoint = current;
					}
				}
				unvisitedPoints.Remove (current);
			}


			current = getPointWithSmallestTentativeDistance (unvisitedPoints, distances);

			if (current == pathEnd || current == null)
			{
				break;
			}
		}

		List<Transform> shortestPath = new List<Transform> ();

        //if there is no path between the start and end nodes then return null
        try
        {
            shortestPath.Add(current.transform);
        }
        catch(System.NullReferenceException e)
        {
            return null;
        }
		
		while (true)
		{
			shortestPath.Add (current.prevPoint.transform);
			current = current.prevPoint;
			if (current == pathStart)
			{
				break;
			}
		}

		shortestPath.Reverse();
		return shortestPath.ToArray ();
	}

	public TrackWayPoint getPointWithSmallestTentativeDistance(List<TrackWayPoint> unvisitedPoints, Dictionary<TrackWayPoint, float> distances)
	{
		float minDistance = float.MaxValue;
		TrackWayPoint result = null;

		foreach(TrackWayPoint point in unvisitedPoints)
		{
			if (distances [point] < minDistance)
			{
				minDistance = distances [point];
				result = point;
			}
		}

		return result;
	}



	void OnDrawGizmos()
	{

	}
}
