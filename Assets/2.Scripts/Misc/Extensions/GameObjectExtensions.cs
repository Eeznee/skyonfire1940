using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{

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
    public static void DestroyAllChildren(this GameObject gameobject)
    {
        foreach(Transform child in gameobject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
