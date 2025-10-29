using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModsDropdown : MonoBehaviour
{
    public StationDropdown dropdownRef;
    public AircraftsDropdown linkedAircraftsDropdown;

    private StationDropdown[] dropdowns;
    private AircraftCard selectedCard;

    private Station[] Stations()
    {
        return selectedCard.sofAircraft.Stations;
    }
    public int[] SelectedMods
    {
        get
        {
            if (dropdowns == null || dropdowns.Length != Stations().Length) ResetAndUpdateStations();

            int[] selection = new int[dropdowns.Length];
            for (int i = 0; i < selection.Length; i++) selection[i] = dropdowns[i].SelectedMod;
            return selection;
        }
    }
    private void Start()
    {
        linkedAircraftsDropdown.OnAircraftChange += ResetAndUpdateStations;
        dropdownRef.gameObject.SetActive(false);

        ResetAndUpdateStations();
    }
    public void LinkNewAircraftDropdown(AircraftsDropdown newAircraftsDropdown)
    {
        linkedAircraftsDropdown = newAircraftsDropdown;
        ResetAndUpdateStations();
    }
    public void ResetAndSelect(int[] selection)
    {
        ResetAndUpdateStations();
        Select(selection);
    }
    public void ResetAndUpdateStations()
    {
        selectedCard = linkedAircraftsDropdown.SelectedCard;

        if(dropdowns != null) foreach (StationDropdown d in dropdowns) DestroyImmediate(d.gameObject);

        dropdowns = new StationDropdown[Stations().Length];

        for (int i = 0; i < Stations().Length; i++)
        {
            dropdowns[i] = Instantiate(dropdownRef, dropdownRef.transform.parent);
            dropdowns[i].Reset(Stations()[i], 0);
        }
    }
    public void Select(int[] selection)
    {
        if (selection.Length < Stations().Length) return;

        for (int i = 0; i < Stations().Length; i++)
        {
            dropdowns[i].Reset(Stations()[i], selection[i]);
        }
    }
}
