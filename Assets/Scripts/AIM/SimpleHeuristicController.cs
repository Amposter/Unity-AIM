using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : VehicleController
{

	//Public variabes
	public static float speed = 5f;
	public bool paused;
    private float speedWeight;

    //Private variables
    public BezierCurve curve;
	private BezierCurve[] curves;
	private int counter;

	private int offset;
	protected Vector3 toPoint;

	protected int resolution; //Number of points to sample on each path curve

	// Use this for initialization
	protected override void Start ()
	{
		base.Start();
		paused = false;
		resolution = 25;
	}

	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
	}

	public void setCurve(BezierCurve curve)
	{
		this.curve = curve;
	}

	public void setCurves(BezierCurve[] curves)
	{
		this.curves = curves;
	}

	protected virtual IEnumerator AutoDrive()
	{
		resolution = 25;
		//  foreach (BezierCurve curve in curves)
		for (counter = 0; counter < curves.Length; ++counter)
		{
			this.curve = curves[counter];
			offset = 1;
			toPoint = curve.GetPointAt(offset / (float)resolution);
			toPoint.y = transform.position.y;
			transform.LookAt(toPoint);

			while (offset <= resolution)
			{
				while (paused) //Do nothing
				{
					yield return new WaitForFixedUpdate();
				}

				Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speedWeight * speed * Time.deltaTime);
				transform.LookAt(newPos);
				transform.position = newPos;
				if (transform.position == toPoint)
				{
					++offset;
					toPoint = curve.GetPointAt((float)offset / resolution);
					toPoint.y = transform.position.y;
					transform.LookAt(toPoint);
				}
				yield return new WaitForFixedUpdate();
			}
		}
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

