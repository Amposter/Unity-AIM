using UnityEngine;
using System.Collections.Generic;

public class IntersectionManager : MonoBehaviour {

    public GameObject carPlaceholder;
    public int[] debugSpawnLocations = { 3,2, 2, 3}; 
    public int debugSpawnCounter = 0;
    private Dictionary<float, List<KeyValuePair<Vector3, Quaternion>>> reservations;
    public Vector3 carDimensions = new Vector3(1.0f,0.79f,2.05f);
    public float padding = 0.15f;

    // Use this for initialization
    void Start ()
    {
        reservations = new Dictionary<float, List<KeyValuePair<Vector3,Quaternion>>>();
        carDimensions += carDimensions * padding;
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public bool Reserve(KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] positions, BezierCurve currLane, BezierCurve nextLane, string name)
    {
        if (!CheckReservation(positions, currLane, nextLane, name)) //Check for clashes first
            return false;

        Vector3 lastPos = nextLane.GetPointAt(0) + (nextLane.GetPointAt(0.02f) - nextLane.GetPointAt(0)).normalized * (carDimensions.z * 2f); //positions[positions.Length - 1].Value.Key;
        lastPos.y = 0.5f;
        Vector3 placeholderPos = nextLane.GetPointAt(0);
        placeholderPos.y = 0.5f;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.rotation = positions[positions.Length - 1].Value.Value;
        cube.transform.localScale = carDimensions;
        cube.transform.position = lastPos;
       // Destroy(cube);

        //Check the path and the last position
        Collider[] colliders = Physics.OverlapBox(lastPos, carDimensions * 0.55f, positions[positions.Length - 1].Value.Value);//Physics.OverlapSphere(positions[positions.Length-1].Value.Key, 2);
        foreach (Collider col in colliders)
        {
            //Debug.Log(col.gameObject.name);
            if ((col.gameObject.tag == "Vehicle" && col.gameObject.GetComponent<AIMController>().Paused()) || (col.gameObject.name == "BackSensor"))
            {
                GameObject placeholder = Instantiate(carPlaceholder);
                placeholder.transform.position = placeholderPos;
                placeholder.transform.rotation = positions[positions.Length - 1].Value.Value;
                placeholder.GetComponent<Placeholder>().id = name;
                placeholder.name = "Placeholder #" + name;
                Debug.Log("Placeholder placed");
            }
        }

        //No clashes, add positions
        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector3 pos = positions[i].Value.Key;
            Quaternion rot = positions[i].Value.Value;

            if (!reservations.ContainsKey(time)) //If there are no bookings for the time, create a new list 
                reservations[time] = new List<KeyValuePair<Vector3, Quaternion>>();

            reservations[time].Add(new KeyValuePair<Vector3, Quaternion>(pos, rot));
            }
        return true;

    }

    //Checks if a car is along a path by samplaing 'resolution' points
    bool CheckPath(BezierCurve path, BezierCurve nextPath, int resolution, string name)
    {
        for (int i = 1; i < resolution+1; ++i)
        {
            Vector3 point = path.GetPointAt((float)i/resolution);
            point.y = 0.5f;
            Collider[] colliders = Physics.OverlapBox(point, new Vector3(0.5f,0.5f,0.5f));//Physics.OverlapSphere(positions[positions.Length-1].Value.Key, 2);
            foreach (Collider col in colliders)
            {
                if (name.Equals(col.gameObject.name))
                    continue;
                if (col.gameObject.tag == "Vehicle" && col.gameObject.GetComponent<AIMController>().ComparePath(nextPath))
                    return false;
            }

        }
        return true;
    }
    //Check if a reservation can be made - i.e. the list of times and positions do not clash with those booked already
    bool CheckReservation(KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] positions, BezierCurve currLane, BezierCurve nextLane, string name)
    {
        //Check if lane is full first.
        Vector3 lastPos = positions[positions.Length - 1].Value.Key; //positions[positions.Length - 1].Value.Key;
       /* GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = lastPos;
        cube.transform.rotation = positions[positions.Length - 1].Value.Value;
        cube.transform.localScale = carDimensions;
        Destroy(cube)*/

        //Check the path and the last position
        Collider[] colliders = Physics.OverlapBox(lastPos,carDimensions*0.55f, positions[positions.Length - 1].Value.Value);//Physics.OverlapSphere(positions[positions.Length-1].Value.Key, 2);
        foreach (Collider col in colliders)
        {
            if (col.gameObject.tag == "Vehicle" && col.gameObject.GetComponent<AIMController>().Paused()) //Lane is full
                return false;
        }
        
        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector3 pos = positions[i].Value.Key;
            Quaternion rot = positions[i].Value.Value;

            if (!reservations.ContainsKey(time)) //Check if there are any bookings for the time
                continue;

            List<KeyValuePair<Vector3,Quaternion>> reservedPositions = reservations[time];
            Bounds b = new Bounds(pos, carDimensions); //Bounding box centered on the car's position
            GameObject bounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //bounds.SetActive(false);
            bounds.transform.position = pos;
            bounds.transform.rotation = rot;
            bounds.transform.localScale = carDimensions;
            bounds.name = "bounds #" + i;

            //Check the requested position against all other position that were booked already
            for (int j = 0; j < reservedPositions.Count; ++j)
            {
                Vector3 otherPos = reservedPositions[j].Key;
                Quaternion otherRot = reservedPositions[j].Value;
                GameObject otherBounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
                otherBounds.transform.position = otherPos;
                otherBounds.name = "otherBounds #" + j;
                otherBounds.transform.rotation = otherRot;
                otherBounds.transform.localScale = carDimensions;

                if (bounds.GetComponent<Collider>().bounds.Intersects(otherBounds.GetComponent<Collider>().bounds))
                {
                    Destroy(otherBounds);
                    Destroy(bounds);
                    return false;
                }
                Destroy(otherBounds);
                Destroy(bounds);
            }
            
        }
        return true;
    }
}
