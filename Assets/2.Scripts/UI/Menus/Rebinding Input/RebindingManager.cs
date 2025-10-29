using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class RebindingManager : MonoBehaviour
{
    public static RebindingManager instance;

    public Actions InputActions;

    [Header("UI references")]
    public ScrollRect mainScrollRect;
    public GameObject mainRebindingMenu;
    public AxisCustomizer axisCustomizer;
    public ConfirmOverlay confirmOverlay;
    public Text duplicateBindingsText;
    public GameObject settingsCategoriesContainer;

    [Header("Mega List Templates")]
    public AxisRebinder axisRebinderTemplate;
    public ButtonRebinder buttonRebinderTemplate;
    public Toggle actionCategoryTemplate;

    [Header("Action Buttons")]
    public Button resetBindings;
    public Button revertChanges;
    public Button bindToLeftClick;
    public Button cancelRebinding;
    public Button clearBinding;




    private List<ButtonRebinder> rebinders;

    private string saveFilePath;

    private void OnDisable()
    {
        CancelRebindingProcess();
        CloseAxisCustomizer();
        InputActions.Disable();
    }
    
    private void ResetBindingsOpenConfirm()
    {
        Action onConfirm = ResetToDefault;
        confirmOverlay.SetupAndOpen(gameObject, onConfirm, null, "Are you sure ?", "This will erase ALL custom bindings and reset them to default", "CONFIRM", "CANCEL");
    }
    private void RevertChangesOpenConfirm()
    {
        Action onConfirm = Load;
        confirmOverlay.SetupAndOpen(gameObject, onConfirm, null, "Are you sure ?", "This will revert all keybinds recently altered (highlighted in yellow)", "CONFIRM", "CANCEL");
    }
    private void BindLeftClick()
    {
        if (ButtonRebinder.rebindingOperation == null) return;
        InputAction action = ButtonRebinder.rebindingOperation.action;
        CancelRebindingProcess();
        action.ApplyBindingOverride(ButtonRebinder.currentlyRebindedId, "<Mouse>/leftButton");
        UpdateWholeRebindInterface();

    }
    private void CancelRebindingProcess()
    {
        if (ButtonRebinder.rebindingOperation == null) return;
        ButtonRebinder.rebindingOperation.Cancel();
    }
    private void ClearCurrentBinding()
    {
        if (ButtonRebinder.rebindingOperation == null) return;
        InputAction action = ButtonRebinder.rebindingOperation.action;
        CancelRebindingProcess();
        action.ApplyBindingOverride(ButtonRebinder.currentlyRebindedId, "");
        UpdateWholeRebindInterface();
    }
    private void SetActiveActionLinkedToMap(bool active, InputActionMap map)
    {
        if (ButtonRebinder.rebindingOperation != null) return;
        foreach (ButtonRebinder individualRebinder in rebinders)
        {
            if (individualRebinder.linkedActionMap == map) individualRebinder.gameObject.SetActive(active);
        }
    }
    private void Awake()
    {
        InputActions = new Actions();
        InputActions.Disable();

        instance = this;
        ButtonRebinder.rebindingOperation = null;
        rebinders = new List<ButtonRebinder>();

        saveFilePath = Path.Combine(Application.persistentDataPath, "sofbindings.json");


        if (File.Exists(saveFilePath))
            Load();
        else
        {
            InputActions.RemoveAllBindingOverrides();
            Save();
        }
        resetBindings.onClick.AddListener(ResetBindingsOpenConfirm);
        revertChanges.onClick.AddListener(RevertChangesOpenConfirm);
        bindToLeftClick.onClick.AddListener(BindLeftClick);
        clearBinding.onClick.AddListener(ClearCurrentBinding);
        cancelRebinding.onClick.AddListener(CancelRebindingProcess);


        foreach (InputActionMap map in InputActions.asset.actionMaps)
        {
            if (map.name == "ActionWheel") continue;

            InputActionMap mapTemp = map;

            Toggle actionMapToggle = Instantiate(actionCategoryTemplate,actionCategoryTemplate.transform.parent);

            Text[] texts = actionMapToggle.GetComponentsInChildren<Text>(true);
            foreach(Text t in texts) t.text = InputSystemUtil.DisplayCamelCaseString(map.name).ToUpper();

            actionMapToggle.isOn = false;
            actionMapToggle.onValueChanged.AddListener(isOn => SetActiveActionLinkedToMap(isOn, mapTemp));

            map.Enable();

            foreach (InputAction action in map.actions)
            {
                if (action.name == "ScrollWheel") continue;

                if (action.type == InputActionType.Button)
                    rebinders.Add(Instantiate(buttonRebinderTemplate, mainScrollRect.content));
                else if (action.type == InputActionType.Value)
                    rebinders.Add(Instantiate(axisRebinderTemplate, mainScrollRect.content));

                rebinders.Last().LinkAction(action);
            }

            SetActiveActionLinkedToMap(false, map);
        }

        UpdateWholeRebindInterface();
        SaveCurrentPathsAsOriginals();
        CloseAxisCustomizer();

        Destroy(actionCategoryTemplate.gameObject);
    }
    public void OpenAndLoadAxisCustomizer(InputAction action, int bindingId)
    {
        axisCustomizer.LoadBindingAndEnableAxisCustomizer(action, bindingId);
        mainRebindingMenu.gameObject.SetActive(false);
        settingsCategoriesContainer.gameObject.SetActive(false);
    }
    public void CloseAxisCustomizer()
    {
        axisCustomizer.gameObject.SetActive(false);
        mainRebindingMenu.gameObject.SetActive(true);
        settingsCategoriesContainer.gameObject.SetActive(true);
    }

    public void Save()
    {
        if (InputActions == null) return;

        string bindings = InputActions.SaveBindingOverridesAsJson();
        if(!string.IsNullOrEmpty(bindings)) File.WriteAllText(saveFilePath, bindings);
        UpdateWholeRebindInterface();
        SaveCurrentPathsAsOriginals();

        UpdateWholeRebindInterface();

        SofSettingsSO.ApplyAndUpdateSettings();
    }
    public void Load()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            if(!string.IsNullOrEmpty(json)) InputActions.LoadBindingOverridesFromJson(json);

            UpdateWholeRebindInterface();
            SaveCurrentPathsAsOriginals();
        }
    }
    private bool JsonIsUpdated()
    {
        string current = InputActions.SaveBindingOverridesAsJson();
        string saved = File.Exists(saveFilePath) ? File.ReadAllText(saveFilePath) : "";

        return current.Equals(saved);
    }
    public void ResetToDefault()
    {
        InputActions.RemoveAllBindingOverrides();
        UpdateWholeRebindInterface();
    }
    public void UpdateWholeRebindInterface()
    {
        bool rebindingOperationActive = ButtonRebinder.rebindingOperation != null;
        bool anyBindingChanged = false;
        foreach (ButtonRebinder rebinder in rebinders)
        {
            rebinder.UpdateRebinderInterface();
            if (rebinder.AnyBindingChanged()) anyBindingChanged = true;
        }

        bindToLeftClick.interactable = cancelRebinding.interactable = clearBinding.interactable = rebindingOperationActive;

        resetBindings.interactable = !rebindingOperationActive;
        revertChanges.interactable = !rebindingOperationActive && anyBindingChanged;

        mainScrollRect.vertical = !rebindingOperationActive;

        UpdateDuplicateBindingView();
    }
    public void SaveCurrentPathsAsOriginals()
    {
        foreach (ButtonRebinder rebinder in rebinders) rebinder.SaveCurrentPathAsOriginal();
    }
    public void UpdateDuplicateBindingView()
    {
        string[] identicalBindings = AllIdenticalBindings(InputActions.asset);

        string txt = "";
        foreach (string identicalB in identicalBindings)
        {
            txt += InputSystemUtil.DisplayCamelCaseString(identicalB.Split(">/")[^1]);
            txt += "\n";
        }

        if (txt == "") txt = "No duplicate input was found.";

        duplicateBindingsText.text = txt;
    }
    public static string[] AllIdenticalBindings(InputActionAsset inputActionAsset)
    {
        Dictionary<string, List<InputAction>> actionsPerBinding = ActionsPerBinding(inputActionAsset);
        List<string> stringList = new List<string>();

        foreach (var entry in actionsPerBinding)
        {
            string txt = entry.Key + " : ";
            InputActionMap firstSeatSpecificMap = null;
            int count = 0;

            foreach (InputAction action in entry.Value.ToArray())
            {
                string mapName = action.actionMap.name;
                bool seatSpecific = mapName == "Pilot" || mapName == "Gunner" || mapName == "Bomber";

                if (seatSpecific && firstSeatSpecificMap == null) firstSeatSpecificMap = action.actionMap;

                if (!seatSpecific || firstSeatSpecificMap == action.actionMap)
                {
                    txt += action.actionMap.name + "/" + action.name + ", ";
                    count++;
                }
            }
            if (count > 1) stringList.Add(txt);
        }
        return stringList.ToArray();
    }
    public static Dictionary<string, List<InputAction>> ActionsPerBinding(InputActionAsset inputActionAsset)
    {
        Dictionary<string, List<InputAction>> actionsPerBinding = new Dictionary<string, List<InputAction>>();

        foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
        {
            if (actionMap.name == "Camera") continue;
            if (actionMap.name == "ActionWheel") continue;

            foreach (InputAction action in actionMap.actions)
            {
                foreach (InputBinding binding in action.bindings)
                {
                    if (binding.isComposite) continue;

                    string path = binding.effectivePath;
                    if (string.IsNullOrEmpty(path)) continue;

                    string fullActionName = $"{actionMap.name}/{action.name}";

                    if (!actionsPerBinding.ContainsKey(path))
                        actionsPerBinding.Add(path, new List<InputAction> { action });
                    else
                        actionsPerBinding[path].Add(action);
                }
            }
        }

        return actionsPerBinding;
    }
}
