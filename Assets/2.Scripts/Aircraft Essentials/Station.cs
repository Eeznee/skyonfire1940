using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Station
{
    [System.Serializable]
    public struct Loadout
    {
        public string name;
        public GameObject[] included;


        public OrdnanceLoad[] AllOrdnanceLoads()
        {
            List<OrdnanceLoad> ordnanceList = new List<OrdnanceLoad>();
            foreach (GameObject gameObject in included)
            {
                if (gameObject != null) ordnanceList.AddRange(gameObject.GetComponentsInChildren<OrdnanceLoad>());
            }
            return ordnanceList.ToArray();
        }


        public void SetActive(bool active)
        {
            foreach (GameObject gameObject in included)
            {
                if (gameObject != null) gameObject.SetActive(active);
            }
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in included)
            {
                if (gameObject != null) GameObject.DestroyImmediate(gameObject);
            }
        }


        public bool Empty
        {
            get
            {
                foreach(GameObject gameObject in included)
                {
                    if (gameObject != null) return false;
                }
                return true;
            }
        }
    }

    public string name = "Bombs Load";
    [HideInInspector]public int picked;
    [Range(0,16)]public int ordnancePriority;
    public bool pairedOrdnanceLaunch;
    public Loadout[] loadouts;


    public Loadout CurrentLoadout => loadouts[picked];
    public bool IsEmpty()
    {
        if (loadouts.Length == 0) return true;
        return CurrentLoadout.included.Length == 0 || CurrentLoadout.included[0] == null;
    }
    public string OptionName()
    {
        return OptionName(picked);
    }
    public string OptionName(int p)
    {
        return loadouts[p].name;
    }
    public void SelectAndDisactivate()
    {
        SelectAndDisactivate(picked);
    }
    public void SelectAndDisactivate(int p)
    {
        picked = p;
        for (int i = 0; i < loadouts.Length; i++)
            loadouts[i].SetActive(i == picked);
    }
    public void SelectAndDestroy()
    {
        SelectAndDestroy(picked);
    }
    public void SelectAndDestroy(int p)
    {
        picked = p;
        SetupPickedOption();
        for (int i = 0; i < loadouts.Length; i++)
        {
            if (i == picked) continue;
            loadouts[i].Destroy();
        }
    }
    private void SetupPickedOption()
    {
        Loadout loadout = loadouts[picked];
        if (loadout.Empty) return;

        loadout.SetActive(true);


        OrdnanceLoad[] allOrdnance = loadout.AllOrdnanceLoads();

        for(int i = 0; i < allOrdnance.Length; i++)
        {
            allOrdnance[i].priority = ordnancePriority;

            if (pairedOrdnanceLaunch)
                allOrdnance[i].pairedOrdnanceLaunch = allOrdnance[(i + 1) % allOrdnance.Length];
        }
    }
    /*
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
    */
}