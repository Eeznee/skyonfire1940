using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class GunButtonHandler : MonoBehaviour
    {
        public Toggle primaries;
        public Toggle secondaries;
        public string primariesName;
        public string secondariesName;

        public void SetDownState()
        {
           if (primaries.isOn) CrossPlatformInputManager.SetButtonDown(primariesName);
           if (secondaries.isOn) CrossPlatformInputManager.SetButtonDown(secondariesName);
        }
        public void SetUpState()
        {
            CrossPlatformInputManager.SetButtonUp(primariesName);
            CrossPlatformInputManager.SetButtonUp(secondariesName);
        }
    }
}
