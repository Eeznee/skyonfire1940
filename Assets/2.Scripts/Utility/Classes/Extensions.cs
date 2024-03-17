using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
    public static void ClampCuboid(this Vector3 v3, float xMax, float yMax, float zMax)
    {
        v3.x = Mathf.Clamp(v3.x, -xMax, xMax);
        v3.y = Mathf.Clamp(v3.y, -yMax, yMax);
        v3.z = Mathf.Clamp(v3.z, -zMax, zMax);
    }
    public static void ClampCube(this Vector3 v3, float max)
    {
        v3.ClampCuboid(max, max, max);
    }
    public static void ClampRectangle(this Vector2 v2, float xMax, float yMax)
    {
        v2.x = Mathf.Clamp(v2.x, -xMax, xMax);
        v2.y = Mathf.Clamp(v2.y, -yMax, yMax);
    }
    public static void ClampSquare(this Vector2 v2, float max)
    {
        v2 = Vector2.ClampMagnitude(v2, max);
        //v2.ClampRectangle(max, max);
    }
    public static Vector3 Up(this Quaternion q)
    {
        return q * Vector3.up;
    }
    public static Vector3 Forward(this Quaternion q)
    {
        return q * Vector3.forward;
    }
    public static Vector3 Right(this Quaternion q)
    {
        return q * Vector3.right;
    }
    public static T GetCreateComponent<T>(this MonoBehaviour mono) where T : Component
    {
        T component = mono.GetComponent<T>();
        return component ? component : mono.gameObject.AddComponent<T>();
    }
    public static T GetCreateComponentInChildren<T>(this MonoBehaviour mono) where T : Component
    {
        T component = mono.GetComponentInChildren<T>();
        return component ? component : mono.gameObject.AddComponent<T>();
    }
    public static T GetComponentInActualChildren<T>(this MonoBehaviour mono) where T : Component
    {
        for (int i = 0; i < mono.transform.childCount; i++)
        {
            T component = mono.transform.GetChild(0).GetComponentInChildren<T>();
            if (component != null) return component;
        }
        return null;
    }

    public static Transform CreateChild(this Transform tr,string name)
    {
        Transform newTr = new GameObject(name).transform;
        newTr.parent = tr;
        newTr.SetPositionAndRotation(tr.position, tr.rotation);
        return newTr;
    }

    public static bool IsChildOf(this Transform tr, Transform potentialParent)
    {
        Transform nextParent = tr;
        while (nextParent != null)
        {
            if (nextParent == potentialParent) return true;
            else nextParent = nextParent.parent;
        }
        return false;
    }

    public static T[] RemoveNulls<T>(this T[] mono) where T : Component
    {
        int noneNullElements = 0;
        for (int i = 0; i < mono.Length; i++)
            if (mono[i] != null) noneNullElements++;

        T[] newMono = new T[noneNullElements];

        int counter = 0;
        for (int i = 0; i < mono.Length; i++)
            if (mono[i] != null) {
                newMono[counter] = mono[i];
                counter++;
            }
        return newMono;
    }

    static public int GetLodIndex(this LODGroup lodGroup)
    {
        LOD[] lods = lodGroup.GetLODs();
        for (int i = 0; i < lods.Length; i++)
        {
            LOD lod = lods[i];
            if (lod.renderers.Length > 0 && lod.renderers[0].isVisible)
                return i;
        }
        return -1;
    }
    public static void DestroyAllChildren(this GameObject gameobject)
    {
        foreach(Transform child in gameobject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public static Vector2 Rotate(this Vector2 v2, float angle)
    {
        Vector3 v3 = new Vector3(v2.x, v2.y, 0f);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        v3 = rotation * v3;
        return new Vector2(v3.x, v3.y);
    }
}
