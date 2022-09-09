using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;

namespace Pinwheel.Jupiter
{
    internal class JCompileLogger
    {
        private const string PACKAGE_NAME = "Jupiter - Low Poly Sky And Weather System";
        private const string PACKAGE_NAME_PLACEHOLDER = "${PACKAGE_NAME}";

        private const string WEBSITE = "http://pinwheel.studio";
        private const string WEBSITE_PLACEHOLDER = "${WEBSITE}";

        private const string SUPPORT_MAIL = "support@pinwheel.studio";
        private const string SUPPORT_MAIL_PLACEHOLDER = "${SUPPORT_MAIL}";

        private const string LINK_COLOR = "blue";
        private const string LINK_COLOR_PLACEHOLDER = "${LC}";

        private const float LOG_MESSAGE_PROBABIILITY = 0.03F;
        private static string[] messages = new string[]
        {
            "Thanks for using the ${PACKAGE_NAME}, please contact <color=${LC}>${SUPPORT_MAIL}</color> for support!",
            "Thanks for using the ${PACKAGE_NAME}, please give it a 5-stars rating and a positive review on the store!"
        };

        //[DidReloadScripts]
        public static void ShowMessageOnCompileSucceeded()
        {
            ValidatePackageAndNamespace();
            if (Random.value < LOG_MESSAGE_PROBABIILITY)
            {
                if (messages.Length == 0)
                    return;
                int msgIndex = Random.Range(0, messages.Length);
                string msg = messages[msgIndex]
                    .Replace(PACKAGE_NAME_PLACEHOLDER, JVersionInfo.ProductNameAndVersionShort)
                    .Replace(WEBSITE_PLACEHOLDER, WEBSITE)
                    .Replace(SUPPORT_MAIL_PLACEHOLDER, SUPPORT_MAIL)
                    .Replace(LINK_COLOR_PLACEHOLDER, LINK_COLOR);
                Debug.Log(msg, null);
            }
        }

        private static void ValidatePackageAndNamespace()
        {
            bool isPackageNameInvalid = PACKAGE_NAME.Equals("PACKAGE_NAME");
            bool isNamespaceInvalid = typeof(JCompileLogger).Namespace.Contains("PACKAGE_NAME");
            if (isPackageNameInvalid)
            {
                string message = "<color=red>Invalid PACKAGE_NAME in CompileLogger, fix it before release!</color>";
                Debug.Log(message);
            }
            if (isNamespaceInvalid)
            {
                string message = "<color=red>Invalid NAMESPACE in CompileLogger, fix it before release!</color>";
                Debug.Log(message);
            }
        }
    }
}