using UnityEngine;
using System.Collections.Generic;

public class IntersectionManager : MonoBehaviour {

    Dictionary<float, List<Vector3>> reservations;
    public Vector3 carDimensions = new Vector3(1.0f,0.79f,2.05f);

    // Use this for initialization
    void Start ()
    {
        reservations = new Dictionary<float, List<Vector3>>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    bool Reserve(KeyValuePair<float, Vector3>[] positions)
    {
        return true;
    }

    //Check if a reservation can be made - i.e. the list of times and positions do not clash with those booked already
    bool CheckReservation(KeyValuePair<float, Vector3>[] positions)
    {
        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector3 pos = positions[i].Value;

            if (!reservations.ContainsKey(time)) //Check if there are any booking for the time
                continue;
            List<Vector3> reservedPositions = reservations[time];
            Bounds b = new Bounds(pos, carDimensions); //Bounding box centered on the car's position
             
            //Check the requested position against all other position that were booked already
            for (int j = 0; j < reservedPositions.Count; ++j)
            {
                Bounds other = new Bounds(reservedPositions[j], carDimensions);
                if (b.Intersects(other))
                    return false;
            }
        }
        return true;
    }
}
