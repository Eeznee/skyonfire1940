using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class AxisRebinder : ButtonRebinder
{
    public Button axisCustomizerButton;

    public override void Awake()
    {
        base.Awake();
        axisCustomizerButton.onClick.AddListener(delegate { RebindingManager.instance.OpenAndLoadAxisCustomizer(linkedAction, bindingID1); });
    }
    public override string FullBindingPath(int bindingId)
    {
        if (bindingId == -1) return "";

        if (linkedAction.bindings[bindingId].isComposite)
        {
            string path = "";
            for (int i = bindingId + 1; i < linkedAction.bindings.Count; i++)
            {
                if (linkedAction.bindings[i].isPartOfComposite) path += linkedAction.bindings[i].effectivePath;
                else return path;
            }
            return path;
        }
        else return base.FullBindingPath(bindingId);
    }

    public delegate bool InputBindingDelegate(InputBinding inputBindingInput);
    protected override void SetBindingsId()
    {
        List<InputBinding> bindings = linkedAction.bindings.ToList();

        bindingID0 = bindings.FindIndex(b => ValidCompositeBinding(bindings,b));
        bindingID1 = bindings.FindIndex(b => !b.effectivePath.Contains("SofDevice") && !b.isPartOfComposite && !b.isComposite);

        if (bindingID0 == -1 || bindingID1 == -1) Debug.LogError(linkedAction.name + " has missing bindings, make sure there is one axis and one composite");
    }

    private bool ValidCompositeBinding(List<InputBinding> bindings, InputBinding binding)
    {
        if (!binding.isComposite) return false;

        int index = bindings.IndexOf(binding) + 1;

        while (index < bindings.Count)
        {
            if (!bindings[index].isPartOfComposite) return true;
            if (bindings[index].effectivePath.Contains("SofDevice")) return false;

            index++;
        }
        return true;
    }

    protected override void StartRebindingProcess(Button button, int rebindId)
    {
        if (linkedAction.bindings[rebindId].isComposite) rebindId++;
        base.StartRebindingProcess(button, rebindId);
    }
    protected override void OnRebindCompleted(Button button, int rebindId)
    {
        base.OnRebindCompleted(button, rebindId);

        if (linkedAction.bindings[rebindId].isPartOfComposite)
        {
            if (rebindId + 1 < linkedAction.bindings.Count && linkedAction.bindings[rebindId + 1].isPartOfComposite)
            {
                StartRebindingProcess(button, rebindId + 1);
                return;
            }
        }
    }
    public override void UpdateRebinderInterface()
    {
        base.UpdateRebinderInterface();

        axisCustomizerButton.interactable = !string.IsNullOrEmpty(linkedAction.bindings[bindingID1].effectivePath);
    }
    protected override string BindingDisplay(Button button, int bindingId)
    {
        if (linkedAction.bindings[bindingId].effectivePath == null) return "";

        if (!linkedAction.bindings[bindingId].isComposite) return base.BindingDisplay(button, bindingId);


        string txt = "";

        bool currentlyRebinding = currentlyRebinded == button;

        if (!currentlyRebinding && string.IsNullOrEmpty(linkedAction.bindings[bindingId + 1].effectivePath)) return "Composite Not Set";

        for (int i = bindingId + 1; i < linkedAction.bindings.Count; i++)
        {
            if (linkedAction.bindings[i].isPartOfComposite)
            {
                if (i != bindingId + 1) txt += " | ";
                txt += InputSystemUtil.DisplayCamelCaseString(linkedAction.bindings[i].name) + ":";

                if (currentlyRebinding && i == currentlyRebindedId)
                {
                    txt += "...";
                    break;
                }
                else
                    txt += PathToReadable(linkedAction.bindings[i].effectivePath);
            }
            else break;
        }
        return txt;
    }
}