using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class TexturesDropdown : MonoBehaviour
{
    private Dropdown dropdown;
    public AircraftsDropdown aircraftsDropdown;
    public string subFolder = "custom_picture";
    public string defaultOption = "Default";
    public bool resetToLastSelected = true;
    private string LastSelected()
    {
        return PlayerPrefs.GetString(subFolder + "LastSelected", defaultOption);
    }
    public string SelectedName { get { return !dropdown || dropdown.captionText.text == defaultOption ? "" : dropdown.captionText.text; } }
    private void Start()
    {
        if (dropdown) return;
        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { OnSelect(); });

        Reset(resetToLastSelected ? LastSelected() : defaultOption);
    }
    private void Update()
    {
        if (aircraftsDropdown && subFolder != aircraftsDropdown.SelectedCard.fileName)
        {
            subFolder = aircraftsDropdown.SelectedCard.fileName;
            Reset(resetToLastSelected ? LastSelected() : defaultOption);
        }
    }
    public void Reset(string selection)
    {
        Update();
        if (!dropdown) Start();
        dropdown.ClearOptions();
        dropdown.options.Add(new Dropdown.OptionData(defaultOption));
        foreach (string name in TextureTool.ListNames(subFolder))
            dropdown.options.Add(new Dropdown.OptionData(name));

        dropdown.value = 0;
        for (int i = 0; i < dropdown.options.Count; i++)
            if (dropdown.options[i].text == selection)
                dropdown.value = i;

        dropdown.RefreshShownValue();
        OnSelect();
    }
    private void OnSelect()
    {
        PlayerPrefs.SetString(subFolder + "LastSelected", dropdown.captionText.text);
    }
    public void PickNew()
    {
        TextureTool.Pick(subFolder, this);
    }
    public void DeleteCurrent()
    {
        if (dropdown.value == 0) return;

        TextureTool.Remove(subFolder, dropdown.captionText.text);

        if (dropdown.value == dropdown.options.Count - 1) dropdown.value--;
        else dropdown.value++;
        Reset(dropdown.options[dropdown.value].text);
    }
    public void OpenFolderLocation()
    {
        Application.OpenURL(TextureTool.FolderPath(subFolder));
        Reset(LastSelected());
    }
}
