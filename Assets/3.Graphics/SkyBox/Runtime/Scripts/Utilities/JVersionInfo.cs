using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    /// <summary>
    /// Utility class contains product info
    /// </summary>
    public static class JVersionInfo
    {
        public static string Code
        {
            get
            {
                return "1.2.7";
            }
        }

        public static string ProductName
        {
            get
            {
                return "Jupiter - Procedural Sky";
            }
        }

        public static string ProductNameAndVersion
        {
            get
            {
                return string.Format("{0} v{1}", ProductName, Code);
            }
        }

        public static string ProductNameShort
        {
            get
            {
                return "Jupiter";
            }
        }

        public static string ProductNameAndVersionShort
        {
            get
            {
                return string.Format("{0} v{1}", ProductNameShort, Code);
            }
        }
    }
}
