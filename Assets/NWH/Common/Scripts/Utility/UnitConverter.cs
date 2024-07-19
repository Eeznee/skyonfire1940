using UnityEngine;

namespace NWH.Common.Utility
{
    public static class UnitConverter
    {
        public static float Inch_To_Meter(float inch)
        {
            return inch * 0.0254f;
        }

        public static float Meter_To_Inch(float meters)
        {
            return meters * 39.3701f;
        }

        /// <summary>
        ///     km/l to l/100km
        /// </summary>
        public static float KmlToL100km(float kml)
        {
            return kml == 0 ? Mathf.Infinity : 100f / kml;
        }


        /// <summary>
        ///     km/l to mpg
        /// </summary>
        public static float KmlToMpg(float kml)
        {
            return kml * 2.825f;
        }


        /// <summary>
        ///     l/100km to km/l
        /// </summary>
        public static float L100kmToKml(float l100km)
        {
            return l100km == 0 ? 0 : 100f / l100km;
        }


        /// <summary>
        ///     l/100km to mpg
        /// </summary>
        public static float L100kmToMpg(float l100km)
        {
            return l100km == 0 ? 0 : 282.5f / l100km;
        }


        /// <summary>
        ///     Converts angular velocity (rad/s) to rotations per minute.
        /// </summary>
        public static float AngularVelocityToRPM(float angularVelocity)
        {
            return angularVelocity * 9.5492965855137f;
        }


        /// <summary>
        ///     Converts rotations per minute to angular velocity (rad/s).
        /// </summary>
        public static float RPMToAngularVelocity(float RPM)
        {
            return RPM * 0.10471975511966f;
        }


        /// <summary>
        ///     mpg to km/l
        /// </summary>
        public static float MpgToKml(float mpg)
        {
            return mpg * 0.354f;
        }


        /// <summary>
        ///     mpg to l/100km
        /// </summary>
        public static float MpgToL100km(float mpg)
        {
            return mpg == 0 ? Mathf.Infinity : 282.5f / mpg;
        }


        /// <summary>
        ///     miles/h to km/h
        /// </summary>
        public static float MphToKph(float value)
        {
            return value * 1.60934f;
        }


        /// <summary>
        ///     m/s to km/h
        /// </summary>
        public static float MpsToKph(float value)
        {
            return value * 3.6f;
        }


        /// <summary>
        ///     m/s to miles/h
        /// </summary>
        public static float MpsToMph(float value)
        {
            return value * 2.23694f;
        }


        public static float Speed_kmhToMph(float kmh)
        {
            return kmh * 0.621371f;
        }


        public static float Speed_kmhToMs(float kmh)
        {
            return kmh * 0.277778f;
        }


        public static float Speed_mphToKmh(float mph)
        {
            return mph * 1.60934f;
        }


        public static float Speed_mphToMs(float mph)
        {
            return mph * 0.44704f;
        }


        public static float Speed_msToKph(float ms)
        {
            return ms * 3.6f;
        }


        public static float Speed_msToMph(float ms)
        {
            return ms * 2.23694f;
        }
    }
}