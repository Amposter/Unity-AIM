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
	    for (int i = 0; i < spawnPoints.Length; ++i)
        {
            Instantiate(car, spawnPoints[i], spawnDirections[i]);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
