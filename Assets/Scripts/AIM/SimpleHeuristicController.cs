using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : MonoBehaviour {

    public float speed = 5f;
    public bool driving;
    private BezierCurve curve;


    protected int resolution; //Number of points to sample on each path curve

	// Use this for initialization
	protected virtual void Start () {
        driving = true;
        resolution = 20;
	}

    // Update is called once per frame
    protected virtual void Update () {
        
	}

    public void setCurve(BezierCurve curve)
    {
        this.curve = curve;
    }

    protected virtual IEnumerator Drive ()
    {
        int offset = 1;
        Vector3 toPoint = curve.GetPointAt(offset /(float)resolution);
        toPoint.y = transform.position.y;
        transform.LookAt(toPoint);

        while (offset <= resolution)
        {
            while (!driving) //Do nothing while paused
            {

                /*if (endOfCurve) //Don't continue with the same curve
                    driving = true;
                break;*/
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
        /* if (driving) //You've reached the end of the curve
         {
             driving = false;
             endOfCurve = false;
             ++currCuve;
             offset = 5;
         }*/
    }

   
}
