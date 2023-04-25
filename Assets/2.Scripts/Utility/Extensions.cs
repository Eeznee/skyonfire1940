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
        return mono.GetComponent<T>() != null ? mono.GetComponent<T>() : mono.gameObject.AddComponent<T>();
    }
}
