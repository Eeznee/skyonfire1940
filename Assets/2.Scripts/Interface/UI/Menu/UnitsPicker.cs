using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsPicker : MonoBehaviour
{
    public enum Usage
    {
        Altitude,
        Distance,
        Speed,
        Climb,
        Mass,
    }
    public Usage unitUsage;
    public void Switch(int unitIndex)
    {
        switch (unitUsage)
        {
            case Usage.Altitude:
                UnitsConverter.altitude.ChangeOption(unitIndex);
                break;
        }
    }
    void Start()
    {
        
    }
}
