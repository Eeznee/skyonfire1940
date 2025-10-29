using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Dropdown))]
public class AircraftsDropdown : MonoBehaviour
{
    public Dropdown dropdown;
    public Action OnAircraftChange;

    private int previousDropdownValue = 0;


    public AircraftCard SelectedCard { get { return StaticReferences.Instance.defaultAircrafts.list[dropdown.value]; } }
    void Awake()
    {
        previousDropdownValue = 0;

        dropdown.onValueChanged.AddListener(delegate 
        {
            if (!StaticReferences.Instance.defaultAircrafts.list[dropdown.value].Available()) dropdown.value = previousDropdownValue;
            OnAircraftChange?.Invoke();
            previousDropdownValue = dropdown.value;
        }
        );

        ResetAndSelect(dropdown.value);
    }
    public void ResetToDefault()
    {
        dropdown.ClearOptions();
        foreach (AircraftCard card in StaticReferences.Instance.defaultAircrafts.list)
        {
            string name = (card.Available() ? "" : "(LOCKED) ") + card.completeName;
            Dropdown.OptionData option = new Dropdown.OptionData(name, card.icon);
            dropdown.options.Add(option);
        }
    }
    public void SelectAircraft(int id)
    {
        if (!StaticReferences.Instance.defaultAircrafts.list[id].Available()) return;

        dropdown.value = id;
        dropdown.RefreshShownValue();
    }
    public void SelectAircraft(AircraftCard card)
    {
        for (int i = 0; i < StaticReferences.Instance.defaultAircrafts.list.Length; i++)
        {
            if (card == StaticReferences.Instance.defaultAircrafts.list[i])
            {
                SelectAircraft(i);
                return;
            }
        }
    }
    public void ResetAndSelect(int id)
    {
        ResetToDefault();
        SelectAircraft(id);
    }
}
