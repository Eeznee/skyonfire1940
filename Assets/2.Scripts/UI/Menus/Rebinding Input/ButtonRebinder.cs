using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ButtonRebinder : MonoBehaviour
{
    public Text label;
    public Button button1;
    public Button button2;

    public Image background1;
    public Image background2;

    private Color defaultBackgroundColor;
    public Color changedBindingBackgroundColor;
    public Color currentlyChangingBindingBackgroundColor;

    public static InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    public static Button currentlyRebinded;
    public static int currentlyRebindedId;

    [HideInInspector] public InputAction linkedAction;
    [HideInInspector] public InputActionMap linkedActionMap;

    protected Text text1;
    protected Text text2;
    protected int bindingID0;
    protected int bindingID1;
    protected string originalPath0;
    protected string originalPath2;

    public const float waitForOther = 0.3f;

    public virtual void Awake()
    {
        text1 = button1.GetComponentInChildren<Text>();
        text2 = button2.GetComponentInChildren<Text>();

        button1.onClick.AddListener(delegate { StartRebindingProcess(button1, bindingID0); });
        button2.onClick.AddListener(delegate { StartRebindingProcess(button2, bindingID1); });

        defaultBackgroundColor = background1.color;
    }

    public void LinkAction(InputAction _linkedAction)
    {
        linkedAction = _linkedAction;
        linkedActionMap = linkedAction.actionMap;
        label.text = InputSystemUtil.DisplayCamelCaseString(linkedAction.name);
        name = linkedAction.name;

        SetBindingsId();
        SaveCurrentPathAsOriginal();
    }
    protected virtual void SetBindingsId()
    {
        List<InputBinding> bindings = linkedAction.bindings.ToList();

        bindingID0 = bindings.FindIndex(b => !b.effectivePath.Contains("SofDevice"));
        bindingID1 = bindings.FindLastIndex(b => !b.effectivePath.Contains("SofDevice"));

        if (bindingID0 == bindingID1) Debug.LogError(linkedAction.name + " has no empty binding available, add one to fix");
    }

    public bool BindingChanged(int id)
    {
        string originalPath = id == 0 ? originalPath0 : originalPath2;
        string currentPath = FullBindingPath(id == 0 ? bindingID0 : bindingID1);

        if (originalPath == null) originalPath = "";
        if (currentPath == null) currentPath = "";

        return originalPath != currentPath;
    }
    public bool AnyBindingChanged()
    {
        for (int i = 0; i < 2; i++)
            if (BindingChanged(i)) return true;
        return false;
    }

    public virtual void UpdateRebinderInterface()
    {
        text1.text = BindingDisplay(button1, bindingID0);
        text2.text = BindingDisplay(button2, bindingID1);

        background1.color = BindingChanged(0) ? changedBindingBackgroundColor : defaultBackgroundColor;
        background2.color = BindingChanged(1) ? changedBindingBackgroundColor : defaultBackgroundColor;
        if (currentlyRebinded == button1) background1.color = currentlyChangingBindingBackgroundColor;
        if (currentlyRebinded == button2) background2.color = currentlyChangingBindingBackgroundColor;

        text1.color = (background1.color == currentlyChangingBindingBackgroundColor) ? Color.black : Color.white;
        text2.color = (background2.color == currentlyChangingBindingBackgroundColor) ? Color.black : Color.white;
    }

    public virtual string FullBindingPath(int bindingId)
    {
        return bindingId == -1 ? "" : linkedAction.bindings[bindingId].effectivePath;
    }
    public void SaveCurrentPathAsOriginal()
    {
        originalPath0 = FullBindingPath(bindingID0);
        originalPath2 = FullBindingPath(bindingID1);
    }
    protected virtual string BindingDisplay(Button button, int bindingId)
    {
        if (bindingId == -1) return "";
        if (currentlyRebinded == button) return "Rebinding...";

        return PathToReadable(linkedAction.bindings[bindingId].effectivePath);
    }
    protected string PathToReadable(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        if (path[0] == '/') path = path.Remove(0, 1);

        if (path.Contains("eyboard") || path.Contains("ouse") || path.Contains("amepad"))
        {
            string[] splitPath = path.Split('/');

            string keyName = path.Replace(splitPath[0] + "/", "");
            return InputSystemUtil.DisplayCamelCaseString(keyName);
        }
        return path;
    }
    protected virtual void StartRebindingProcess(Button button, int rebindId)
    {
        if (rebindingOperation != null) return;

        InputBinding binding = linkedAction.bindings[rebindId];
        currentlyRebinded = button;
        currentlyRebindedId = rebindId;

        linkedAction.actionMap.Disable();

        button.interactable = false;

        rebindingOperation = linkedAction.PerformInteractiveRebinding(rebindId)
        .WithControlsExcluding("<Mouse>/leftButton").WithControlsExcluding("<Pointer>/press")
        .WithoutGeneralizingPathOfSelectedControl().OnMatchWaitForAnother(waitForOther)
        .OnComplete(operation => OnRebindCompleted(button, rebindId))
        .OnCancel(operation => EndRebindOperation(button, rebindId));

        rebindingOperation.Start();
        RebindingManager.instance.UpdateWholeRebindInterface();
    }
    protected virtual void OnRebindCompleted(Button button, int rebindId)
    {
        EndRebindOperation(button, rebindId);
    }
    protected void EndRebindOperation(Button button, int rebindId)
    {
        rebindingOperation.Dispose();
        rebindingOperation = null;
        currentlyRebinded = null;

        linkedAction.actionMap.Enable();

        button.interactable = true;

        RebindingManager.instance.UpdateWholeRebindInterface();
    }


    protected readonly Color buttonColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    protected readonly Color buttonColorNewBinding = new Color(0.8f, 0.6f, 0f, 1f);
}
