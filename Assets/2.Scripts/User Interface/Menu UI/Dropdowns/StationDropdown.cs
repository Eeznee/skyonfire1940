using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class StationDropdown : MonoBehaviour
{
    private Dropdown dropdown;
    public Text stationName;

    public int SelectedMod { get { return dropdown.value; } }

    public void Start()
    {
        if (dropdown) return;
        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { OnSelect(); });
    }
    public void Reset(Station station, int selection)
    {
        if (dropdown == null) Start();
        gameObject.SetActive(true);
        dropdown.ClearOptions();
        foreach (Transform tr in station.options)
            dropdown.options.Add(new Dropdown.OptionData(tr ? tr.name : "Empty"));
        dropdown.value = selection;
        dropdown.RefreshShownValue();
        stationName.text = station.name;
    }
    private void OnSelect()
    {

    }
}
