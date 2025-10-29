using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuickDogfightMission : MonoBehaviour
{
    public MapData mapData;
    public int hour = 8;
    public bool winter = false;
    public bool war = false;
    public Game.Squadron[] squads;

    public string missionName;
    public string missionDescription;

    public ShortMissionPanel shortMissionPanel;

    public Slider timeOfDay;
    public AircraftsDropdown playerAircraft;
    public AircraftsDropdown ennemyAircraft;
    public Button openPlayerAircraftCustomizer;
    public InputField playerAmount;
    public InputField ennemyAmount;
    public InputField altitude;
    public InputField difficulty;
    public Button startGame;

    public bool customPlayerAircraft;
    public bool playerCustomizationAvailable;
    public bool customTimeOfDay;


    private int[] selectedPlayerMods;
    private string selectedPlayerTexture;

    private void Start()
    {

    }
    private void OnEnable()
    {
        openPlayerAircraftCustomizer.onClick.AddListener(OpenPlayerAircraftCustomizer);
        playerAircraft.OnAircraftChange += ResetPlayerAircraftCustomization;
    }
    private void OpenPlayerAircraftCustomizer()
    {
        //aircraftCustomizer.StartCustomizing(gameObject, playerAircraft.SelectedCard, OnAircraftCustomizerConfirmed);
    }
    private void OnAircraftCustomizerConfirmed()
    {
        //selectedPlayerMods = aircraftCustomizer.mods.SelectedMods;
        //selectedPlayerTexture = aircraftCustomizer.texture.SelectedName;
    }
    private void ResetPlayerAircraftCustomization()
    {
        selectedPlayerMods = null;
        selectedPlayerTexture = null;
    }

    public void StartMission()
    {
        if (playerAircraft)
        {
            if (!playerAircraft.SelectedCard.Available())
            {
                //buyAircraft.SetActive(true);
                return;
            }
        }

        if (timeOfDay) PlayerPrefs.SetFloat("Hour", timeOfDay.value);
        else PlayerPrefs.SetFloat("Hour", hour);

        PlayerPrefs.SetInt("Winter", winter ? 1 : 0);
        PlayerPrefs.SetInt("War", war ? 1 : 0);
        PlayerPrefs.SetInt("SquadronsAmount", squads.Length);

        if (playerAircraft) squads[0].aircraftCard = playerAircraft.SelectedCard;
        if (playerAmount) squads[0].amount = int.Parse(playerAmount.text);
        if (selectedPlayerMods != null) squads[0].stations = selectedPlayerMods;
        if (!string.IsNullOrEmpty(selectedPlayerTexture)) squads[0].textureName = selectedPlayerTexture;

        if (ennemyAircraft) squads[1].aircraftCard = ennemyAircraft.SelectedCard;
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
            squads[i].SaveSquadron(i);

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
