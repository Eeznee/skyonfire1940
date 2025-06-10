using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugIndicators : MonoBehaviour
{
    public Text text;

    void Start()
    {
    }
    void Update()
    {
        text.text = Text();
    }
    public string Text()
    {
        if (!Player.aircraft) return "";
        SofAircraft aircraft = Player.aircraft;
        ObjectData data = Player.modular.data;

        string finalText = "";

        finalText += "MASS   : " + aircraft.rb.mass.ToString("0.0") + " kg\n";
        finalText += "FUEL   : " + aircraft.fuel.TotalFuel.ToString("0.0") + " kg\n";
        finalText += "AMMO   : " + aircraft.armament.TotalAmmoMass.ToString("0.0") + " kg\n";
        finalText += "BOMBS  : " + aircraft.armament.TotalOrdnanceMass.ToString("0.0") + " kg\n";

        finalText += "\n";

        finalText += "Δ ALT  : " + aircraft.data.relativeAltitude.Get.ToString("0.0") + " m\n";
        finalText += "BANK   : " + aircraft.data.bankAngle.Get.ToString("0.0") + " °\n";
        finalText += "PITCH  : " + aircraft.data.pitchAngle.Get.ToString("0.0") + " °\n";
        finalText += "ALPHA  : " + aircraft.data.angleOfAttack.Get.ToString("0.0") + " °\n";
        finalText += "TURN   : " + aircraft.data.turnRate.Get.ToString("0.0") + " °/s\n";
        finalText += "ROLL   : " + aircraft.data.rollRate.Get.ToString("0.0") + " °/s\n";

        finalText += "\n";

        float rpm = aircraft.engines.Main.RadPerSec * 60f / (Mathf.PI * 2f);
        float totalThrust = 0f;

        foreach(JetEngine jetEngine in aircraft.engines.AllJetEngines)
        {
            totalThrust += jetEngine.Thrust;
        }
        foreach(Propeller propeller in aircraft.engines.Propellers)
        {
            totalThrust += propeller.Thrust;
        }

        finalText += "RPM    : " + rpm.ToString("0") + "\n";
        if (aircraft.engines.Main.Class == EngineClass.PistonEngine)
        {
            finalText += "POWER  : " + (aircraft.engines.AllPistonEngines[0].BrakePower / 745.7f).ToString("0.0") + " hp\n";
            finalText += "M.P.   : " + aircraft.engines.AllPistonEngines[0].ManifoldPressureInAppropriateUnit() + "\n";
            finalText += "GEAR   : " + (aircraft.engines.AllPistonEngines[0].SuperchargerSetting + 1) +"\n";
        }

        finalText += "THRUST : " + totalThrust.ToString("0") + " N\n";
        finalText += "TEMP   : " + aircraft.engines.Main.Temp.Temperature.ToString("0.0") + " °C\n";
        finalText += "HEALTH : " + (aircraft.engines.Main.structureDamage * 100f).ToString("0.0") + " %\n";

        if (aircraft.engines.Main.Class == EngineClass.PistonEngine)
        {
            float bladeAngle = ((PistonEngine)aircraft.engines.Main).propeller.BladeAngle;
            finalText += "PROP PITCH : " + bladeAngle.ToString("0.0") + " °\n";

            float propEfficiency = ((PistonEngine)aircraft.engines.Main).propeller.BladeEfficiency() * 100f;
            finalText += "PROP EFF   : " + propEfficiency.ToString("0.0") + " %\n";
        }

        finalText += "\n";
        finalText += "TEMP     : " + aircraft.data.temperature.Get.ToString("0.0") + " °C\n";
        finalText += "PRESSURE : " + (aircraft.data.pressure.Get / Aerodynamics.SeaLvlPressure).ToString("0.00") + " bar\n";
        finalText += "DENSITY  : " + aircraft.data.density.Get.ToString("0.00") + " kg/m³\n";
        return finalText;
    }
}
