using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShortMission : MonoBehaviour
{
    [Header("UI References")]
    public ShortMissionPanel shortMissionPanel;
    public Button openMissionPanel;

    [Header("Mission Parameters")]
    public string missionName;
    [TextArea(8,8)]
    public string missionDescription;

    public MapData mapData;
    public int hour = 8;
    public bool winter = false;
    public bool war = false;
    public Game.Squadron[] squads;

    [Header("Customizable Parameters")]
    public bool customPlayerAircraft;
    public bool playerCustomizationAvailable;
    public bool customTimeOfDay;

    private void Start()
    {
        openMissionPanel.onClick.RemoveAllListeners();
        openMissionPanel.onClick.AddListener(OpenMissionPanel);
    }
    private void OpenMissionPanel()
    {
        //shortMissionPanel.LoadShortMission(this);
    }

    public void StartMission()
    {
        /*
        if (playerAircraft)
        {
            if (!playerAircraft.SelectedCard.Available())
            {
                buyAircraft.SetActive(true);
                return;
            }
        }
        */
        if (customTimeOfDay) PlayerPrefs.SetFloat("Hour", shortMissionPanel.timeOfDay.value);
        else PlayerPrefs.SetFloat("Hour", hour);

        PlayerPrefs.SetInt("Winter", winter ? 1 : 0);
        PlayerPrefs.SetInt("War", war ? 1 : 0);
        PlayerPrefs.SetInt("SquadronsAmount", squads.Length);

        if (customPlayerAircraft) squads[0].aircraftCard = shortMissionPanel.playerAircraft.SelectedCard;
        if(playerCustomizationAvailable)
        {
            squads[0].stations = shortMissionPanel.modsDropdown.SelectedMods;
            squads[0].textureName = shortMissionPanel.texturesDropdown.SelectedName;
        }

        for (int i = 0; i < squads.Length; i++)
            squads[i].SaveSquadron(i);

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
