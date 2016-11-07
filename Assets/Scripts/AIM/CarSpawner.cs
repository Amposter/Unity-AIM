using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour
{


    //Car prefab to be used
    public GameObject car;

    //Spawn variables
    private List<TrackWayPoint> startPoints;
    public float spawnInterval = 1.0f;
	public int carsPerStartPoint = 1;

	//performance measuremnets
	public float avgIdleTime;
	public float avgSpeed;
	public int carsThrough;
	public int collisions;

	public float testDuration = 0;


	// Use this for initialization
	void Start ()
    {
        startPoints = GameObject.Find("PathManager").GetComponent<PathManager>().startPoints;
        //SpawnOne();
        //Invoke("SpawnOne", 16.0f);
        StartCoroutine("Spawn"); 
    }


    //Spawns a car every 'spawnInterval' seconds alternating amongst the start way points.
    IEnumerator Spawn()
    {
		int[] carsSpawnedPerStartPoint = new int[startPoints.Count];
		int carsSpawned = 0;
		do
		{
			for (int pointIndex = 0; pointIndex < startPoints.Count; pointIndex++)
			{
				if(carsSpawnedPerStartPoint[pointIndex] < carsPerStartPoint)
				{
					Collider[] colliders = Physics.OverlapSphere(startPoints[pointIndex].transform.position, 2);
					bool full = false;
					foreach (Collider col in colliders)
					{
						if (col.gameObject.tag == "Vehicle")
							full = true;
					}
					if (!full)
					{
						GameObject instance = Instantiate(car);
						instance.GetComponent<AIMController>().start = startPoints[pointIndex];
						carsSpawnedPerStartPoint[pointIndex]++;
					}
				}
			}
			yield return new WaitForSeconds(spawnInterval);
		}
		while(carsSpawned < carsPerStartPoint*startPoints.Count);

		yield return null;
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
                instance.GetComponent<AIMController>().start = startPoints[random];
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Update is called once per frame
    void Update ()
    {
    }

    //Spawn a single car, for debugging purposes only.
    void SpawnOne()
    {
        GameObject instance = Instantiate(car);
        instance.name = "Car #" + Config.lastVIN++;
    }

}
