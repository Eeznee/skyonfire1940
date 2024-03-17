using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Dropdown))]
public class AircraftsDropdown : MonoBehaviour
{
    private Dropdown dropdown;
    [SerializeField] private AircraftsList list;

    public Action OnAircraftChange;


    private int selected;
    public AircraftCard SelectedCard { get { return list.list[dropdown.value]; } }
    void Start()
    {
        if (dropdown) return;
        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { OnAircraftChange(); });

        OnAircraftChange += CheckForLocked;

        Reset(dropdown.value);
    }
    public void Reset(int id)
    {
        if (!dropdown) Start();
        dropdown.ClearOptions();
        foreach (AircraftCard card in list.list)
        {
            string name = (card.Available() ? "" : "(LOCKED) ") + card.completeName;
            Dropdown.OptionData option = new Dropdown.OptionData(name,card.icon);
            dropdown.options.Add(option);
        }
        dropdown.value = id;
        dropdown.RefreshShownValue();
    }
    private void CheckForLocked()
    {
        AircraftCard card = list.list[dropdown.value];
        if (!card.Available())
        {
            dropdown.value = selected;
            dropdown.RefreshShownValue();
        } else
        {
            selected = dropdown.value;
        }
    }
}
