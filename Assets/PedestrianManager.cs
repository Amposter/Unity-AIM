using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PedestrianManager : MonoBehaviour {

	private PedestrianStartBoundary[] spawnPoints;
	private bool active = false;
	private List<GameObject> pedestrians;
	public GameObject pedestrian;

	// Use this for initialization
	void Start () 
	{
		pedestrians = new List<GameObject> ();
		GameObject[] temp = GameObject.FindGameObjectsWithTag ("PedestrianStartBoundary");
		spawnPoints = new PedestrianStartBoundary[temp.Length];
		for (int i = 0; i < temp.Length; ++i)
			spawnPoints [i] = temp [i].GetComponent<PedestrianStartBoundary> ();
	}
	
	void FixedUpdate()
	{
		if (active) 
		{
			for (int i = 0; i < pedestrians.Count; ++i) 
			{
				GameObject p = pedestrians [i];
				if (p == null)
					pedestrians.Remove (p);
				else
					p.GetComponent<Pedestrian> ().UpdatePosition ();
			}
		}
	}

	IEnumerator SpawnPedestrians()
	{
		yield return new WaitForSeconds (10f); //Start spawning 10 seconds after level begins
		while (true) 
		{
			PedestrianStartBoundary spawner = spawnPoints [Random.Range (0, spawnPoints.Length)];
			int count = Random.Range (1, 11); // 1 <= x < 11
			for (int i = 0; i < count; ++i) 
			{
				Vector3[] path = spawner.getPath ();
				GameObject ped = GameObject.Instantiate(pedestrian);
				ped.GetComponent<Pedestrian> ().SetPath (path);
				ped.transform.position = path [0];
				pedestrians.Add (ped);
				yield return new WaitForSeconds (Random.Range(0, 1.0f));
			}
			yield return new WaitForSeconds (5 + UnityEngine.Random.Range (1, 10));
		}
	}

	public void Activate()
	{
		active = true;
		StartCoroutine ("SpawnPedestrians");
	}

	public void Deactivate()
	{
		active = false;
		StopCoroutine ("SpawnPedestrians");
		for (int i = 0; i < pedestrians.Count; ++i) 
		{
			GameObject p = pedestrians [i];
			if (p != null)
				Destroy (p);
		}
		pedestrians.Clear ();
			
	}

	public bool isActive()
	{
		return active;
	}
}
