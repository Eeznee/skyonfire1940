using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class TexturesDropdown : MonoBehaviour
{
    public AircraftsDropdown linkedAircraftsDropdown;
    public Dropdown dropdown;
    public string subFolder = "custom_picture";
    public InputField renameField;

    public Button import;
    public Button editName;
    public Button confirmName;
    public Button openFileLocation;
    public Button delete;

    public string SelectedName => dropdown.captionText.text == "Default" ? "" : dropdown.captionText.text;

    private void Awake()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        import.onClick.AddListener(PickNew);
        editName.onClick.AddListener(StartRenaming);
        confirmName.onClick.AddListener(EndRenaming);
        openFileLocation.onClick.AddListener(OpenFolderLocation);
        delete.onClick.AddListener(DeleteCurrent);

        ResetAndUpdateOptions();

        if (linkedAircraftsDropdown)
            linkedAircraftsDropdown.OnAircraftChange += ResetAndUpdateOptions;
        else
            Select(PlayerPrefs.GetString(subFolder + "LastSelected", ""));

        SetEditNameMode(false);
    }

    private void OnDropdownValueChanged(int a)
    {
        PlayerPrefs.SetString(subFolder + "LastSelected", dropdown.options[dropdown.value].text);
        if (subFolder == "custom_picture") SofSettingsSO.ApplyAndUpdateSettings();
    }
    public void LinkNewAircraftsDropdown(AircraftsDropdown newAircraftsDropdown)
    {
        linkedAircraftsDropdown = newAircraftsDropdown;
        ResetAndUpdateOptions();
    }
    public void ResetAndSelect(string selection)
    {
        ResetAndUpdateOptions();
        Select(selection);
    }
    public void ResetAndUpdateOptions()
    {
        if (linkedAircraftsDropdown) subFolder = linkedAircraftsDropdown.SelectedCard.fileName;

        dropdown.ClearOptions();
        dropdown.options.Add(new Dropdown.OptionData("Default"));
        foreach (string name in TextureTool.ListNames(subFolder))
            dropdown.options.Add(new Dropdown.OptionData(name));

        dropdown.value = 0;

        dropdown.RefreshShownValue();
    }
    public void Select(string selection)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
            if (dropdown.options[i].text == selection)
                dropdown.value = i;

        dropdown.RefreshShownValue();
    }

    private void SetEditNameMode(bool editMode)
    {
        dropdown.targetGraphic.gameObject.SetActive(!editMode);
        renameField.gameObject.SetActive(editMode);

        import.gameObject.SetActive(!editMode);
        editName.gameObject.SetActive(!editMode);
        confirmName.gameObject.SetActive(!editMode);
        openFileLocation.gameObject.SetActive(!editMode && !Extensions.IsMobile);
        delete.gameObject.SetActive(!editMode);
        confirmName.gameObject.SetActive(editMode);
    }
    private string extension;
    private void StartRenaming()
    {
        if (dropdown.value == 0) return;

        SetEditNameMode(true);

        extension = Path.GetExtension(dropdown.captionText.text);
        renameField.text = Path.GetFileNameWithoutExtension(dropdown.captionText.text);
        renameField.ActivateInputField();
    }
    private void EndRenaming()
    {
        if (dropdown.value == 0) return;

        SetEditNameMode(false);

        string newName = renameField.text + extension;
        TextureTool.Rename(dropdown.captionText.text, ref newName, subFolder);
        dropdown.options[dropdown.value].text = newName;
        ResetAndSelect(newName);
    }

    public void PickNew()
    {
        TextureTool.Pick(subFolder, (texture) =>
        {
            ResetAndUpdateOptions();
            if (texture && !string.IsNullOrEmpty(texture.name)) Select(texture.name);
        });
    }
    public void DeleteCurrent()
    {
        if (dropdown.value == 0) return;

        TextureTool.Remove(subFolder, dropdown.captionText.text);

        if (dropdown.value == dropdown.options.Count - 1) dropdown.value--;

        ResetAndSelect(dropdown.options[dropdown.value].text);
    }
    public void OpenFolderLocation()
    {
        Application.OpenURL(TextureTool.FolderPath(subFolder));
    }
}
