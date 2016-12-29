using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class IntersectionManager : MonoBehaviour {

    public GameObject carPlaceholder;
    public float cleanInterval = 20.0f;
    public float cleanOffset = 5.0f;
    public int debugSpawnCounter = 0;
    private Dictionary<float, List<KeyValuePair<Vector2, Quaternion>>> reservations;
    private Vector2 carDimensions = new Vector2(2.39f,3.51f); //with padding

    // Use this for initialization
    void Start ()
    {
        reservations = new Dictionary<float, List<KeyValuePair<Vector2,Quaternion>>>();
        StartCoroutine("Clean");
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
		
    //Cleans all entries that are at least 'cleanOffset' seconds old
    IEnumerator Clean()
    {
        List<float> staleBookings = new List<float>(); ;
        while (true)
        {
            yield return new WaitForSeconds(cleanInterval);
            float timeStamp = (float)Math.Round(Time.time,1);
            float timeCheck = timeStamp - cleanOffset;
            foreach (float key in reservations.Keys)
            {
                if (key < timeCheck)
                    staleBookings.Add(key);
            }
            foreach (float time in staleBookings)
                reservations.Remove(time);

            staleBookings.Clear();
        }
    }

    public bool Reserve(KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[] positions)
    {
        if (!CheckReservation(positions)) //Check for clashes first
            return false;


        //No clashes, add positions
        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector2 pos = positions[i].Value.Key;
            Quaternion rot = positions[i].Value.Value;

            if (!reservations.ContainsKey(time)) //If there are no bookings for the time, create a new list 
                reservations[time] = new List<KeyValuePair<Vector2, Quaternion>>();

            reservations[time].Add(new KeyValuePair<Vector2, Quaternion>(pos, rot));
            }
        return true;

    }

  
    //Check if a reservation can be made - i.e. the list of times and positions do not clash with those booked already
    bool CheckReservation(KeyValuePair<float, KeyValuePair<Vector2, Quaternion>>[] positions)
    {

        for (int i = 0; i < positions.Length; ++i)
        {
            float time = positions[i].Key;
            Vector2 pos = positions[i].Value.Key;
            Quaternion rot = positions[i].Value.Value;

            if (!reservations.ContainsKey(time)) //Check if there are any bookings for the time
                continue;

			List<KeyValuePair<Vector2,Quaternion>> reservedPositions = reservations[time];
			GameObject bounds = GameObject.Instantiate(carPlaceholder);
            bounds.transform.position = pos;
            bounds.transform.rotation = rot;
            bounds.name = "bounds #" + i;

            //Check the requested position against all other position that were booked already
            for (int j = 0; j < reservedPositions.Count; ++j)
            {
                Vector2 otherPos = reservedPositions[j].Key;
                Quaternion otherRot = reservedPositions[j].Value;
				GameObject otherBounds = GameObject.Instantiate(carPlaceholder);
				otherBounds.transform.position = otherPos;
				otherBounds.transform.rotation = otherRot;
				otherBounds.name = "bounds #" + i;
				UnityEditor.EditorApplication.isPaused = true;
				if (bounds.GetComponent<Bounds>().collided())
                {
                    Destroy(otherBounds);
                    Destroy(bounds);
                    return false;
                }
				Destroy(otherBounds);
            }
			Destroy(bounds);      
        }
        return true;
    }
}
