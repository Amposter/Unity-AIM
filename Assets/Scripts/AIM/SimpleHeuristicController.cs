using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : MonoBehaviour {

    //Public variabes
    public float speed = 5f;

    //Private variables
    public bool paused;
    private BezierCurve curve;


    protected int resolution; //Number of points to sample on each path curve

	// Use this for initialization
	protected virtual void Start () {
        paused = false;
        resolution = 20;
	}

    // Update is called once per frame
    protected virtual void Update ()
    {
        
	}

    public void setCurve(BezierCurve curve)
    {
        this.curve = curve;
    }

    protected virtual IEnumerator Drive ()
    {
        Debug.Log("Driving normally");
        int offset = 1;
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
}
