using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShortMission : MonoBehaviour
{
    public MapData mapData;
    public int hour = 8;
    public bool winter = false;
    public bool war = false;
    public Game.Squadron[] squads;

    public AircraftScrollableList playerAircraft;
    public AircraftScrollableList ennemyAircraft;
    public InputField playerAmount;
    public InputField ennemyAmount;
    public InputField altitude;
    public InputField difficulty;
    public GameObject buyAircraft;
    public Button startGame;

    private void OnDisable()
    {
       if (buyAircraft)buyAircraft.SetActive(false);
    }

    public void StartMission()
    {
        if (playerAircraft)
        {
            if (!playerAircraft.SelectedCard.Available())
            {
                buyAircraft.SetActive(true);
                return;
            }
        }
        PlayerPrefs.SetInt("Winter", winter ? 1 : 0);
        PlayerPrefs.SetInt("War", war ? 1 : 0);
        PlayerPrefs.SetFloat("Hour", hour);
        PlayerPrefs.SetInt("SquadronsAmount", squads.Length);

        if (playerAircraft) squads[0].aircraftCard = playerAircraft.SelectedCard;
        if (ennemyAircraft) squads[1].aircraftCard = ennemyAircraft.SelectedCard;
        if (playerAmount) squads[0].amount = int.Parse(playerAmount.text);
        if (ennemyAmount) squads[1].amount = int.Parse(ennemyAmount.text);
        if (altitude)
        {
            float alt = Mathf.Clamp(float.Parse(altitude.text), 600f, 5000f);
            squads[0].startPosition.y = alt;
            squads[1].startPosition.y = alt;
        }
        if (difficulty)
        {
            float dif = Mathf.Clamp(float.Parse(difficulty.text), 0f, 100f)/100f;
            squads[0].difficulty = dif;
            squads[1].difficulty = dif;
        }
        for (int i = 0; i < squads.Length; i++)
        {
            PlayerPrefs.SetInt("Squadron" + i + "Team", (int)squads[i].team);
            PlayerPrefs.SetInt("Squadron" + i + "Aircraft", squads[i].aircraftCard.id);
            PlayerPrefs.SetInt("Squadron" + i + "Amount", squads[i].amount);
            PlayerPrefs.SetInt("Squadron" + i + "Player", squads[i].includePlayer ? 1 : 0);
            PlayerPrefs.SetFloat("Squadron" + i + "PosX", squads[i].startPosition.x);
            PlayerPrefs.SetFloat("Squadron" + i + "PosY", squads[i].startPosition.y);
            PlayerPrefs.SetFloat("Squadron" + i + "PosZ", squads[i].startPosition.z);
            PlayerPrefs.SetFloat("Squadron" + i + "Heading", squads[i].startHeading);
            PlayerPrefs.SetFloat("Squadron" + i + "Difficulty", squads[i].difficulty);
            PlayerPrefs.SetInt("Squadron" + i + "Airfield", squads[i].airfield); //Airfield are not supported yet on mission editor
        }

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
