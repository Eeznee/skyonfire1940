using UnityEngine;
using UnityEngine.UI;

public class IndicatorsList : MonoBehaviour
{
    public bool gsp;
    public bool ias;
    public bool roc;
    public bool alt;
    public bool thr;
    public bool ammo;
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
        gfr = gfr && PlayerPrefs.GetInt("GFR", 0) == 1;
        hdg = hdg && PlayerPrefs.GetInt("HDG", 0) == 1;
        temp = temp && PlayerPrefs.GetInt("TEMP", 1) == 1;
    }

    public string Text()
    {
        SofAircraft player = PlayerManager.player.aircraft;
        ObjectData data = player.data;

        string finalText = "";
        if (gsp)
            finalText += "GSP : " + Mathf.RoundToInt(data.gsp.Get * UnitsConverter.speed.Multiplier) + " " + UnitsConverter.speed.Symbol + "\n";
        if (ias)
            finalText += "IAS : " + Mathf.RoundToInt(data.ias.Get * UnitsConverter.speed.Multiplier) + " " + UnitsConverter.speed.Symbol + "\n";
        if (roc)
            finalText += "ROC : " + Mathf.RoundToInt(data.vsp.Get * UnitsConverter.climbRate.Multiplier) + " " + UnitsConverter.climbRate.Symbol + "\n";
        if (alt)
            finalText += "ALT : " + Mathf.RoundToInt(player.transform.position.y * UnitsConverter.altitude.Multiplier) + " " + UnitsConverter.altitude.Symbol + "\n";
        if (thr)
            finalText += "THR : " + (player.boost ? "BOOST\n" : (Mathf.RoundToInt(player.throttle * 100f) + " %\n"));

        if (ammo && player.primaries.Length > 0)
        {
            string clip = player.primaries[0].magStorage ? " | " + player.primaries[0].magStorage.magsLeft : "";
            finalText += "PRM : " + Gun.AmmunitionCount(player.primaries) + " \n";
        }
        if (ammo && player.secondaries.Length > 0)
        {
            string clip = player.secondaries[0].magStorage ? " | " + player.secondaries[0].magStorage.magsLeft : "";
            finalText += "SCD : " + Gun.AmmunitionCount(player.secondaries) +  clip + " \n";
        }

        if (gfr)
            finalText += "GFR : " + Mathf.CeilToInt(data.gForce * 10f) / 10f + " G\n";
        if (hdg)
            finalText += "HDG : " + Mathf.CeilToInt(data.heading.Get) + " \n";
        if (player.engines.Length == 0) return finalText;
        if (temp)
            finalText += "WTR : " + player.engines[0].waterTemperature.ToString("0.0") + " °C\n";
        if (temp)
            finalText += "OIL : " + player.engines[0].oilTemperature.ToString("0.0") + " °C\n";


        return finalText;
    }
}
