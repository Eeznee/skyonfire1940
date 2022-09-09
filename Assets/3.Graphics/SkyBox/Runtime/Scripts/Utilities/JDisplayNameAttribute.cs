using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Jupiter
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class JDisplayName : Attribute
    {
        public string DisplayName { get; set; }

        public JDisplayName(string name)
        {
            DisplayName = name;
        }
    }
}
