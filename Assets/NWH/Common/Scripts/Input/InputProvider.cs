using System;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.Common.Input
{
    /// <summary>
    ///     Base class from which all input providers inherit.
    /// </summary>
    public abstract class InputProvider : MonoBehaviour
    {
        /// <summary>
        ///     List of all InputProviders in the scene.
        /// </summary>
        public static List<InputProvider> Instances = new List<InputProvider>();

        protected static int InstanceCount;
            

        public virtual void Awake()
        {
            Instances.Add(this);
            InstanceCount++;
        }
        
        
        public virtual void OnDestroy()
        {
            // Reset instances list on scene exit.
            Instances.Remove(this);
            InstanceCount--;
        }


        /// <summary>
        ///     Returns combined input of all InputProviders present in the scene.
        ///     Result will be a sum of all inputs of the selected type.
        ///     T is a type of InputProvider that the input will be retrieved from.
        /// </summary>
        public static int CombinedInput<T>(Func<T, int> selector) where T : InputProvider
        {
            int sum = 0;
            for (int i = 0; i < InstanceCount; i++)
            {
                InputProvider ip = Instances[i];
                if (ip is T provider)
                {
                    sum += selector(provider);
                }
            }

            return sum;
        }


        /// <summary>
        ///     Returns combined input of all InputProviders present in the scene.
        ///     Result will be a sum of all inputs of the selected type.
        ///     T is a type of InputProvider that the input will be retrieved from.
        /// </summary>
        public static float CombinedInput<T>(Func<T, float> selector) where T : InputProvider
        {
            float sum = 0;
            for (int i = 0; i < InstanceCount; i++)
            {
                InputProvider ip = Instances[i];
                if (ip is T provider)
                {
                    sum += selector(provider);
                }
            }

            return sum;
        }


        /// <summary>
        ///     Returns combined input of all InputProviders present in the scene.
        ///     Result will be positive if any InputProvider has the selected input set to true.
        ///     T is a type of InputProvider that the input will be retrieved from.
        /// </summary>
        public static bool CombinedInput<T>(Func<T, bool> selector) where T : InputProvider
        {
            for (int i = 0; i < InstanceCount; i++)
            {
                InputProvider ip = Instances[i];
                if (ip is T provider && selector(provider))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        ///     Returns combined input of all InputProviders present in the scene.
        ///     Result will be a sum of all inputs of the selected type.
        ///     T is a type of InputProvider that the input will be retrieved from.
        /// </summary>
        public static Vector2 CombinedInput<T>(Func<T, Vector2> selector) where T : InputProvider
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < InstanceCount; i++)
            {
                InputProvider ip = Instances[i];
                if (ip is T provider)
                {
                    sum += selector(provider);
                }
            }

            return sum;
        }
    }
}