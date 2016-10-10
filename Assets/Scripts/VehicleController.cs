﻿using UnityEngine;
using System.Collections;


public class VehicleController : MonoBehaviour
{
    public bool showSensorsDebug = true;
	public float sensorRange = 10;
	public float proximityThreshHold = 3;
    float[] sensorInputs = new float[32];
    bool useFrontSensorOnly = false;
	protected int finalLayerMask;
	public bool proximityWarning = false;

    //sensors
	RaycastHit sensor0; //THis is the left sensor
	RaycastHit sensor1;
	RaycastHit sensor2;
	RaycastHit sensor3; 
	RaycastHit sensor4; //This is the forward sensor
	RaycastHit sensor5;
	RaycastHit sensor6; 
	RaycastHit sensor7; 
	RaycastHit sensor8; //this is the right sensor
	RaycastHit sensor9; //This is the backwards sensor


	protected int boundaryMask = 1 << 9;
	protected int vehicleMask = 1 << 10;
	protected int pedestrianMask = 1 << 11;

	// Use this for initialization
	protected virtual void Start ()
    {
		finalLayerMask = vehicleMask | pedestrianMask; //build a layermask that only checks the boundary, vehicle and pedestrian physics layers
	}
	
	// Update is called once per frame
	protected virtual void Update ()
    {
		float minObstacleRange = 1;

		//perform a raycast for each sensor
		if (Physics.Raycast (transform.position, transform.forward, out sensor4, sensorRange, finalLayerMask))
		{
			sensorInputs [4] = (transform.position - sensor4.point).magnitude/sensorRange;
			if (sensorInputs [4] < minObstacleRange)
			{
				minObstacleRange = sensorInputs [4];
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor4.point, Color.red);
			}
		}
		else
		{
			sensorInputs[4] = 1;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + transform.forward * sensorRange, Color.green);
			}
		}

		if (!useFrontSensorOnly)
		{
			if (Physics.Raycast (transform.position, -transform.right, out sensor0, sensorRange, finalLayerMask))
			{
				sensorInputs [0] = (transform.position - sensor0.point).magnitude/sensorRange;
				if (sensorInputs [0] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [0];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor0.point, Color.red);
				}
			}
			else
			{
				sensorInputs[0] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + -transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, 22.5f, 0) * -transform.right, out sensor1, sensorRange, finalLayerMask))
			{
				sensorInputs [1] = (transform.position - sensor1.point).magnitude/sensorRange;
				if (sensorInputs [1] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [1];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor1.point, Color.red);
				}
			}
			else
			{
				sensorInputs[1] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 22.5f, 0) * -transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, 45f, 0) * -transform.right, out sensor2, sensorRange, finalLayerMask))
			{
				sensorInputs [2] = (transform.position - sensor2.point).magnitude/sensorRange;
				if (sensorInputs [2] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [2];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor2.point, Color.red);
				}
			}
			else
			{
				sensorInputs[2] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 45f, 0) * -transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, 67.5f, 0) * -transform.right, out sensor3, sensorRange, finalLayerMask))
			{
				sensorInputs [3] = (transform.position - sensor3.point).magnitude/sensorRange;
				if (sensorInputs [3] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [3];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor3.point, Color.red);
				}
			}
			else
			{
				sensorInputs[3] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 67.5f, 0) * -transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, -22.5f, 0) * transform.right, out sensor5, sensorRange, finalLayerMask))
			{
				sensorInputs [5] = (transform.position - sensor5.point).magnitude/sensorRange;
				if (sensorInputs [5] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [5];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor5.point, Color.red);
				}
			}
			else
			{
				sensorInputs[5] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, -22.5f, 0) * transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, -45f, 0) * transform.right, out sensor6, sensorRange, finalLayerMask))
			{
				sensorInputs [6] = (transform.position - sensor6.point).magnitude/sensorRange;
				if (sensorInputs [6] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [6];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor6.point, Color.red);
				}
			}
			else
			{
				sensorInputs[6] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, -45f, 0) * transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, Quaternion.Euler (0, -67.5f, 0) * transform.right, out sensor7, sensorRange, finalLayerMask))
			{
				sensorInputs [7] = (transform.position - sensor7.point).magnitude/sensorRange;
				if (sensorInputs [7] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [7];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor7.point, Color.red);
				}
			}
			else
			{
				sensorInputs[7] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, -67.5f, 0) * transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, transform.right, out sensor8, sensorRange, finalLayerMask))
			{
				sensorInputs [8] = (transform.position - sensor8.point).magnitude/sensorRange;
				if (sensorInputs [8] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [8];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor8.point, Color.red);
				}
			}
			else
			{
				sensorInputs[8] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + transform.right * sensorRange, Color.green);
				}
			}

			if (Physics.Raycast (transform.position, -transform.forward, out sensor9, sensorRange, finalLayerMask))
			{
				sensorInputs [9] = (transform.position - sensor9.point).magnitude/sensorRange;
				if (sensorInputs [9] < minObstacleRange)
				{
					minObstacleRange = sensorInputs [9];
				}
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, sensor9.point, Color.red);
				}
			}
			else
			{
				sensorInputs[9] = 1;
				if (showSensorsDebug)
				{
					Debug.DrawLine (transform.position, transform.position + -transform.forward * sensorRange, Color.green);
				}
			}
		}

		if (minObstacleRange * sensorRange < proximityThreshHold)
		{
			proximityWarning = true;
		}
		else
		{
			proximityWarning = false;
		}

	}

	public void setFrontSensorOnly(bool value)
	{
		useFrontSensorOnly = value;
	}

	public float getFrontSensorInput()
	{
		return sensorInputs[4];
	}

	public float[] getSensorInputs()
	{
		return sensorInputs;
	}
}
