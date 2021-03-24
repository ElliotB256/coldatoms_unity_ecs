using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVWriter : MonoBehaviour
{
    public Stats StatsScript;
    string filename = "";
    TextWriter tw;
    string Headings = "Frame, N, U, mfp, mct";
    string PHeading = "";

    int N;
    float U;
    float mfp;
    float mct;
    float[] P = new float[20];
    int frame = 0;
    

    void Awake()
    {
        Stats StatsScript = GetComponent<Stats>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
            // Pressure Headings
        for (int i = 0; i < P.Length; i ++)
        {
            PHeading += ", P" + i.ToString();
        }

        filename = System.IO.Directory.GetCurrentDirectory() + "/Assets/Core/Data/DataFileCSV.csv"; //Application.dataPath
        Debug.Log(filename);
            // false deletes everything previously
        tw = new StreamWriter(filename, false);

            // Headings
        tw.WriteLine(Headings + PHeading);
        tw.Close();

    }

    // Update is called once per frame
    void LateUpdate()
    {
        frame ++;

        PullData();
        WriteCSV();

    }

    public void PullData()
    {
        N = StatsScript.N;
        U = StatsScript.totIntEnergy;
        mfp = StatsScript.mfp;
        mct = StatsScript.mct;
        P = StatsScript.pressures;

    }

    public void WriteCSV()
    {
            // true appends to the end of the file
        tw = new StreamWriter(filename, true);

            // As this is not just a string I am not sure how to write this more efficiently
        tw.WriteLine(frame + "," + N + "," + U + "," + mfp + "," + mct + "," + P[0] + "," + P[1] + "," + P[2] + "," + P[3] + "," + P[4]
        + "," + P[5] + "," + P[6] + "," + P[7] + "," + P[8] + "," + P[9] + "," + P[10] + "," + P[11] + "," + P[12]
        + "," + P[13] + "," + P[14] + "," + P[15] + "," + P[16] + "," + P[17] + "," + P[18] + "," + P[19]);

        tw.Close();
    }

}
