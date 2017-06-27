using UnityEngine;
using System.Collections;

//Car placeholder class. For spawning an invisible vehicle at a position until the actual vehicle arrives there.
public class Placeholder : MonoBehaviour {

    public string id;

    void OnTriggerExit(Collider col)
    {
        if (id.Equals(col.gameObject.name))
            Destroy(gameObject);
    }
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
