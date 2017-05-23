using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : VehicleController
{

    //Public variabes
    public float speed = 8f;

    //Private variables
    public bool paused;
    private BezierCurve curve;
    private BezierCurve[] curves;


    protected int resolution; //Number of points to sample on each path curve

	// Use this for initialization
	protected override void Start ()
	{
		base.Start();
        paused = false;
        resolution = 20;
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
        foreach (BezierCurve curve in curves)
        {
            int offset = 1;
            Vector3 toPoint = curve.GetPointAt(offset / (float)resolution);
            toPoint.y = transform.position.y;
            transform.LookAt(toPoint);

            while (offset <= resolution)
            {
                while (paused) //Do nothing
                {
					yield return new WaitForFixedUpdate();
                }
				Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.fixedDeltaTime);
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
    protected virtual IEnumerator Drive ()
    {
        int offset = 1;
        Vector3 toPoint = curve.GetPointAt(offset /(float)resolution);
        toPoint.y = transform.position.y;
        transform.LookAt(toPoint);

        while (offset <= resolution)
        {
            while (paused) //Do nothing
            {
				yield return new WaitForFixedUpdate();
            }
			Vector3 newPos = Vector3.MoveTowards(transform.position, toPoint, speed * Time.fixedDeltaTime);
            transform.LookAt(newPos);
            transform.position = newPos; 
            if (transform.position == toPoint)
            {         
                ++offset;
                toPoint = curve.GetPointAt((float)offset/resolution);
                toPoint.y = transform.position.y;
                transform.LookAt(toPoint);
            }
			yield return new WaitForFixedUpdate();
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
}

