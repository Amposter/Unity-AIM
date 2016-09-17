﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour {


    //Car prefab to be used
    public GameObject car;

    //Spawn variables
    private List<TrackWayPoint> startPoints;
    public float spawnInterval = 1.0f;
    private int counter;

    public float timeScale = 1;
	// Use this for initialization
	void Start ()
    {

        GameObject[] wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
        startPoints = new List<TrackWayPoint>();
        foreach (GameObject obj in wayPoints)
        {
            if (obj.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.START)
                startPoints.Add(obj.GetComponent<TrackWayPoint>());
        }

        StartCoroutine("Spawn"); 
    }

    //Spawns a car every 'spawnInterval' seconds alternating amongst the start way points.
    IEnumerator Spawn()
    {
        counter = 0;
        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(startPoints[counter%startPoints.Count].transform.position, 2);
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = (startPoints[counter%startPoints.Count].transform.position);
            //sphere.transform.localScale = new Vector3(2.0f,2.0f,2.0f);
            bool full = false;
            foreach (Collider col in colliders)
            {
                if (col.gameObject.tag == "Vehicle")
                    full = true;
            }
            if (!full)
            {
                GameObject instance = Instantiate(car);
                car.GetComponent<AIMController>().start = startPoints[counter%startPoints.Count];
            }
            ++counter;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    //Spawns a car ever 'spawnInterval' seconds randomly choosing a start way point.
    IEnumerator SpawnRandom()
    {
        while (true)
        {
            int random = Random.Range(0, startPoints.Count);
            Collider[] colliders = Physics.OverlapSphere(startPoints[random].transform.position, 2);
            bool full = false;
            foreach (Collider col in colliders)
            {
                if (col.gameObject.tag == "Vehicle")
                    full = true;
            }
            if (!full)
            {
                GameObject instance = Instantiate(car);
                car.GetComponent<AIMController>().start = startPoints[random];
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Update is called once per frame
    void Update () {
        Time.timeScale = timeScale;
	}
}
