using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TextDisplay : MonoBehaviour
{

    public string textValue;
    public Text textElement;
    public Stats StatsScript;

    public GameObject atomPrefab;

    int dp = 2;
    float dpFactor;

    float radius;

    // Start is called before the first frame update
    void Start()
    {

        // textValue = "N: " + StatsScript.N.ToString();
        // textElement.text = textValue;

        dpFactor = (float)Math.Pow(10, dp);
        radius = atomPrefab.GetComponent<AtomProxy>().ScatteringRadius;
    }

    // Update is called once per frame
    void Update()
    {
        textValue = "N: " + StatsScript.N.ToString();
        textValue += "\n";
        textValue += "R: " + radius.ToString() + " m";
        textValue += "\n";
        textValue += "U: " + StatsScript.totIntEnergy.ToString() + " J";
        textValue += "\n";
        textValue += "V\u2080: " + ((float)Math.Floor(dpFactor*StatsScript.V0)/dpFactor).ToString() + " m\u00B3";
        textValue += "\n";
        textValue += "V\u2081: " + ((float)Math.Floor(dpFactor*StatsScript.V1)/dpFactor).ToString() + " m\u00B3";
        textValue += "\n";
        textValue += "V\u2082: " + ((float)Math.Floor(dpFactor*StatsScript.V2)/dpFactor).ToString() + " m\u00B3";
        

        textElement.text = textValue;
    }

    // Write function to add line 
    // void addLine(Text DisplayText, Text Variable)
    // {
        ///
    // }
}
