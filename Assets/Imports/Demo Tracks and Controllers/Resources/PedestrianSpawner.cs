using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{

    public GameObject pedestrianPrefab;
	public int maxPedestrians;
	public float periodInterval;
	public float periodClusterSize;
	public float periodTimeScatter;
	public float pedestrianSpeed;

	private float timer = 0;

	List<GameObject> pedestrianList;
	Vector3 moveVector;

	// Use this for initialization
	void Start () 
	{
		pedestrianList = new List<GameObject> ();
		moveVector = (transform.GetChild (0).position - transform.GetChild (1).position).normalized * pedestrianSpeed;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
			if (timer >= periodInterval)
			{
				for (int i = 0; i < periodClusterSize; i++)
				{
					if (pedestrianList.Count >= maxPedestrians)
					{
						break;
					}
					Invoke ("SpawnPedestrian", (Random.value * periodTimeScatter) * Time.timeScale);
				}
				timer = 0;
			}

		foreach(GameObject pedestrian in pedestrianList)
		{
			if (pedestrian.GetComponent<Pedestrian> ().isDone)
			{
				GameObject.Destroy (pedestrian);
				pedestrianList.Remove (pedestrian);
				break;
			}
		}

		timer += Time.deltaTime;
	}

	public void SpawnPedestrian()
	{
		GameObject pedestrian;
		if (Random.Range (0, 2) < 1)
		{
			
			float xPos = transform.GetChild (0).position.x - transform.GetChild (0).gameObject.GetComponent<BoxCollider> ().bounds.extents.x + 
						 Random.value * (transform.GetChild (0).gameObject.GetComponent<BoxCollider> ().bounds.extents.x * 2);
			
			float zPos = transform.GetChild (0).position.z - transform.GetChild (0).gameObject.GetComponent<BoxCollider> ().bounds.extents.z + 
						 Random.value * (transform.GetChild (0).gameObject.GetComponent<BoxCollider> ().bounds.extents.z * 2);
			
			pedestrian = (GameObject) GameObject.Instantiate (pedestrianPrefab, new Vector3 (xPos, transform.GetChild (0).position.y, zPos), new Quaternion ());

			pedestrian.GetComponent<Pedestrian> ().walk (gameObject.name, 1, -moveVector);

		}
		else
		{
			float xPos = transform.GetChild (1).position.x - transform.GetChild (1).gameObject.GetComponent<BoxCollider> ().bounds.extents.x + 
				Random.value * (transform.GetChild (1).gameObject.GetComponent<BoxCollider> ().bounds.extents.x * 2);

			float zPos = transform.GetChild (1).position.z - transform.GetChild (1).gameObject.GetComponent<BoxCollider> ().bounds.extents.z + 
				Random.value * (transform.GetChild (1).gameObject.GetComponent<BoxCollider> ().bounds.extents.z * 2);

            pedestrian = (GameObject)Instantiate(pedestrianPrefab, new Vector3(xPos, transform.GetChild(1).position.y, zPos), new Quaternion());

            pedestrian.GetComponent<Pedestrian> ().walk (gameObject.name, 2, moveVector);

		}
		pedestrianList.Add (pedestrian);
	}


}
