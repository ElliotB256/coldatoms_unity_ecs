using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public float mfp = 0f;
    public float mct = 0f;

    public float totIntEnergy = 0f;
    public float U0 = 0f;
    public float U1 = 0f;
    public float U2 = 0f;

    public int N = 0;
    public int N0 = 0;
    public int N1 = 0;
    public int N2 = 0;
    
    public float V = 0f;
    public float V0 = 0f;
    public float V1 = 0f;
    public float V2 = 0f;

    public float T0 = 0f;
    public float T1 = 0f;
    public float T2 = 0f;
    
    public float[] pressures = new float[20];

    public float currentTime = 0f;
    public float dT = 0f;

    public float OscillationNumber = 0f;
    public float MaxOscillations = 0f;

}
