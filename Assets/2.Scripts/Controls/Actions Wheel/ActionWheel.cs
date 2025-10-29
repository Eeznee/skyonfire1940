using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Burst;
using UnityEngine.EventSystems;


public class ActionWheel : MonoBehaviour
{
    public static ActionWheel instance;

    public Button[] buttons;
    public float[] buttonsAngle;
    public Transform root;

    public Image centerImage;
    public Text centerText;

    private Transform currentSelection;
    private int currentHover;
    private bool actionWheelOpened;

    private void Start()
    {
        instance = this;

        buttons[0].onClick.AddListener(() => OnButtonClicked(0));
        buttons[1].onClick.AddListener(() => OnButtonClicked(1));
        buttons[2].onClick.AddListener(() => OnButtonClicked(2));
        buttons[3].onClick.AddListener(() => OnButtonClicked(3));
        buttons[4].onClick.AddListener(() => OnButtonClicked(4));
        buttons[5].onClick.AddListener(() => OnButtonClicked(5));

        //ControlsManager.actionWheel.ToggleWheel.performed += _ => ToggleActionWheel();
        //ControlsManager.actionWheel.Navigate.performed += ctx => NavigateActionWheel(ctx);
        ControlsManager.actionWheel.Option1.performed += _ => buttons[0].onClick.Invoke();
        ControlsManager.actionWheel.Option2.performed += _ => buttons[1].onClick.Invoke();
        ControlsManager.actionWheel.Option3.performed += _ => buttons[2].onClick.Invoke();
        ControlsManager.actionWheel.Option4.performed += _ => buttons[3].onClick.Invoke();
        ControlsManager.actionWheel.Option5.performed += _ => buttons[4].onClick.Invoke();
        ControlsManager.actionWheel.Option6.performed += _ => buttons[5].onClick.Invoke();
        ControlsManager.actionWheel.Back.performed += _ => GoBackToPrevious();

        CloseActionWheel();

        //PlayerActions.actions.
    }
    private void OnDisable()
    {
        CloseActionWheel();
    }

    private void GoBackToPrevious()
    {
        if (!actionWheelOpened) return;
        if (currentSelection == root)
        {
            CloseActionWheel();
            return;
        }

        Transform newSelection = currentSelection.parent;

        currentHover = -1;
        currentSelection = newSelection;

        UpdateUI();
    }
    private void OnButtonClicked(int buttonId)
    {
        if (!actionWheelOpened) return;
        if (buttonId < 0 || buttonId >= currentSelection.childCount) return;

        Transform newSelection = currentSelection.GetChild(buttonId);
        if (newSelection == null) return;

        if (!WheelSelectionActive(newSelection)) return;

        currentHover = -1;
        newSelection.SendMessage("InvokeCustomAction", SendMessageOptions.DontRequireReceiver);

        if (newSelection.childCount > 0) currentSelection = newSelection;

        UpdateUI();
    }

    const float magnitudeMin = 0.7f;
    const float maxDeltaAngle = 30f;
    public void NavigateActionWheel(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (input.magnitude < magnitudeMin) return;

        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        float minDelta = 360f;
        int buttonChosen = -1;

        for (int i = 0; i < buttons.Length; i++)
        {
            float delta = Mathf.Abs(buttonsAngle[i] - angle);

            if (delta < minDelta && delta < maxDeltaAngle && buttons[i].IsInteractable())
            {
                minDelta = delta;
                buttonChosen = i;
            }

        }

        if (buttonChosen != currentHover)
        {
            currentHover = buttonChosen;
            UpdateUI();
        }
    }
    public void ToggleActionWheel()
    {
        actionWheelOpened = !actionWheelOpened;

        if (actionWheelOpened)
            OpenActionWheel();
        else
            CloseActionWheel();
    }
    public void OpenActionWheel()
    {
        actionWheelOpened = true;
        currentHover = -1;
        currentSelection = root;
        Actions.ActionWheelActions wheel = ControlsManager.actionWheel;
        ControlsManager.SetActiveAllActionsContainingBinding(false);

        UpdateUI();
    }
    public void CloseActionWheel()
    {
        actionWheelOpened = false;
        currentHover = -1;
        currentSelection = root;

        Actions.ActionWheelActions wheel = ControlsManager.actionWheel;
        ControlsManager.SetActiveAllActionsContainingBinding(true);

        UpdateUI();
    }
    public void UpdateUI()
    {
        foreach (Button button in buttons) button.gameObject.SetActive(actionWheelOpened);

        if (currentHover < 0 || currentHover >= buttons.Length) EventSystem.current?.SetSelectedGameObject(null);
        else EventSystem.current.SetSelectedGameObject(buttons[currentHover].gameObject);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            Text text = button.GetComponentInChildren<Text>();


            bool selectionIsEnabled = false;
            string txtString = "...";

            if (i >= 0 && i < currentSelection.childCount)
            {
                Transform selection = currentSelection.GetChild(i);
                selectionIsEnabled = WheelSelectionActive(selection);
                txtString = selection.name;
            }

            button.interactable = selectionIsEnabled;
            text.text = (i + 1).ToString() + ". " + txtString;
        }

        centerImage.enabled = actionWheelOpened;
        centerText.enabled = actionWheelOpened;
        centerText.text = currentSelection == root ? "Main Actions" : currentSelection.name;
    }
    public bool WheelSelectionActive(Transform tr)
    {
        ActionWheelSelection actionWheelSelection = tr.GetComponent<ActionWheelSelection>();

        if (tr.childCount == 0)
        {
            if (actionWheelSelection != null) return actionWheelSelection.WheelSelectionActive();
            else return true;
        }
        else
        {
            for (int i = 0; i < tr.childCount; i++)
                if (WheelSelectionActive(tr.GetChild(i))) return true;
        }
        return false;
    }

    public void PlayerAction(string actionName)
    {
        ControlsManager.Action(actionName);
    }
    public void GoToGunnerIfOnlyOne()
    {
        if (Player.aircraft.crew.Length == 2)
        {
            Player.SetCrew(1);
            CloseActionWheel();
        }
    }
    public void SwitchSeat()
    {
        Player.CycleSeats();
    }
    public void GoToBombardierSeat()
    {
        Player.SetSeat(Player.aircraft.bombardierSeat);
    }
    public void SwitchCrew(int crewId)
    {
        Player.SetCrew(crewId);
    }
    public void NextCrew()
    {
        Player.SetCrew(Player.crewId + 1);
    }
    public void PreviousCrew()
    {
        Player.SetCrew(Player.crewId - 1);
    }
    public void SwitchToCustomCam(int camId)
    {
        SofCamera.SwitchViewMode(-camId);
    }
    public void ToggleDynamicCam()
    {
        ControlsManager.dynamic = !ControlsManager.dynamic;
    }
    public void ToggleViewMode()
    {
        SofCamera.ToggleViewMode();
    }
    public void PausePlay()
    {
        TimeManager.SetPause(!TimeManager.paused);
    }
    public void HideUI()
    {

    }
}