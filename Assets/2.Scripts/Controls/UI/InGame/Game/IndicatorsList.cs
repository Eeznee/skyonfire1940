using UnityEngine;
using UnityEngine.UI;
using System;

public class IndicatorsList : MonoBehaviour
{
    public bool gsp;
    public bool ias;
    public bool roc;
    public bool alt;
    public bool thr;
    public bool ammo;
    public bool fuel;
    public bool gfr;
    public bool hdg;
    public bool temp;

    private void Start()
    {
        gsp = gsp && PlayerPrefs.GetInt("GSP", 1) == 1;
        ias = ias && PlayerPrefs.GetInt("IAS", 0) == 1;
        roc = roc && PlayerPrefs.GetInt("ROC", 0) == 1;
        alt = alt && PlayerPrefs.GetInt("ALT", 1) == 1;
        thr = thr && PlayerPrefs.GetInt("THR", 1) == 1;
        ammo = ammo && PlayerPrefs.GetInt("AMMO", 1) == 1;
        fuel = fuel && PlayerPrefs.GetInt("FUE", 1) == 1;
        gfr = gfr && PlayerPrefs.GetInt("GFR", 0) == 1;
        hdg = hdg && PlayerPrefs.GetInt("HDG", 0) == 1;
        temp = temp && PlayerPrefs.GetInt("TEMP", 1) == 1;
    }

    public string Text()
    {
        if (!Player.complex) return "";
        SofAircraft aircraft = Player.aircraft;
        ObjectData data = Player.complex.data;
        
        string finalText = "";
        if (gsp)
            finalText += "GSP : " + Mathf.RoundToInt(data.gsp.Get * UnitsConverter.speed.Multiplier) + " " + UnitsConverter.speed.Symbol + "\n";
        if (ias)
            finalText += "IAS : " + Mathf.RoundToInt(data.ias.Get * UnitsConverter.speed.Multiplier) + " " + UnitsConverter.speed.Symbol + "\n";
        if (roc)
            finalText += "ROC : " + Mathf.RoundToInt(data.vsp.Get * UnitsConverter.climbRate.Multiplier) + " " + UnitsConverter.climbRate.Symbol + "\n";
        if (alt)
            finalText += "ALT : " + Mathf.RoundToInt(data.altitude.Get * UnitsConverter.altitude.Multiplier) + " " + UnitsConverter.altitude.Symbol + "\n";

        if (!aircraft) return finalText;

        if (thr)
            finalText += "THR : " + (aircraft.boost ? "BOOST\n" : (Mathf.RoundToInt(aircraft.throttle * 100f) + " %\n"));

        if (ammo && aircraft.primaries.Length > 0)
        {
            string clip = aircraft.primaries[0].magStorage ? " | " + aircraft.primaries[0].magStorage.magsLeft : "";
            finalText += "PRM : " + Gun.AmmunitionCount(aircraft.primaries) + clip + " \n";
        }
        if (ammo && aircraft.secondaries.Length > 0)
        {
            string clip = aircraft.secondaries[0].magStorage ? " | " + aircraft.secondaries[0].magStorage.magsLeft : "";
            finalText += "SCD : " + Gun.AmmunitionCount(aircraft.secondaries) +  clip + " \n";
        }
        if (fuel && aircraft)
        {
            int fuelTime = (int)aircraft.fuelSystem.FuelTimer;
            string minutes = (fuelTime / 60).ToString("D2");
            string seconds = (fuelTime % 60).ToString("D2");
            finalText += "FUE : " + minutes + ":" + seconds + "\n";
        }

        if (gfr)
            finalText += "GFR : " + Mathf.CeilToInt(data.gForce * 10f) / 10f + " G\n";
        if (hdg)
            finalText += "HDG : " + Mathf.CeilToInt(data.heading.Get) + " \n";
        if (!aircraft ||aircraft.engines.Length == 0) return finalText;
        if (temp)
            finalText += "WTR : " + aircraft.engines[0].temp.waterTemperature.ToString("0.0") + " °C\n";
        if (temp)
            finalText += "OIL : " + aircraft.engines[0].temp.oilTemperature.ToString("0.0") + " °C\n";


        return finalText;
    }
}
