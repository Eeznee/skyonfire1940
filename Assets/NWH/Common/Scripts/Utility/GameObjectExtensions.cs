using UnityEngine;

namespace NWH.Common.Utility
{
    public static class GameObjectExtensions
    {
        public static Bounds FindBoundsIncludeChildren(this GameObject gameObject)
        {
            Bounds bounds = new Bounds();
            foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                bounds.Encapsulate(mr.bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Equal to GetComponentInParent but with option to include inactive in search.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="includeInactive"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentInParent<T>(this Transform transform, bool includeInactive = true) where T : Component
        {
            var here = transform;
            T result = null;
            while (here && !result)
            {
                if (includeInactive || here.gameObject.activeSelf)
                {
                    result = here.GetComponent<T>();
                }
                here = here.parent;
            }
            return result;
        }

        /// <summary>
        /// Searches for a component in children and parents (and self).
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="includeInactive"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentInParentsOrChildren<T>(this Transform transform, bool includeInactive = true)
            where T : Component
        {
            T result = transform.GetComponentInParent<T>(includeInactive);
            if (result == null)
            {
                result = transform.GetComponentInChildren<T>(includeInactive);
            }

            return result;
        }
    }
}

