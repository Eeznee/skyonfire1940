using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadronPositionner : MonoBehaviour
{
    [HideInInspector] public MissionCreator missionCreator;
    [HideInInspector] public SquadronOnMap currentSquadron;

    public Slider headingSlider;
    public Slider altitudeSlider;
    public Slider difficultySlider;

    private RectTransform rect;
    private RectTransform parentRect;

    private Vector3 pos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = rect.parent.parent.GetComponent<RectTransform>();
        if(currentSquadron == null) gameObject.SetActive(false);
    }
    public void EditSquad(SquadronOnMap squad)
    {
        currentSquadron = squad;
        headingSlider.value = squad.squad.startHeading;
        altitudeSlider.value = squad.squad.startPosition.y;
        difficultySlider.value = squad.squad.difficulty;

        gameObject.SetActive(true);
        transform.position = ControlsManager.uiActions.Main.Point.ReadValue<Vector2>();
        transform.SetAsLastSibling();

        pos = rect.localPosition;
    }
    public void Confirm()
    {
        missionCreator.UpdateSquadronValues(headingSlider.value, altitudeSlider.value, difficultySlider.value * 0.01f, currentSquadron.squad);
        currentSquadron.StopEditing();

        currentSquadron = null;
        gameObject.SetActive(false);
    }
    public void SetAsPlayer()
    {
        //missionCreator.UpdateSquadronValues(headingSlider.value, altitudeSlider.value, difficultySlider.value * 0.01f, currentSquadron.squad);
        missionCreator.SetPlayer(currentSquadron.squad);
    }
    public void Remove()
    {
        gameObject.SetActive(false);
        missionCreator.Remove(currentSquadron.squad);
        Destroy(currentSquadron.gameObject);
        currentSquadron = null;
    }
    private void Update()
    {
        if(currentSquadron != null) currentSquadron.UpdateInterface(headingSlider.value, missionCreator.ally.isOn);
        FitWithinFrame();
    }
    private void FitWithinFrame()
    {
        rect.transform.localPosition = pos;

        Vector3[] corners = new Vector3[4];
        Vector3[] parentCorners = new Vector3[4];
        rect.GetWorldCorners(corners);
        parentRect.GetWorldCorners(parentCorners);

        if (corners[0].x < parentCorners[0].x) rect.Translate(parentCorners[0].x - corners[0].x, 0f,0f);
        if (corners[0].y < parentCorners[0].y) rect.Translate(0f, parentCorners[0].y - corners[0].y, 0f);
        if (corners[2].x > parentCorners[2].x) rect.Translate(parentCorners[2].x - corners[2].x, 0f, 0f);
        if (corners[2].y > parentCorners[2].y) rect.Translate(0f,parentCorners[2].y - corners[2].y, 0f);
    }
}
