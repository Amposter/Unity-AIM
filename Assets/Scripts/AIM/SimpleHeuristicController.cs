using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : VehicleController
{

	//Public variabes
	public static float speed = 8f;
	public bool paused;
    public float speedWeight;

    //Private variables
    public BezierCurve curve;
	private BezierCurve[] curves;
	protected int counter;

	protected int offset;
	protected Vector2 toPoint;

	protected int resolution; //Number of points to sample on each path curve

	// Use this for initialization
	protected override void Start ()
	{
		base.Start();
		paused = false;
	}

	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
	}
		
	public void updateResolution(float length)
	{
		int val = 0;
		if (length < 5)
			val = 3;
		else if (length < 10)
			val = 6;
		else if (length < 26)
			val = 8;
		else
			val = 15;
		resolution = val;
	}
	public void updatePosition()
	{		

		if ((counter > curves.Length - 1) || paused)
			return;

		if (offset > resolution) 
		{
			++counter;
			offset = 1;
			if (counter > curves.Length - 1)
				return;
			this.curve = curves [counter];
			updateResolution (this.curve.length);
			toPoint = curve.GetPointAt (offset / (float)resolution);
			transform.rotation *= Quaternion.FromToRotation(transform.up, (toPoint - (Vector2)transform.position).normalized);
		}

		//TODO: Fix multiple over steps. For now, use small enough amount of sampling steps
		Vector2 travelVector = (toPoint - (Vector2)transform.position);
		Vector2 dir = travelVector.normalized;
		Vector2 newPos = (Vector2)transform.position + speed * dir * Time.fixedDeltaTime;
		float distanceToTravel = travelVector.magnitude;
		if (distanceToTravel <= speed * Time.fixedDeltaTime) //Newpos oversteps the next waypoint
		{ 
			++offset;
			toPoint = curve.GetPointAt((float)offset/resolution);
			transform.rotation *= Quaternion.FromToRotation(transform.up, dir);
		} 
		transform.position = newPos;  

	}

	public void setCurve(BezierCurve curve)
	{
		this.curve = curve;
	}

	public void setCurves(BezierCurve[] curves)
	{
		this.curves = curves;

	}



	protected void AutoDriveFrame()
	{

	}

	//Update the offset along the BezierCurve to be the point closest to object's current position
	protected void UpdateNextPoint()
	{
		int temp = offset;
		BezierCurve curve;
		Vector3 point = this.curve.GetPointAt(temp / (float)resolution);
		point.y = transform.position.y;
		float minDist = (point - transform.position).magnitude;
		float dist;

		for (int i = counter; i < curves.Length; ++i)
		{
			curve = curves[i];

			while (++temp <= resolution)
			{
				point = curve.GetPointAt(temp / (float)resolution);
				point.y = transform.position.y;
				dist = (point - transform.position).magnitude;
				if (dist > minDist)
				{
					this.offset = temp-1;
					this.curve = curve;
					this.counter = i;
					this.toPoint = curve.GetPointAt((float)offset / resolution);
					this.toPoint.y = transform.position.y;
					return;
				}
				else
					minDist = dist;
			}

			temp = 0;   
		}
		//Debug.Log(offset + " new " + (temp - 1));
		//  if (temp < resolution) //Move to the next point if you can
		//     offset = temp;
		//  else

	}

	protected virtual IEnumerator Drive ()
	{
		offset = 1;
		Vector3 toPoint = curve.GetPointAt(offset /(float)resolution);
		toPoint.y = transform.position.y;
		transform.LookAt(toPoint);

		while (offset <= resolution)
		{
			while (paused) //Do nothing
			{
				yield return null;
			}
            Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.deltaTime);
			transform.LookAt(newPos);
			transform.position = newPos; 
			if (transform.position == toPoint)
			{         
				++offset;
				toPoint = curve.GetPointAt((float)offset/resolution);
				toPoint.y = transform.position.y;
				transform.LookAt(toPoint);
			}
			yield return null;
		}
	}

	//Methods for pausing
	public void Pause()
	{
		paused = true;
	}

	public void Unpause()
	{
		paused = false;
	}

	public bool Paused()
	{
		return paused;
	}

	//Helper methods
	public Vector3 getToPoint()
	{
		return toPoint;
	}

    public void setSpeedWeight(float weight)
    {
        speedWeight = weight;
    }

    public float getSpeedWeight()
    {
        return speedWeight;
    }
}

