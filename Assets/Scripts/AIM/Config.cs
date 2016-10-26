﻿using UnityEngine;
using System.Collections;

//A collection of setup and configuration variables as well as for car ID's
public class Config {

    public static bool NEAT = false;
    public static bool HyperNEAT = true;

    // Use this for initialization
    public static int lastVIN = 0;

    public static float[] reserverationOffsets =
    {
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f,
        0.1f
    };

    public static int[] decimalPlaces =
    {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1
    };

    public static int[] samplingSteps =
    {
        6,
        6,
        15,
        3,
        6,
        8,
        7,
        5,
        3,
        3
    };
}
