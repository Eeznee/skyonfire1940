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
        public string name;
        private UnitOption baseUnit;
        private UnitOption[] options;
        private int currentOption = 0;

        public string Symbol { get { return options[currentOption].symbol; } }
        public float Multiplier { get { return options[currentOption].multiplier / baseUnit.multiplier; } }

        public Usage(string n, UnitOption _baseUnit, UnitOption[] os)
        {
            name = n;
            baseUnit = _baseUnit;
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
    static readonly UnitOption nmiLength = new UnitOption("nautical mile", "nmi", 0.000539957f);
    static readonly UnitOption miLength = new UnitOption("mile", "mi", 0.00062137f);


    //Velocity
    static readonly UnitOption kphVelocity = new UnitOption("kilometers per hour", "km/h", 3.6f);
    static readonly UnitOption fpmVelocity = new UnitOption("feets per minute", "ft/m", 196.85f);
    static readonly UnitOption mphVelocity = new UnitOption("miles per hour", "mi/h", 2.236936f);
    static readonly UnitOption knVelocity = new UnitOption("knot", "kn", 1.943844f);
    static readonly UnitOption mpsVelocity = new UnitOption("meters per second", "m/s", 1f); // SI unit

    //Mass
    static readonly UnitOption gMass = new UnitOption("gram", "g", 1000f);
    static readonly UnitOption lbMass = new UnitOption("pound", "lb", 2.20462f);
    static readonly UnitOption kgMass = new UnitOption("kilogram", "kg", 1f); // SI unit
    static readonly UnitOption tMass = new UnitOption("ton", "t", 0.001f);

    //Usages
    public static readonly Usage altitude = new Usage("Altitude", mLength, new UnitOption[] { mLength, ftLength });
    public static readonly Usage distance = new Usage("Distance", mLength, new UnitOption[] { kmLength, miLength, nmiLength });
    public static readonly Usage speed = new Usage("Speed", mpsVelocity, new UnitOption[] { kphVelocity , mphVelocity, knVelocity });
    public static readonly Usage climbRate = new Usage("Climb Rate", mpsVelocity,new UnitOption[] { mpsVelocity, fpmVelocity });
    public static readonly Usage mass = new Usage("Mass", kgMass, new UnitOption[] { kgMass, lbMass });
    public static readonly Usage ammunitionSizes = new Usage("Ammunition Sizes", mmLength, new UnitOption[] { mmLength, inLength });
}
