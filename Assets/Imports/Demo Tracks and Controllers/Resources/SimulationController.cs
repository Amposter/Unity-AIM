using UnityEngine;
using System.Collections;

public class SimulationController : MonoBehaviour
{

	public float timeScale = 1;

    public PathManager[] pathManagers;

	float normalFixedDeltaTime;

	// Use this for initialization
	void Start ()
	{
		normalFixedDeltaTime = Time.fixedDeltaTime;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Time.timeScale = timeScale;
		Time.fixedDeltaTime = normalFixedDeltaTime * (1/timeScale);
	}
}
