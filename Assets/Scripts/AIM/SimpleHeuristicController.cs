using UnityEngine;
using System.Collections;

public class SimpleHeuristicController : MonoBehaviour {

    public GameObject BZ;
    protected bool driving;
    public float speed = 10f;
	// Use this for initialization
	protected virtual void Start () {
        driving = true;
	}

    // Update is called once per frame
    protected virtual void Update () {
        if (driving)
            Move();
	}

    protected void Move ()
    {
        transform.position += transform.forward * speed * Time.deltaTime; //Too naive
    }

   
}
