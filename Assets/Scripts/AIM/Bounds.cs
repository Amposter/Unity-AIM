using UnityEngine;
using System.Collections;

public class Bounds : MonoBehaviour {

	private bool hasCollided = false;

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag == "Bounds")
			hasCollided = true;
	}

	public bool collided()
	{
		return hasCollided;
	}

}
