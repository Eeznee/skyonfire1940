using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadronPositionner : MonoBehaviour
{
    [HideInInspector] public MissionCreator missionCreator;
    [HideInInspector] public SquadronOnMap currentSquadron;
    public InputField headingInput;
    public Slider headingSlider;
    public InputField altitudeInput;
    public Slider altitudeSlider;
    public Slider difficultySlider;
    public InputField difficultyInput;
    public Button confirm;


    public void UpdateValues() { 
        currentSquadron.UpdateHeading(headingSlider.value);
    }

    public void OnEnable()
    {
        transform.position = Input.mousePosition;
        transform.SetAsLastSibling();
        confirm.interactable = true;
    }

    public void EditSquad(SquadronOnMap squad)
    {
        currentSquadron = squad;
        headingInput.text = squad.assignedSquad.startHeading.ToString();
        altitudeInput.text = squad.assignedSquad.startPosition.y.ToString();
        difficultyInput.text = squad.assignedSquad.difficulty.ToString();

        gameObject.SetActive(true);
    }

    public void Confirm()
    {
        missionCreator.Confirm(int.Parse(headingInput.text), int.Parse(altitudeInput.text),int.Parse(difficultyInput.text)/100f, currentSquadron.assignedSquad.hiddenId);
        currentSquadron.StopEdit();

        gameObject.SetActive(false);
    }
    public void Remove()
    {
        gameObject.SetActive(false);
        missionCreator.Remove(currentSquadron.assignedSquad.hiddenId);
        Destroy(currentSquadron.gameObject);
    }
}
