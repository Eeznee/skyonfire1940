using UnityEngine;
using UnityEngine.UI;

namespace NWH.Common.Demo
{
    public class DemoWelcomeMessage : MonoBehaviour
    {
        public GameObject welcomeMessageGO;
        public Button closeButton;


        private void Start()
        {
            if (!Application.isEditor)
            {
                welcomeMessageGO.SetActive(true);
            }

            closeButton.onClick.AddListener(Close);
        }


        private void Close()
        {
            welcomeMessageGO.SetActive(false);
        }
    }
}