using UnityEngine;
using UnityEngine.UI;
public class DogfightPanel : MonoBehaviour
{
    [Header("Main UI Elements")]
    public Text missionLabel;
    public Text missionDescription;
    public Button startGame;
    public Button exitPanel;
    public GameObject missionSelectorPanel;

    [Header("General Config")]
    public Slider timeOfDay;
    public Slider altitude;
    public Slider difficulty;

    [Header("Player Config")]
    public AircraftsDropdown playerAircraft;
    public Slider playerAmount;
    public TexturesDropdown playerTextures;
    public ModsDropdown playerMods;

    [Header("Ennemy Config")]
    public AircraftsDropdown ennemyAircraft;
    public Slider ennemyAmount;
    public TexturesDropdown ennemyTextures;
    public ModsDropdown ennemyMods;

    private bool firstTimeLoading;

    private void Awake()
    {
        playerTextures.LinkNewAircraftsDropdown(playerAircraft);
        playerMods.LinkNewAircraftDropdown(playerAircraft);

        ennemyTextures.LinkNewAircraftsDropdown(ennemyAircraft);
        ennemyMods.LinkNewAircraftDropdown(ennemyAircraft);

        exitPanel.onClick.RemoveAllListeners();
        exitPanel.onClick.AddListener(ExitPanel);

        firstTimeLoading = true;
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

        if (firstTimeLoading)
        {
            missionLabel.text = shortMission.missionName;
            missionDescription.text = shortMission.missionDescription;

            timeOfDay.value = shortMission.hour;
            altitude.value = shortMission.squads[0].startPosition.y;
            difficulty.value = shortMission.squads[0].difficulty * 100f;

            playerAircraft.SelectAircraft(shortMission.squads[0].aircraftCard);
            playerAmount.value = shortMission.squads[0].amount;
            playerMods.ResetAndSelect(shortMission.squads[0].stations);

            ennemyAircraft.SelectAircraft(shortMission.squads[1].aircraftCard);
            ennemyAmount.value = shortMission.squads[1].amount;
            ennemyMods.ResetAndSelect(shortMission.squads[1].stations);
        }

        startGame.onClick.RemoveAllListeners();
        startGame.onClick.AddListener(delegate { shortMission.StartMission(this); });

        firstTimeLoading = false;
    }
}
