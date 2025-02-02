using UnityEngine;

public static class UnitsConverter
{
    public const float mpsToKph = 3.6f;
    public const float kphToMps = 0.27777777777f;
    public class UnitOption
    {
        public string name;
        public string symbol;
        public float multiplier;
        public UnitOption(string n, string s, float m)
        {
            name = n;
            symbol = s;
            multiplier = m;
        }
    }

    public class Usage
    {
        //Default multiplier is option[0] , it is what I use as a developper , and generally a si unit
        public string name;
        UnitOption[] options;
        int currentOption = 0;

        public string Symbol { get { return options[currentOption].symbol; } }
        public float Multiplier { get { return options[currentOption].multiplier / options[0].multiplier; } }

        public Usage(string n, UnitOption[] os)
        {
            name = n;
            options = os;
        }

        public void ChangeOption(int desiredOption)
        {
            currentOption = desiredOption;
            PlayerPrefs.SetInt(name, currentOption);
        }
    }

    //Length
    static readonly UnitOption mmLength = new UnitOption("millimeter", "mm", 1000f);
    static readonly UnitOption inLength = new UnitOption("inch", "in", 39.37f);
    static readonly UnitOption ftLength = new UnitOption("feet", "ft", 3.2808f);
    static readonly UnitOption mLength = new UnitOption("meter", "m", 1f); // SI unit
    static readonly UnitOption kmLength = new UnitOption("kilometer", "km", 0.001f);
    static readonly UnitOption miLength = new UnitOption("mile", "mi", 0.00062137f);

    //Velocity
    static readonly UnitOption kphVelocity = new UnitOption("kilometers per hour", "km/h", 3.6f);
    static readonly UnitOption fpsVelocity = new UnitOption("feets per second", "ft/s", 3.2808f);
    static readonly UnitOption mphVelocity = new UnitOption("miles per hour", "mi/h", 2.236936f);
    static readonly UnitOption knVelocity = new UnitOption("knot", "kn", 1.943844f);
    static readonly UnitOption mpsVelocity = new UnitOption("meters per second", "m/s", 1f); // SI unit

    //Mass
    static readonly UnitOption gMass = new UnitOption("gram", "g", 1000f);
    static readonly UnitOption lbMass = new UnitOption("pound", "lb", 2.20462f);
    static readonly UnitOption kgMass = new UnitOption("kilogram", "kg", 1f); // SI unit
    static readonly UnitOption tMass = new UnitOption("ton", "t", 0.001f);

    //Usages
    public static readonly Usage altitude = new Usage("Altitude", new UnitOption[] { mLength, ftLength });
    public static readonly Usage distance = new Usage("Distance", new UnitOption[] { mLength, kmLength, miLength, ftLength });
    public static readonly Usage speed = new Usage("Speed", new UnitOption[] { mpsVelocity, kphVelocity , mphVelocity, fpsVelocity, knVelocity });
    public static readonly Usage climbRate = new Usage("Climb Rate", new UnitOption[] { mpsVelocity, fpsVelocity });
    public static readonly Usage mass = new Usage("Mass", new UnitOption[] { kgMass, lbMass });
    public static readonly Usage ammunitionSizes = new Usage("Ammunition Sizes", new UnitOption[] { mmLength, inLength });

    //Presets
    public static readonly int[] metricPreset = new int[] { 0, 1, 0, 1, 0, 0 };
    public static readonly int[] imperialPreset = new int[] { 1, 2, 1, 2, 1, 1 };

    public static readonly Usage[] allUsages = new Usage[] { altitude, distance, ammunitionSizes, speed, climbRate , mass };
    
    public static void Initialize()
    {
        //If first time , use metric
        if (PlayerPrefs.GetInt("Altitude",100) == 100)
        {
            UsePreset(metricPreset);
        }
        foreach (Usage u in allUsages)
        {
            u.ChangeOption(PlayerPrefs.GetInt(u.name, 0));
        }
    }

    public static void UsePreset(int[] preset)
    {
        for (int i = 0; i < allUsages.Length; i++)
        {
            allUsages[i].ChangeOption(preset[i]);
        }
    }
}
