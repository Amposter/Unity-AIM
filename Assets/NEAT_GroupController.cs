using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class NEAT_GroupController : UnitController
{

	bool IsRunning; 
	SharpNeat.Phenomes.IBlackBox box;
	PathManager _pathManager;
	public GameObject NEAT_VehiclePrefab;
    public float groupFitness;
	private List<GameObject> NEAT_VehiclesList = new List<GameObject>();
	public int totalTriggered = 0;

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
				NEAT_Controller neatController = NEAT_Vehicle.GetComponent<NEAT_Controller> ();

				if (NEAT_Vehicle.activeInHierarchy && neatController.finishedRoute)
				{
					NEAT_Vehicle.SetActive (false);
				}

				neatController.updateSensors ();

				if (!neatController.NEATMode)
				{
					continue;
				}

				box.InputSignalArray[0] = neatController.getSensorInputs()[0];
				box.InputSignalArray[1] = neatController.getSensorInputs()[1];
				box.InputSignalArray[2] = neatController.getSensorInputs()[2];
				box.InputSignalArray[3] = neatController.getSensorInputs()[3];
				box.InputSignalArray[4] = neatController.getSensorInputs()[4];
				box.InputSignalArray[5] = neatController.getSensorInputs()[5];
				box.InputSignalArray[6] = neatController.getSensorInputs()[6];
				box.InputSignalArray[7] = neatController.getSensorInputs()[7];
				box.InputSignalArray[8] = neatController.getSensorInputs()[8];
				box.InputSignalArray[9] = neatController.getSensorInputs()[9];


				box.Activate();
                //Invert output as all inputs are inverted
                NEAT_Vehicle.GetComponent<SimpleHeuristicController>().setSpeedWeight(0.5f);//Mathf.Clamp((float)(box.OutputSignalArray[0])/SimpleHeuristicController.speed,0,1);

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
		for (int i = 0; i < 10; i++)
		{
			foreach (TrackWayPoint startPoint in _pathManager.startPoints)
			{
				GameObject NEAT_Vehicle = Instantiate (NEAT_VehiclePrefab, startPoint.transform.position, startPoint.transform.rotation) as GameObject;
				NEAT_Vehicle.transform.parent = transform;
				NEAT_Vehicle.GetComponent<NEAT_Controller> ().groupController = this;
                NEAT_Vehicle.GetComponent<NEAT_Controller>().controller = NEAT_Controller.Controller.HEURISTIC;
                NEAT_Vehicle.GetComponent<NEAT_Controller> ().startDriving (_pathManager.getCurvesFromPathNodes (_pathManager.getRandomPathNodesFromStartNode (startPoint)));
				NEAT_VehiclesList.Add (NEAT_Vehicle);
			}
			yield return new WaitForSeconds(1f+(UnityEngine.Random.Range(1, 15)/10f));
		}

		yield return null;
	}


	public override float GetFitness()
	{
		if (totalTriggered > 0)
		{
			if (groupFitness <= 0)
			{
				return 1;
			}
			return groupFitness / totalTriggered;
		}
		else
		{
			return 0.1f;
		}
	}


	public void cleanUp()
	{
		DestroyImmediate (this.gameObject);
	}

}
