using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawnear : MonoBehaviour {

    private List<TrackWayPoint> startPoints;

    //Car prefab to be used
    public GameObject car;

    //Spawn variables
    public float spawnInterval = 1.0f;
    private int counter;

	// Use this for initialization
	void Start ()
    {

        counter = 0;
        GameObject[] wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
        startPoints = new List<TrackWayPoint>();
        foreach (GameObject obj in wayPoints)
        {
            if (obj.GetComponent<TrackWayPoint>().type == TrackWayPoint.Type.START)
                startPoints.Add(obj.GetComponent<TrackWayPoint>());
        }

        StartCoroutine("Spawn"); 
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            GameObject instance = Instantiate(car);
            Debug.Log(counter);
            car.GetComponent<AIMController>().start = startPoints[counter++ % startPoints.Count];
            yield return new WaitForSeconds(spawnInterval);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
