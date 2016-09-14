using UnityEngine;
using System.Collections.Generic;

public class IntersectionManager : MonoBehaviour {

    public int[] debugSpawnLocations = { 3,2, 2, 3}; 
    public int debugSpawnCounter = 0;
    private Dictionary<float, List<KeyValuePair<Vector3, Quaternion>>> reservations;
    public Vector3 carDimensions = new Vector3(1.0f,0.79f,2.05f);

    // Use this for initialization
    void Start ()
    {
        reservations = new Dictionary<float, List<KeyValuePair<Vector3,Quaternion>>>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public bool Reserve(KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] positions)
    {
        if (!CheckReservation(positions)) //Check for clashes first
            return false;

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

    //Check if a reservation can be made - i.e. the list of times and positions do not clash with those booked already
    bool CheckReservation(KeyValuePair<float, KeyValuePair<Vector3, Quaternion>>[] positions)
    {
        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector3 pos = positions[i].Value.Key;
            Quaternion rot = positions[i].Value.Value;

            if (!reservations.ContainsKey(time)) //Check if there are any booking for the time
                continue;

            List<KeyValuePair<Vector3,Quaternion>> reservedPositions = reservations[time];
            Bounds b = new Bounds(pos, carDimensions); //Bounding box centered on the car's position
            GameObject bounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //bounds.SetActive(false);
            bounds.transform.position = pos;
            bounds.transform.rotation = rot;
            bounds.transform.localScale = carDimensions;

            //Check the requested position against all other position that were booked already
            for (int j = 0; j < reservedPositions.Count; ++j)
            {
                Vector3 otherPos = reservedPositions[j].Key;
                Quaternion otherRot = reservedPositions[j].Value;
                GameObject otherBounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
                otherBounds.transform.position = otherPos;
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
