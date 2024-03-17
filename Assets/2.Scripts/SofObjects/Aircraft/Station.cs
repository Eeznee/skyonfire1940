using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Station
{
    public string name = "Bombs Load";
    public int picked;
    public int priority;
    public bool symmetrical;
    public Transform[] options;
    public Transform[] symmetricalOptions;



    public Transform Current()
    {
        return options[picked];
    }
    public string OptionName()
    {
        return OptionName(picked);
    }
    public string OptionName(int p)
    {
        return options[p] == null ? "Empty" : options[p].name;
    }
    public void UpdateOptions()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null) continue;
            options[i].gameObject.SetActive(i == picked);
            if (symmetrical && i < symmetricalOptions.Length && symmetricalOptions[i] != null)
                symmetricalOptions[i].gameObject.SetActive(i == picked);
        }
    }
    public void ChooseOption()
    {
        ChooseOption(picked);
    }
    public void ChooseOption(int p)
    {
        picked = p;
        SetupPickedOption();
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null || i == picked) continue;
            if (symmetrical) GameObject.DestroyImmediate(symmetricalOptions[i].gameObject);
            GameObject.DestroyImmediate(options[i].gameObject);
        }
    }
    private void SetupPickedOption()
    {
        Transform option = options[picked];
        if (option == null) return;

        option.gameObject.SetActive(true);
        if (symmetrical) symmetricalOptions[picked].gameObject.SetActive(true);

        OrdnanceLoad load = option.GetComponentInChildren<OrdnanceLoad>();
        if (load)
        {
            load.symmetrical = symmetrical ? symmetricalOptions[picked].GetComponentInChildren<OrdnanceLoad>() : null;
            load.priority = priority;
        }
    }
    public static T[] GetOrdnances<T>(Station[] stations) where T : OrdnanceLoad
    {
        List<T> ordnances = new List<T>();
        foreach (Station s in stations) {
            if (s.Current() == null) continue;
            T tryOrdnance = s.Current().GetComponentInChildren<T>();
            if (tryOrdnance) ordnances.Add(tryOrdnance);
        }
        return ordnances.ToArray();
    }
}