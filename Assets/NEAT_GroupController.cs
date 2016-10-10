using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NEAT_GroupController : UnitController
{

	bool IsRunning; 
	SharpNeat.Phenomes.IBlackBox box;
	PathManager _pathManager;
	public GameObject NEAT_VehiclePrefab;
	private List<GameObject> NEAT_VehiclesList = new List<GameObject>();

	// Use this for initialization
	void Start ()
	{

	}

	public void setPathManager(PathManager pathManager)
	{
		_pathManager = pathManager;
	}

	// Update is called once per frame
	void Update()
	{
		if (IsRunning)
		{
			foreach (GameObject NEAT_Vehicle in NEAT_VehiclesList)
			{
				NEAT_Controller neatController = NEAT_Vehicle.GetComponent<NEAT_Controller> ();

				if (neatController.NEATMode)
				{
					SharpNeat.Phenomes.ISignalArray inputArr = box.InputSignalArray;
					inputArr[0] = neatController.getSensorInputs()[0];
					inputArr[1] = neatController.getSensorInputs()[1];
					inputArr[2] = neatController.getSensorInputs()[2];
					inputArr[3] = neatController.getSensorInputs()[3];
					inputArr[4] = neatController.getSensorInputs()[4];
					inputArr[5] = neatController.getSensorInputs()[5];
					inputArr[6] = neatController.getSensorInputs()[6];
					inputArr[7] = neatController.getSensorInputs()[7];
					inputArr[8] = neatController.getSensorInputs()[8];
					inputArr[9] = neatController.getSensorInputs()[9];

					box.Activate();

					SharpNeat.Phenomes.ISignalArray outputArr = box.OutputSignalArray;

					NEAT_Vehicle.GetComponent<Vehicle> ().setAccelerationMagnitude ((float)outputArr[0]);
					//NEAT_Vehicle.GetComponent<Vehicle> ().setbrakingMagnitude ((float)outputArr[1]);
					NEAT_Vehicle.GetComponent<Vehicle> ().setSteeringMagnitude ((float)outputArr[1]);
				}
					
				if (NEAT_Vehicle.activeInHierarchy && neatController.finishedRoute)
				{
					NEAT_Vehicle.SetActive (false);
				}
			}

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

	protected IEnumerator spawnCars()
	{
		for(int i = 0; i < 3; i++)
		{
			foreach (TrackWayPoint startPoint in _pathManager.startPoints)
			{
				GameObject NEAT_Vehicle = Instantiate (NEAT_VehiclePrefab, startPoint.transform.position, startPoint.transform.rotation, this.transform) as GameObject;
				NEAT_Vehicle.GetComponent<NEAT_Controller> ().groupController = this;
				NEAT_Vehicle.GetComponent<NEAT_Controller> ().startDriving (_pathManager.getCurvesFromPathNodes(_pathManager.getRandomPathNodesFromStartNode(startPoint)));
				NEAT_VehiclesList.Add(NEAT_Vehicle);
			}
			yield return new WaitForSeconds(2);
		}
	}


	public override float GetFitness()
	{
		return 1;
	}


	public void cleanUp()
	{
		DestroyImmediate (this.gameObject);
	}

}
