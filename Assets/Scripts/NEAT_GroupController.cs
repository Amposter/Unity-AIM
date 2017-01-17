using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class NEAT_GroupController : UnitController
{

	bool IsRunning; 
	SharpNeat.Phenomes.IBlackBox box;
	PathManager _pathManager;
	public GameObject NEAT_VehiclePrefab;
    public float groupFitness;
	private List<GameObject> NEAT_VehiclesList = new List<GameObject>();
	private PedestrianSpawner2D[] pedestrianSpawners;
	public int totalTriggered = 0;
	public int speedCheckCount = 0;
	public float totalSpeedAccumulator = 0;
	public int minDistanceCheckCount = 0;
	public float totalDistanceAccumulator = 0;
	public int collisionCount = 0;
	private int spawnBlockedCount = 0;
	public Optimizer optimizer;
	private int carsSpawned = 0;
	public int carsThrough = 0;
	public float idleTime = 0;

	// Use this for initialization
	void Start ()
	{
        groupFitness = 0;

	}

	public void setPathManager(PathManager pathManager)
	{
		_pathManager = pathManager;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (IsRunning)
		{
            foreach (GameObject NEAT_Vehicle in NEAT_VehiclesList)
			{
				if (NEAT_Vehicle == null)
					continue;
				
				NEAT_Controller neatController = NEAT_Vehicle.GetComponent<NEAT_Controller>();

				if (NEAT_Vehicle.activeInHierarchy && neatController.finishedRoute) 
				{
					Destroy(NEAT_Vehicle);
				}
				else if (NEAT_Vehicle.activeInHierarchy) 
				{

					neatController.updateSensors ();

					box.InputSignalArray [0] = neatController.getAngleToWayPoint();
					box.InputSignalArray [1] = neatController.getMinObstacleAngle();
					box.InputSignalArray [2] = neatController.getMinObstacleRange();
					box.InputSignalArray [3] = neatController.getDistToWayPoint();


					box.Activate ();
 
					neatController.setSpeedWeight (Mathf.Clamp ((float)(box.OutputSignalArray [0]), 0, 1));
					neatController.updatePosition ();
				}
			}

			foreach (PedestrianSpawner2D p in pedestrianSpawners)
				p.UpdatePedestrians ();

		}
	}

	public override void Stop()
	{
		this.IsRunning = false;

	}

	public override void Activate(SharpNeat.Phenomes.IBlackBox box)
	{
		this.box = box;
		this.IsRunning = true;
		StartCoroutine ("spawnCars");
	}

	protected IEnumerator spawnCars ()
	{

		optimizer = GameObject.Find ("Optimizer").GetComponent<Optimizer> ();
		List<TrackWayPoint> startPoints = GameObject.Find ("PathManager").GetComponent<PathManager> ().startPoints;
	
		int[] carsSpawnedPerStartPoint = new int[startPoints.Count];

		pedestrianSpawners = FindObjectsOfType (typeof(PedestrianSpawner2D)) as PedestrianSpawner2D[];
		foreach (PedestrianSpawner2D p in pedestrianSpawners)
		{
			p.SetUp ();
			StartCoroutine(p.SpawnPedestrians());
		}
		do {
			for (int pointIndex = 0; pointIndex < startPoints.Count; pointIndex++)
			{
				if (carsSpawnedPerStartPoint [pointIndex] < optimizer.Test_carsPerStartPoint)
				{
					Collider[] colliders = Physics.OverlapSphere (startPoints [pointIndex].transform.position, 2);
					bool full = false;
					foreach (Collider col in colliders)
					{
						if (col.gameObject.tag == "Vehicle")
							full = true;
					}
					if (!full)
					{
						GameObject NEAT_Vehicle = Instantiate (NEAT_VehiclePrefab) as GameObject;
						//NEAT_Vehicle.transform.position = (Vector2)startPoints[pointIndex].transform.position;
						NEAT_Vehicle.transform.parent = transform;
						NEAT_Vehicle.GetComponent<NEAT_Controller> ().groupController = this;
						NEAT_Vehicle.GetComponent<NEAT_Controller> ().startDriving (_pathManager.getCurvesFromPathNodes (_pathManager.getRandomPathNodesFromStartNode (startPoints[pointIndex])));
						NEAT_VehiclesList.Add (NEAT_Vehicle);
						carsSpawned++;
					}
					else
					{
						spawnBlockedCount++;
					}
				}
			}
			yield return new WaitForSeconds (1 + (UnityEngine.Random.Range (1, 20) / (float)10));
		} while(carsSpawned < optimizer.Test_carsPerStartPoint * startPoints.Count);

		yield return null;
	}



	public override float GetFitness()
	{
		return Mathf.Clamp(
							((totalSpeedAccumulator / speedCheckCount) * 2500)
							//+((totalDistanceAccumulator / minDistanceCheckCount) * 1000)
							-(spawnBlockedCount*50)
							-(collisionCount*125)
							,1, float.MaxValue);
	}

	public void record() 
	{ 
		string delimiter = ","; 

		float throughPut = (float)carsThrough / (float)carsSpawned; 
		float avgSpeed = (totalSpeedAccumulator / speedCheckCount)*8; 
	

		string[][] output = new string[][]  
		{ 
			new string[]{throughPut + "", avgSpeed + "", collisionCount/2 + "", idleTime + "", carsSpawned + ""} 
		}; 


		int length = output.GetLength(0);   
		StringBuilder sb = new StringBuilder();   
		for (int index = 0; index < length; index++)   
		sb.AppendLine(string.Join(delimiter, output[index]));   

		File.AppendAllText(Application.dataPath + "/results/results_run"+optimizer.test_currentRun+"_neuro.csv", sb.ToString());  
	} 
	

	public void cleanUp()
	{
		foreach (PedestrianSpawner2D p in pedestrianSpawners) 
		{
			StopCoroutine (p.SpawnPedestrians());
			p.Clean ();
		}
		DestroyImmediate (this.gameObject);
	}

}
