using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : MonoBehaviour {

    public GameObject BZ;
    public float speed = 5f;
    public bool driving;

    protected BezierCurve[] curves;
    protected bool endOfCurve;
    protected int resolution; //Number of points to sample on each path curve

    private int currCuve;
    private float offset;

	// Use this for initialization
	protected virtual void Start () {
        driving = true;
        endOfCurve = false;
        offset = 1;
        currCuve = 0;
        resolution = 5;
	}

    // Update is called once per frame
    protected virtual void Update () {
        
	}

    IEnumerator Drive ()
    {
        BezierCurve curve = curves[currCuve];
        Vector3 toPoint = curve.GetPointAt(offset /(float)resolution);
        toPoint.y = transform.position.y;
        transform.LookAt(toPoint);

        while (offset <= resolution)
        {
            if (!driving) //You haven't reached end of the curve but you need to stop e.g. - pedestrians
            {
                if (endOfCurve) //Don't continue with the same curve
                    driving = true;
                break;
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
        if (driving) //You've reached the end of the curve
        {
            driving = false;
            endOfCurve = false;
            ++currCuve;
            offset = 1;
        }
    }

   
}
