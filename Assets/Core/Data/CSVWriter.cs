using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVWriter : MonoBehaviour
{
    public Stats StatsScript;
    string filename = "";
    TextWriter tw;
    string Headings = "Frame, Time, dT, U, U0, U1, U2, V, V0, V1, V2, T0, T1, T2, N, N0, N1, N2, mfp, mct";
    string PHeading = "";

    int N;
    int N0;
    int N1;
    int N2;

    float U;
    float U0;
    float U1;
    float U2;

    float V;
    float V0;
    float V1;
    float V2;

    float T0;
    float T1;
    float T2;

    float mfp;
    float mct;
    float[] P = new float[20];
    int frame = 0;
    float Time;
    float dT;


    

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
        N0 = StatsScript.N0;
        N1 = StatsScript.N1;
        N2 = StatsScript.N2;

        U = StatsScript.totIntEnergy;
        U0 = StatsScript.U0;
        U1 = StatsScript.U1;
        U2 = StatsScript.U2;

        V = StatsScript.V;
        V0 = StatsScript.V0;
        V1 = StatsScript.V1;
        V2 = StatsScript.V2;

        T0 = StatsScript.T0;
        T1 = StatsScript.T1;
        T2 = StatsScript.T2;

        Time = StatsScript.currentTime;
        dT = StatsScript.dT;
        mfp = StatsScript.mfp;
        mct = StatsScript.mct;




        P = StatsScript.pressures;

    }

    public void WriteCSV()
    {
            // true appends to the end of the file
        tw = new StreamWriter(filename, true);

            // As this is not just a string I am not sure how to write this more efficiently
        tw.WriteLine(frame + "," + Time + "," + dT + "," + U + "," + U0 + "," + U1 + "," + U2 + ","
        + V + "," + V0 + "," + V1 + "," + V2 + "," + T0 + "," + T1 + "," + T2 + ","
        + N + "," + N0 + "," + N1 + "," + N2 + "," + mfp + "," + mct + "," 
        + P[0] + "," + P[1] + "," + P[2] + "," + P[3] + "," + P[4] + "," + P[5] + ","
        + P[6] + "," + P[7] + "," + P[8] + "," + P[9] + "," + P[10] + "," + P[11] + "," + P[12]
        + "," + P[13] + "," + P[14] + "," + P[15] + "," + P[16] + "," + P[17] + "," + P[18] + "," + P[19]);

        tw.Close();
    }

}
