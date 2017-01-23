using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour {

    public float spawnInterval = 8.0f;
    public float lowerBound = 0.5f;
    public float upperBound = 20.0f;
    public float spawnIncr = 0.2f;
    private bool hovering = false;

    private List<TrackWayPoint> startPoints;

    public GameObject car;

	// Use this for initialization
	void Start ()
    {
        startPoints = new List<TrackWayPoint>();
		Collider[] colliders = new Collider[0];// = Physics.OverlapBox(transform.position, transform.GetComponent<BoxCollider>().size * 0.5f, transform.rotation, 1 << 8);
        foreach (Collider col in colliders)
            startPoints.Add(col.gameObject.GetComponent<TrackWayPoint>());
        StartCoroutine("Spawn");

    }

    // Update is called once per frame
    void Update ()
    {
	    if (hovering)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                spawnInterval += spawnIncr;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                spawnInterval -= spawnIncr;

            if (spawnInterval > upperBound)
                spawnInterval = upperBound;
            else if (spawnInterval < lowerBound)
                spawnInterval = lowerBound;

            spawnInterval = (float)Math.Round(spawnInterval,1);
        }
	}

    void OnGUI()
    {
        if (hovering)
            GUI.Label(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y + 15, 100, 25),spawnInterval.ToString());
        
    }

    void OnMouseEnter()
    {
        hovering = true;
    }

    void OnMouseExit()
    {
        hovering = false;
    }

    IEnumerator Spawn()
    {
        while(true)
        {
            foreach (TrackWayPoint point in startPoints)
            {
                Collider[] colliders = Physics.OverlapSphere(point.transform.position, 1);
                //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sphere.transform.position = (startPoints[counter%startPoints.Count].transform.position);
                //sphere.transform.localScale = new Vector3(2.0f,2.0f,2.0f);
                bool full = false;
                foreach (Collider col in colliders)
                {
                    if (col.gameObject.tag == "Vehicle")
                        full = true;
                }
                if (!full)
                {
                    GameObject instance = Instantiate(car);
                    //instance.name = "Car #" + vin;
                    //++vin;
                    instance.GetComponent<AIMController>().start = point;

                    MeshRenderer[] meshes = instance.GetComponentsInChildren<MeshRenderer>();
                    Color colour = new Color(UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f));
                    foreach (MeshRenderer mesh in meshes)
                        mesh.material.color = colour;
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
