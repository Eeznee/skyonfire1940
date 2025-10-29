using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Game Settings", menuName = "SOF/Game Data/Short Mission")]
[Serializable]
public class ShortMissionSO : ScriptableObject
{
    [Header("Mission Text")]
    public string missionName;
    [TextArea(8, 8)]
    public string missionDescription;

    [Header("Parameters")]
    public int hour = 8;
    public bool winter = false;
    public bool war = false;
    public Game.Squadron[] squads;

    [Header("Customizable Parameters")]
    public bool customPlayerAircraft;
    public bool playerCustomizationAvailable;
    public bool customTimeOfDay;


    public void StartMission(ShortMissionPanel missionPanel)
    {
        if (customTimeOfDay) PlayerPrefs.SetFloat("Hour", missionPanel.timeOfDay.value);
        else PlayerPrefs.SetFloat("Hour", hour);

        PlayerPrefs.SetInt("Winter", winter ? 1 : 0);
        PlayerPrefs.SetInt("War", war ? 1 : 0);
        PlayerPrefs.SetInt("SquadronsAmount", squads.Length);
        PlayerPrefs.SetInt("PlayerSquadron", 0);


        Game.Squadron playerSquadron = squads[0].CreateCopyOf();

        if (customPlayerAircraft) playerSquadron.aircraftCard = missionPanel.playerAircraft.SelectedCard;
        if (playerCustomizationAvailable)
        {
            playerSquadron.stations = missionPanel.modsDropdown.SelectedMods;
            playerSquadron.textureName = missionPanel.texturesDropdown.SelectedName;
        }

        playerSquadron.SaveSquadron(0);

        for (int i = 1; i < squads.Length; i++)
            squads[i].SaveSquadron(i);

        SceneManager.LoadScene("Dover");
    }

    public void StartMission(DogfightPanel missionPanel)
    {
        PlayerPrefs.SetFloat("Hour", missionPanel.timeOfDay.value);
        PlayerPrefs.SetInt("Winter", winter ? 1 : 0);
        PlayerPrefs.SetInt("War", war ? 1 : 0);
        PlayerPrefs.SetInt("SquadronsAmount", squads.Length);
        PlayerPrefs.SetInt("PlayerSquadron", 0);

        Game.Squadron playerSquadron = squads[0].CreateCopyOf();

        playerSquadron.aircraftCard = missionPanel.playerAircraft.SelectedCard;
        playerSquadron.amount = Mathf.RoundToInt(missionPanel.playerAmount.value);
        playerSquadron.stations = missionPanel.playerMods.SelectedMods;
        playerSquadron.textureName = missionPanel.playerTextures.SelectedName;


        Game.Squadron ennemySquadron = squads[1].CreateCopyOf();

        ennemySquadron.aircraftCard = missionPanel.ennemyAircraft.SelectedCard;
        ennemySquadron.amount = Mathf.RoundToInt(missionPanel.ennemyAmount.value);
        ennemySquadron.stations = missionPanel.ennemyMods.SelectedMods;
        ennemySquadron.textureName = missionPanel.ennemyTextures.SelectedName;

        playerSquadron.difficulty = ennemySquadron.difficulty = missionPanel.difficulty.value * 0.01f;
        playerSquadron.startPosition.y = ennemySquadron.startPosition.y = missionPanel.altitude.value;

        playerSquadron.SaveSquadron(0);
        ennemySquadron.SaveSquadron(1);

        SceneManager.LoadScene("Dover");
    }
}
