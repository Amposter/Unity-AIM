﻿using UnityEngine;
using System.Collections;

public class Lane : MonoBehaviour {

    //Lanes the vehicle can turn into, null otherwise
    public Lane right;
    public Lane left;
    public Lane straight;

    //Bezier Curves for the turn trajectory, null otherwise
    public GameObject leftPath;
    public GameObject rightPath;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
