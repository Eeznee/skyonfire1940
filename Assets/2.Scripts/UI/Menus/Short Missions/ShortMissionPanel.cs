using UnityEngine;
using UnityEngine.UI;
public class ShortMissionPanel : MonoBehaviour
{
    public Slider timeOfDay;
    public Text missionLabel;
    public Text missionDescription;

    public GameObject missionSelectorPanel;
    public AircraftsDropdown playerAircraft;
    public TexturesDropdown texturesDropdown;
    public ModsDropdown modsDropdown;
    //public GameObject buyAircraft;
    public Button startGame;
    public Button exitPanel;

    private void Awake()
    {
        texturesDropdown.LinkNewAircraftsDropdown(playerAircraft);
        modsDropdown.LinkNewAircraftDropdown(playerAircraft);

        exitPanel.onClick.RemoveAllListeners();
        exitPanel.onClick.AddListener(ExitPanel);
    }
    public void ExitPanel()
    {
        missionSelectorPanel.SetActive(true);
        gameObject.SetActive(false);
    }
    public void LoadShortMission(ShortMissionSO shortMission)
    {
        missionSelectorPanel.SetActive(false);
        gameObject.SetActive(true);

        missionLabel.text = shortMission.missionName;
        missionDescription.text = shortMission.missionDescription;

        timeOfDay.value = shortMission.hour;
        timeOfDay.interactable = shortMission.customTimeOfDay;

        playerAircraft.SelectAircraft(shortMission.squads[0].aircraftCard);
        playerAircraft.dropdown.interactable = shortMission.customPlayerAircraft;

        modsDropdown.ResetAndSelect(shortMission.squads[0].stations);
        texturesDropdown.dropdown.interactable = shortMission.playerCustomizationAvailable;



        //playerAircraft.transform.parent.gameObject.SetActive(shortMission.customPlayerAircraft);
        //texturesDropdown.transform.parent.gameObject.SetActive(shortMission.playerCustomizationAvailable);
        //modsDropdown.transform.parent.gameObject.SetActive(shortMission.playerCustomizationAvailable);

        //timeOfDay.value = shortMission.hour;
        //playerAircraft.SelectAircraft(shortMission.squads[0].aircraftCard);

        startGame.onClick.RemoveAllListeners();
        startGame.onClick.AddListener(delegate { shortMission.StartMission(this); });
    }
}
