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

        ArmamentManager arm = aircraft.armament;

        if (thr)
        {
            finalText += "THR : ";

            bool boosting = false;
            bool effectiveBoosting = false;
            bool takeOffBoost = false;
            float lowestRemainingBoost = Mathf.Infinity;

            foreach(PistonEngine engine in aircraft.engines.AllPistonEngines)
            {
                boosting |= engine.RunMode != EngineRunMode.Continuous && engine.PistonPreset.HasBoost;
                effectiveBoosting |= engine.BoostIsEffective;
                takeOffBoost |= engine.RunMode == EngineRunMode.TakeOffBoost;
                if (engine.BoostTime < lowestRemainingBoost) lowestRemainingBoost = engine.BoostTime;
            }

            if (boosting)
            {
                finalText += takeOffBoost ? "T/O " : "WEP ";

                if (!effectiveBoosting) finalText += "(TOO HIGH)";
                else if (lowestRemainingBoost <= 0f) finalText += "(DANGER)";
                else finalText += "(" + SecondsToMinuted(lowestRemainingBoost) + ")";
            }
            else
                finalText += Mathf.RoundToInt(aircraft.engines.Throttle * 100f) + "%";
            
            finalText += "\n";
        }


        if (ammo && arm.primaries.Length > 0)
        {
            string clip = arm.primaries[0].magStorage ? " | " + arm.primaries[0].magStorage.magsLeft : "";
            finalText += "PRM : " + Gun.AmmunitionCount(arm.primaries) + clip + " \n";
        }
        if (ammo && arm.secondaries.Length > 0)
        {
            string clip = arm.secondaries[0].magStorage ? " | " + arm.secondaries[0].magStorage.magsLeft : "";
            finalText += "SCD : " + Gun.AmmunitionCount(arm.secondaries) +  clip + " \n";
        }
        if (fuel && aircraft)
        {
            finalText += "FUE : " + SecondsToMinuted(aircraft.fuel.FuelTimer) + "\n";
        }

        if (gfr)
            finalText += "GFR : " + Mathf.CeilToInt(data.gForce * 10f) / 10f + " G\n";
        if (hdg)
            finalText += "HDG : " + Mathf.CeilToInt(data.heading.Get) + " \n";

        if (!aircraft || aircraft.engines.AllEngines.Length == 0) return finalText;
        if (temp)
            finalText += "WTR : " + aircraft.engines.Main.Temp.WaterTemperature.ToString("0.0") + " °C\n";
        if (temp)
            finalText += "OIL : " + aircraft.engines.Main.Temp.OilTemperature.ToString("0.0") + " °C\n";
        if (temp)
            finalText += "ENG : " + aircraft.engines.Main.Temp.Temperature.ToString("0.0") + " °C\n";
        if (temp)
            finalText += "DMG : " + (aircraft.engines.Main.structureDamage * 100f).ToString("0.0") + " %\n";

        finalText += "\n";

        //TODO: Implement rpm as settings
        float rpm = aircraft.engines.Main.RadPerSec * 60f / (Mathf.PI * 2f);
        finalText += "RPM : " + rpm.ToString("0") + "\n";

        float bladeAngle = ((PistonEngine) aircraft.engines.Main).propeller.BladeAngle;
        finalText += "P-ALPHA : " + bladeAngle.ToString("0.0") + " °\n";

        float propEfficiency = ((PistonEngine)aircraft.engines.Main).propeller.BladeEfficiency() * 100f;
        finalText += "P-EFF : " + propEfficiency.ToString("0.0") + " %\n";

        return finalText;
    }
    public string SecondsToMinuted(float seconds)
    {
        int secondsInt = Mathf.RoundToInt(seconds);
        string minutesTxt = (secondsInt / 60).ToString("D2");
        string secondsTxt = (secondsInt % 60).ToString("D2");
        return minutesTxt + ":" + secondsTxt;
    }
}
