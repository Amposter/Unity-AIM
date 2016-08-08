using UnityEngine;
using System.Collections;

public class Pedestrian : MonoBehaviour
{
	Vector3 walkDirection = Vector3.zero;
	public bool isDone = false;
	int startID = 0;
	string parentName = "";

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		gameObject.transform.position += walkDirection * Time.deltaTime;
	}

	public void walk(string parentName, int startID, Vector3 walkDirection)
	{
		this.parentName = parentName;
		this.startID = startID;
		this.walkDirection = walkDirection;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!parentName.Equals (other.transform.parent.gameObject.name))
		{
			return;
		}
		if(startID == 1 && other.gameObject.tag.Equals("Pedestrian2"))
		{
			isDone = true;
		}
		else if(startID == 2 && other.gameObject.tag.Equals("Pedestrian1"))
		{
			isDone = true;
		}
		
	}
}
