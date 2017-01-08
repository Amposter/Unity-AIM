using UnityEngine;
using System.Collections;

public class PedestrianSpawner2D : MonoBehaviour {

	Vector2 startPoint;
	Vector2 direction;
	private float speed = 2f;
	private int numPedestrians = 4;
	private float deniedRefreshInterval = 0.5f;
	private float spawnInterval = 7.5f;
	private float baseSpawnTime = 1f;
	private float randomInterval = 2f;
	public GameObject pedestrian;
	GameObject[] pedestrians;

	public void SetUp()
	{
		startPoint = transform.GetChild (0).transform.position;
		direction = ((Vector2)transform.GetChild (1).transform.position - startPoint);
		pedestrians = new GameObject[numPedestrians];
	}

	public void UpdatePedestrians()
	{
		foreach (GameObject p in pedestrians)
		{
			if (p == null)
				continue;
			else 
			{
				Vector2 pos = direction.normalized * speed * Time.fixedDeltaTime;
				p.transform.position += new Vector3(pos.x,pos.y,0);
			}
		}
	}

	bool IsClear()
	{
		RaycastHit2D hit;
		hit = Physics2D.Raycast (startPoint, direction, direction.magnitude, 1<<10);
		return !hit;
	}

	public void Clean()
	{
		foreach (Pedestrian2D p in pedestrians)
			Destroy (p);
	}
	public IEnumerator SpawnPedestrians()
	{
		
		while (true) 
		{
			yield return new WaitForSeconds (spawnInterval);
			int counter = 0;
			pedestrians = new GameObject[numPedestrians];
			while (counter < numPedestrians) 
			{
				while (!IsClear ()) {
					Debug.Log ("Cannot spawn peds");
					yield return new WaitForSeconds (deniedRefreshInterval);
				}
				pedestrians [counter] = Instantiate (pedestrian);
				pedestrians [counter].transform.position = startPoint;
				++counter;
				yield return new WaitForSeconds (baseSpawnTime + Random.Range(0,randomInterval));
			}
		}
	}

}
