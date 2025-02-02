using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModsDropdown : MonoBehaviour
{
    public AircraftsDropdown aircraftsDropdown;
    public Text label;
    private StationDropdown dropdownRef;
    private StationDropdown[] dropdowns;

    private Station[] Stations()
    {
        return aircraftsDropdown.SelectedCard.sofAircraft.Stations;
    }
    public int[] SelectedMods
    {
        get
        {
            if (dropdowns == null || dropdowns.Length != Stations().Length) ResetDefault();
            int[] selection = new int[dropdowns.Length];
            for (int i = 0; i < selection.Length; i++) selection[i] = dropdowns[i].SelectedMod;
            return selection;
        }
    }
    private void Start()
    {
        if (dropdownRef) return;
        dropdownRef = GetComponentInChildren<StationDropdown>();
        dropdownRef.gameObject.SetActive(false);

        aircraftsDropdown.OnAircraftChange += ResetDefault;
        ResetDefault();
    }

    public void ResetDefault()
    {
        int[] values = new int[Stations().Length];
        for (int i = 0; i < values.Length; i++) values[i] = 0;
        Reset(values);
    }
    public void Reset(int[] selection)
    {
        if (dropdownRef == null) Start();
        if (dropdowns != null) foreach (StationDropdown d in dropdowns) DestroyImmediate(d.gameObject);

        dropdowns = new StationDropdown[Stations().Length];

        for (int i = 0; i < Stations().Length; i++)
        {
            dropdowns[i] = Instantiate(dropdownRef, dropdownRef.transform.parent);
            dropdowns[i].Reset(Stations()[i], selection[i]);
        }
        label.text = aircraftsDropdown.SelectedCard.completeName + " Modifications (This Squadron)";
    }
}
