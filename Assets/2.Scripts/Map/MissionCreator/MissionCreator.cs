using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MissionCreator : MonoBehaviour
{
    [Header("Minimap")]
    public MapData mapData;
    RectTransform minimapTr;
    public SquadronOnMap squadronIcon;
    public SquadronPositionner squadronPositionner;

    [Header("Map Data")]
    public Slider hour;
    public Toggle winter;
    public Toggle war;

    [Header("Squadron Data")]
    public AircraftScrollableList aircraftList;
    public Toggle ally;
    public Toggle axis;
    public InputField amount;
    public Toggle player;

    [Header("Other")]
    public GameObject playerLabel;
    public GameObject buyAircraft;
    public Button startGame;

    Vector2 squadPos;
    private float lastAltitude = 500f;
    private float lastHeading = 0f;
    private float lastDifficulty = 0.4f;

    int playerId = -1;

    List<Game.Squadron> squadrons = new List<Game.Squadron>();
    [HideInInspector] public Game.Squadron squad;



    private void Start()
    {
        squadronPositionner.missionCreator = this;
        minimapTr = mapData.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (playerId == -1)
        {
            player.interactable = true;
            startGame.interactable = false;
            playerLabel.SetActive(true);
            buyAircraft.SetActive(false);
        }
        else if (!squadrons[squadrons.FindIndex(x => x.hiddenId == playerId)].aircraftCard.Available())
        {
            player.interactable = player.isOn = false;
            startGame.interactable = false;
            playerLabel.SetActive(false);
            buyAircraft.SetActive(true);
        }
        else
        {
            player.interactable = player.isOn = false;
            startGame.interactable = true;
            playerLabel.SetActive(false);
            buyAircraft.SetActive(false);
        }

#if MOBILE_INPUT
        if (Input.touches.Length == 0) return;
        if (Input.touches[0].tapCount == 2 && Input.GetTouch(0).position.x <= Screen.width * 2 / 3f && !squadronPositionner.gameObject.activeInHierarchy)
#else
        if (Input.GetMouseButtonDown(1) && Input.mousePosition.x <= Screen.width * 2 / 3f && !squadronPositionner.gameObject.activeInHierarchy)
#endif
        {
            //Calculate cursor position on map
            float widthScaling = Screen.width / GetComponentInParent<CanvasScaler>().referenceResolution.x;
            float heightScaling = Screen.width / GetComponentInParent<CanvasScaler>().referenceResolution.x;
            squadPos.x = (Input.mousePosition.x - minimapTr.position.x) / minimapTr.sizeDelta.x / widthScaling + 0.5f;
            squadPos.y = (Input.mousePosition.y - minimapTr.position.y) / minimapTr.sizeDelta.y / heightScaling + 0.5f;

            //Create the new squad
            Game.Team team = ally.isOn ? Game.Team.Ally : Game.Team.Axis;
            squad = new Game.Squadron(aircraftList.SelectedCard, team, 1,lastDifficulty * 100f, false);
            squad.startPosition.y = lastAltitude;
            squad.startHeading = lastHeading;
            squadrons.Add(squad);

            //Create the squadron game icon
            SquadronOnMap icon = Instantiate(squadronIcon, Input.mousePosition, Quaternion.identity, mapData.transform);
            icon.Create(squad);
        }
    }

    public void Confirm(float heading, float altitude, float difficulty, int id)
    {
        squad = squadrons.Find(x => x.hiddenId == id);
        squad.startPosition = mapData.RealMapPosition(squadPos, altitude);
        squad.startHeading = heading;
        squad.difficulty = difficulty;
        squad.amount = Mathf.Min(int.Parse(amount.text), aircraftList.SelectedCard.formation.aircraftPositions.Length);
        squad.team = ally.isOn ? Game.Team.Ally : Game.Team.Axis;
        squad.aircraftCard = aircraftList.SelectedCard;
        squad.includePlayer = (player.isOn && !squad.includePlayer) || (squad.includePlayer && startGame.interactable);

        squadrons[squadrons.FindIndex(x => x.hiddenId == id)] = squad;
        if (player.isOn)
            playerId = squad.hiddenId;

        lastAltitude = altitude;
        lastHeading = heading;
        lastDifficulty = difficulty;
    }
    public void Remove(int id)
    {
        if (id == playerId)
            playerId = -1;
        squadrons.RemoveAt(squadrons.FindIndex(x => x.hiddenId == id));
    }

    public void StartMission()
    {
        PlayerPrefs.SetInt("Winter", winter.isOn ? 1 : 0);
        PlayerPrefs.SetInt("War", war.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("Hour", hour.value);

        PlayerPrefs.SetInt("SquadronsAmount", squadrons.Count);
        for (int i = 0; i < squadrons.Count; i++)
        {
            PlayerPrefs.SetInt("Squadron" + i + "Team", (int)squadrons[i].team);
            PlayerPrefs.SetInt("Squadron" + i + "Aircraft", squadrons[i].aircraftCard.id);
            PlayerPrefs.SetInt("Squadron" + i + "Amount", squadrons[i].amount);
            PlayerPrefs.SetInt("Squadron" + i + "Player", squadrons[i].includePlayer ? 1 : 0);
            PlayerPrefs.SetFloat("Squadron" + i + "PosX", squadrons[i].startPosition.x);
            PlayerPrefs.SetFloat("Squadron" + i + "PosY", squadrons[i].startPosition.y);
            PlayerPrefs.SetFloat("Squadron" + i + "PosZ", squadrons[i].startPosition.z);
            PlayerPrefs.SetFloat("Squadron" + i + "Heading", squadrons[i].startHeading);
            PlayerPrefs.SetFloat("Squadron" + i + "Difficulty", squadrons[i].difficulty);
            PlayerPrefs.SetInt("Squadron" + i + "Airfield", -1); //Airfield are not supported yet on mission editor
        }

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
