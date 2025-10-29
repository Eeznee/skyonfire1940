using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ConfirmOverlay : MonoBehaviour
{
    public Text headerText;
    public Text descriptionText;
    public Text confirmText;
    public Text cancelText;

    public Button confirmButton;
    public Button cancelButton;

    private event Action OnConfirm;
    private event Action OnCancel;

    private GameObject objectToReEnable;

    private void OnEnable()
    {
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
    }
    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(Confirm);
        cancelButton.onClick.RemoveListener(Cancel);
    }

    public void SetupAndOpen(GameObject _objectToReEnable, Action _onConfirm, Action _onCancel,  string header, string description, string confirm, string cancel)
    {
        headerText.text = header;
        descriptionText.text = description;
        confirmText.text = confirm;
        cancelText.text = cancel;

        OnConfirm = _onConfirm;
        OnCancel = _onCancel;

        objectToReEnable = _objectToReEnable;
        objectToReEnable?.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Confirm()
    {
        OnConfirm?.Invoke();
        CloseWindow();
    }
    public void Cancel()
    {
        OnCancel?.Invoke();
        CloseWindow();
    }
    private void CloseWindow()
    {
        objectToReEnable?.SetActive(true);
        gameObject.SetActive(false);
    }
}
