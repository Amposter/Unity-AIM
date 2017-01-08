using UnityEngine;
using System.Collections;

public class Pedestrian2D : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "Pedestrian2" || col.tag == "Vehicle") 
		{
			Destroy (gameObject);
		}
	}
}
