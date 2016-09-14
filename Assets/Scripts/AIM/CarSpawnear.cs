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
        Debug.Assert(spawnPoints.Length == spawnDirections.Length);
        //InvokeRepeating("Spawn", 1.0f, 1.5f); //Invoke("Spawn", 1.0f);
        Invoke("Spawn", 0.0f);
        //Invoke("Spawn", 9.5f);
    }

    void Spawn()
    {
        Instantiate(car);
    }

	// Update is called once per frame
	void Update () {
	
	}
}
