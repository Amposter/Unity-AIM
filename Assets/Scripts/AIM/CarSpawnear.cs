using UnityEngine;
using System.Collections;

public class CarSpawnear : MonoBehaviour {

    //Spawn points + Angles
    public Vector3[] spawnPoints;
    public Quaternion[] spawnDirections;

    //Car prefab to be used
    public GameObject car;

	// Use this for initialization
	void Start () {
        Debug.Log("hey");
        Debug.Assert(spawnPoints.Length == spawnDirections.Length);
        InvokeRepeating("Spawn", 1.0f, 1.5f);
	  /*  for (int i = 0; i < spawnPoints.Length; ++i)
        {
            Instantiate(car, transform.position + spawnPoints[i], spawnDirections[i]);
        }*/
	}
	
    void Spawn()
    {
     //   while (true)
        {
            Instantiate(car);
           // yield Wait
        }
    }
	// Update is called once per frame
	void Update () {
	
	}
}
