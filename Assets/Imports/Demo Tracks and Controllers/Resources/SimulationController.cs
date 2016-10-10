using UnityEngine;
using System.Collections;

public class SimulationController : MonoBehaviour
{

	//public float timeScale = 5;
	public bool renderPreview = true;
	float normalFixedDeltaTime;

	public bool[] selectedTracks;
	public GameObject[] tracksList = new GameObject[11];

	int currentTrack = -1;

	// Use this for initialization
	void Start ()
	{
		normalFixedDeltaTime = Time.fixedDeltaTime;
		foreach (GameObject track in tracksList)
		{
			track.SetActive (false);
		}
		//Camera.main.gameObject.SetActive (renderPreview); 

		loadNextTrack ();
	}


	// Update is called once per frame
	void Update ()
	{
		//Time.timeScale = timeScale;
		//Time.fixedDeltaTime = normalFixedDeltaTime * (1/timeScale);


	}

	//loads the next selected track and returns true
	//if there are no more selected tracks left, returns false
	public bool loadNextTrack()
	{
		for (int i = currentTrack+1; i < tracksList.Length; i++)
		{
			if (selectedTracks [i])
			{
				loadTrack (i);
				return true;
			}
		}
		return false;
	}

	public void loadTrack(int trackNum)
	{
		if (currentTrack != -1) {
			tracksList [currentTrack].SetActive (false);
		}
		tracksList [trackNum].SetActive (true);
		currentTrack = trackNum;

		if (!renderPreview) {
			return;
		}

		switch (trackNum)
		{
		case 0:
			Camera.main.transform.position = new Vector3 (0,175,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 1:
			Camera.main.transform.position = new Vector3 (0,76,2);
			Camera.main.transform.rotation = Quaternion.Euler(90,90,0);
			break;
		case 2:
			Camera.main.transform.position = new Vector3 (0,106,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 3:
			Camera.main.transform.position = new Vector3 (0,106,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 4:
			Camera.main.transform.position = new Vector3 (0,156,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 5:
			Camera.main.transform.position = new Vector3 (0,86,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 6:
			Camera.main.transform.position = new Vector3 (0,160,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 7:
			Camera.main.transform.position = new Vector3 (0,109,-2.9f);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 8:
			Camera.main.transform.position = new Vector3 (0,58,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 9:
			Camera.main.transform.position = new Vector3 (-7,55,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		case 10:
			Camera.main.transform.position = new Vector3 (0,135,0);
			Camera.main.transform.rotation = Quaternion.Euler(90,0,0);
			break;
		}

	}

	public GameObject getCurrentTrack()
	{
		return tracksList [currentTrack];
	}
}
