using UnityEngine;

public class UnitsSwapper : MonoBehaviour
{
    public void SwitchAltitude(int option)
    {
        UnitsConverter.altitude.ChangeOption(option);
    }
    public void SwitchDistance(int option)
    {
        UnitsConverter.distance.ChangeOption(option);
    }
    public void SwitchAmmunitionSizes(int option)
    {
        UnitsConverter.ammunitionSizes.ChangeOption(option);
    }
    public void SwitchSpeed(int option)
    {
        UnitsConverter.speed.ChangeOption(option);
    }
    public void SwitchClimbRate(int option)
    {
        UnitsConverter.climbRate.ChangeOption(option);
    }
    public void SwitchMass(int option)
    {
        UnitsConverter.mass.ChangeOption(option);
    }

    public void UseMetrics()
    {
        UnitsConverter.UsePreset(UnitsConverter.metricPreset);
    }
    public void UseImperials()
    {
        UnitsConverter.UsePreset(UnitsConverter.imperialPreset);
    }
}
