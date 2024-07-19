using NWH.Common.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NWH.Common.Demo
{
    [RequireComponent(typeof(Text))]
    public class DemoVehicleNameDisplay : MonoBehaviour
    {
        private Text vehicleText;

        private void Awake()
        {
            vehicleText = GetComponent<Text>();
            StartCoroutine(VehicleNameCoroutine());
        }


        private IEnumerator VehicleNameCoroutine()
        {
            while (true)
            {
                Vehicle vehicle = Vehicle.ActiveVehicle;
                if (vehicle != null)
                {
                    vehicleText.text = $"{vehicle.name} [{vehicle.GetType().Name}]";
                }
                else
                {
                    vehicleText.text = "[no active vehicle]";
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
