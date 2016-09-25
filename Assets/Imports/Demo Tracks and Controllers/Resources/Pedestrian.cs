using UnityEngine;
using System.Collections;

public class Pedestrian : MonoBehaviour
{
	Vector3 walkDirection = Vector3.zero;
	public bool isDone = false;
	public int startID = 0;
	string parentName = "";
    bool waiting = false;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
        if(waiting == false)
        {
            gameObject.transform.position += walkDirection * Time.deltaTime;
        }   
	}

	public void walk(string parentName, int startID, Vector3 walkDirection)
	{
		this.parentName = parentName;
		this.startID = startID;
		this.walkDirection = walkDirection;
        transform.rotation = Quaternion.LookRotation(walkDirection.normalized, Vector3.up);
	}

    public void OnTriggerStay(Collider other)
    {
		if (!other.gameObject.tag.Equals ("Vehicle"))
		{
			return;
		}
		if (!waiting)
        {
            waiting = true;
            Invoke("stopWaiting", 0.5f);
        }
    }

    public void stopWaiting()
    {
        waiting = false;
    }

	public void OnTriggerEnter(Collider other)
	{   
		if(other.gameObject.tag.Equals("Pedestrian1") && startID == 2)
		{
			isDone = true;
		}
		else if(other.gameObject.tag.Equals("Pedestrian2") && startID == 1)
		{
			isDone = true;
		}
	}

}
