using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class CarSpawner : MonoBehaviour
{


    //Car prefab to be used
    public GameObject car;

    //Spawn variables
    private List<TrackWayPoint> startPoints;
	private List<AIMController> AimControllers;
	public int carsPerStartPoint = 1;

	//performance measuremnets
	public float idleTime = 0;
	public float avgSpeed = 0;
	public int carsThrough = 0;
	int carsSpawned = 0;

	public float testDuration = 90;
	private float testTimer = 0;

	public float totalSpeedAccumulator = 0;
	public int totalSpeedCheckCount = 0;

	string filePath;

	// Use this for initialization
	void Start ()
    {
		AimControllers = new List<AIMController> ();
        startPoints = GameObject.Find("PathManager").GetComponent<PathManager>().startPoints;
        //SpawnOne();
        //Invoke("SpawnOne", 16.0f);
		filePath = Application.dataPath + "/results/output.csv";
        StartCoroutine("Spawn"); 
    }


	public int trial = 1;
	// Update is called once per frame
	void Update ()
	{
		testTimer += Time.deltaTime;
		if (testTimer >= testDuration)
		{
			testTimer = 0;
			Record ();
			print ("completed trial " + trial);
			Clean ();
			trial++;
			StartCoroutine("Spawn"); 
		}
	}

	void Clean()
	{
		for (int i = 0; i < AimControllers.Count; i++)
		{
			if (AimControllers [i] != null)
				GameObject.DestroyImmediate (AimControllers [i].gameObject);
		}
		AimControllers.Clear ();

		testTimer = 0;
		carsSpawned = 0;
		carsThrough = 0;
		avgSpeed = 0;
		idleTime = 0;
		totalSpeedCheckCount = 0;
		totalSpeedAccumulator = 0;

		BezierCurve[] curves = FindObjectsOfType (typeof(BezierCurve)) as BezierCurve[];
		foreach (BezierCurve curve in curves)
			curve.reset ();
	}

	void Record()
	{
		string delimiter = ",";

		float throughPut = (float)carsThrough / (float)carsSpawned;
		avgSpeed = totalSpeedAccumulator/totalSpeedCheckCount;

		string[][] output = new string[][] 
		{
			new string[]{throughPut + "", avgSpeed + "", "0", idleTime + "", carsSpawned + ""}
		};

		int length = output.GetLength(0);  
		StringBuilder sb = new StringBuilder();  
		for (int index = 0; index < length; index++)  
			sb.AppendLine(string.Join(delimiter, output[index]));  

		File.AppendAllText(filePath, sb.ToString()); 
	}

    //Spawns a car every 'spawnInterval' seconds alternating amongst the start way points.
    IEnumerator Spawn()
    {
		int[] carsSpawnedPerStartPoint = new int[startPoints.Count];

		do
		{
			for (int pointIndex = 0; pointIndex < startPoints.Count; pointIndex++)
			{
				if(carsSpawnedPerStartPoint[pointIndex] < carsPerStartPoint)
				{
					Collider2D col = Physics2D.OverlapCircle(startPoints[pointIndex].transform.position, 2, (1<<10));
			/*		bool full = false;
					foreach (Collider2D col in colliders)
					{
						if (col.gameObject.tag == "Vehicle")
							full = true;
					}*/
					if (!col)
					{
						GameObject instance = Instantiate(car);
						instance.GetComponent<AIMController>().carSpawner = this;
						AimControllers.Add(instance.GetComponent<AIMController>());
						instance.GetComponent<AIMController>().start = startPoints[pointIndex];
						carsSpawnedPerStartPoint[pointIndex]++;
						carsSpawned++;
					}
				}
			}
			yield return new WaitForSeconds(1+(Random.Range(1, 20)/(float)10));
		}
		while(carsSpawned < carsPerStartPoint*startPoints.Count);

		yield return null;
    }





    //Spawn a single car, for debugging purposes only.
    void SpawnOne()
    {
        GameObject instance = Instantiate(car);
        instance.name = "Car #" + Config.lastVIN++;
    }

}
