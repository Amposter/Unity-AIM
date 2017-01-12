using UnityEngine;
using System.Collections;
using System;


public class VehicleController : MonoBehaviour
{
    public bool showSensorsDebug = true;
	public float sensorRange = 10;
	public float proximityThreshHold = 3;
    public float[] sensorInputs = new float[10];
    bool useFrontSensorOnly = false;
	protected int finalLayerMask;
	public bool proximityWarning = false;
	bool isObstacleDetected = false;
	protected float minObstacleRange = 0;
	protected float minAngle;

    //sensors
	//RaycastHit2D sensor0; //THis is the left sensor
	RaycastHit2D sensor1;
	RaycastHit2D sensor2;
	RaycastHit2D sensor3; 
	RaycastHit2D sensor4; //This is the forward sensor
	RaycastHit2D sensor5;
	RaycastHit2D sensor6; 
	RaycastHit2D sensor7; 
	//RaycastHit2D sensor8; //this is the right sensor
	//RaycastHit2D sensor9; //This is the backwards sensor


	protected int boundaryMask = 1 << 9;
	protected int vehicleMask = 1 << 10;
	protected int pedestrianMask = 1 << 11;


	// Use this for initialization
	protected virtual void Start ()
    {
		finalLayerMask = vehicleMask | pedestrianMask; //build a layermask that only checks the vehicle and pedestrian physics layers
	}
		

	// Update is called once per frame
	protected virtual void Update ()
    {
		
	}

	public virtual void updateSensors()
	{
		minObstacleRange = 0;
		isObstacleDetected = false;
		//perform a raycast for each sensor
		sensor4 = Physics2D.Raycast (transform.position, transform.up, sensorRange, finalLayerMask);
		if (sensor4)
		{
			sensorInputs [4] = (float)Math.Round(1f-((Vector2)transform.position - sensor4.point).magnitude/sensorRange,4);

			if (sensor4.collider.gameObject.layer == 10 || sensor4.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [4] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [4];
				minAngle = 0f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor4.point, Color.red);
			}
		}
		else
		{
			sensorInputs[4] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + transform.up * sensorRange, Color.green);
			}
		}
			
		/*sensor0 = Physics2D.Raycast (transform.position, -transform.right, sensorRange, finalLayerMask);
		if (sensor0)
		{
			sensorInputs [0] = (float)Math.Round(1f-((Vector2)transform.position - sensor0.point).magnitude/sensorRange,4);

			if (sensor0.collider.gameObject.layer == 10 || sensor0.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [0] > minObstacleRange)
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
			sensorInputs[0] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + -transform.right * sensorRange, Color.green);
			}
		}*/
		sensor1 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, -22.5f) * -transform.right, sensorRange, finalLayerMask);
		if (sensor1)
		{
			sensorInputs [1] = (float)Math.Round(1f-((Vector2)transform.position - sensor1.point).magnitude/sensorRange,4);

			if (sensor1.collider.gameObject.layer == 10 || sensor1.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [1] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [1];
				minAngle = -1f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor1.point, Color.red);
			}
		}
		else
		{
			sensorInputs[1] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, -22.5f) * -transform.right * sensorRange, Color.green);
			}
		}

		sensor2 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, -45f) * -transform.right, sensorRange, finalLayerMask);
		if (sensor2)
		{
			sensorInputs [2] = (float)Math.Round(1f-((Vector2)transform.position - sensor2.point).magnitude/sensorRange,4);

			if (sensor2.collider.gameObject.layer == 10 || sensor2.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [2] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [2];
				minAngle = -0.67f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor2.point, Color.red);
			}
		}
		else
		{
			sensorInputs[2] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, -45f) * -transform.right * sensorRange, Color.green);
			}
		}

		sensor3 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, -67.5f) * -transform.right, sensorRange, finalLayerMask);
		if (sensor3)
		{
			sensorInputs [3] = (float)Math.Round(1f-((Vector2)transform.position - sensor3.point).magnitude/sensorRange,4);

			if (sensor3.collider.gameObject.layer == 10 || sensor3.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [3] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [3];
				minAngle = -0.33f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor3.point, Color.red);
			}
		}
		else
		{
			sensorInputs[3] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, -67.5f) * -transform.right * sensorRange, Color.green);
			}
		}

		sensor5 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, 67.5f) * transform.right, sensorRange, finalLayerMask);
		if (sensor5)
		{
			sensorInputs [5] = (float)Math.Round(1f-((Vector2)transform.position - sensor5.point).magnitude/sensorRange,4);

			if (sensor5.collider.gameObject.layer == 10 || sensor5.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [5] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [5];
				minAngle = 0.33f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor5.point, Color.red);
			}
		}
		else
		{
			sensorInputs[5] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, 67.5f) * transform.right * sensorRange, Color.green);
			}
		}
		sensor6 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, 45f) * transform.right, sensorRange, finalLayerMask);
		if (sensor6)
		{
			sensorInputs [6] = (float)Math.Round(1f-((Vector2)transform.position - sensor6.point).magnitude/sensorRange,4);

			if (sensor6.collider.gameObject.layer == 10 || sensor6.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [6] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [6];
				minAngle = 0.67f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor6.point, Color.red);
			}
		}
		else
		{
			sensorInputs[6] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, 45f) * transform.right * sensorRange, Color.green);
			}
		}

		sensor7 = Physics2D.Raycast (transform.position, Quaternion.Euler (0, 0, 22.5f) * transform.right, sensorRange, finalLayerMask);
		if (sensor7)
		{
			sensorInputs [7] = (float)Math.Round(1f-((Vector2)transform.position - sensor7.point).magnitude/sensorRange,4);

			if (sensor7.collider.gameObject.layer == 10 || sensor7.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [7] > minObstacleRange)
			{
				minObstacleRange = sensorInputs [7];
				minAngle = 1f;
			}
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, sensor7.point, Color.red);
			}
		}
		else
		{
			sensorInputs[7] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + Quaternion.Euler (0, 0, 22.5f) * transform.right * sensorRange, Color.green);
			}
		}

	/*	sensor8 = Physics2D.Raycast (transform.position, transform.right, sensorRange, finalLayerMask);
		if (sensor8)
		{
			sensorInputs [8] = (float)Math.Round(1f-((Vector2)transform.position - sensor8.point).magnitude/sensorRange,4);

			if (sensor8.collider.gameObject.layer == 10 || sensor8.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [8] > minObstacleRange)
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
			sensorInputs[8] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + transform.right * sensorRange, Color.green);
			}
		}

		sensor9 = Physics2D.Raycast (transform.position, -transform.up, sensorRange, finalLayerMask);
		if (sensor9)
		{
			sensorInputs [9] = (float)Math.Round(1f-((Vector2)transform.position - sensor9.point).magnitude/sensorRange,4);

			if (sensor9.collider.gameObject.layer == 10 || sensor9.collider.gameObject.layer == 11)
			{
				isObstacleDetected = true;
			}

			if (sensorInputs [9] > minObstacleRange)
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
			sensorInputs[9] = 0;
			if (showSensorsDebug)
			{
				Debug.DrawLine (transform.position, transform.position + -transform.up * sensorRange, Color.green);
			}
		}*/



		if ((1f-minObstacleRange) * sensorRange < proximityThreshHold && isObstacleDetected)
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
