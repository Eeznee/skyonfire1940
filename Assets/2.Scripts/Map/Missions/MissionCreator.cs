using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
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
    public AircraftsDropdown aircraft;
    public Toggle ally;
    public Toggle axis;
    public InputField amount;
    public Toggle player;
    public TexturesDropdown texture;
    public ModsDropdown mods;

    [Header("Other")]
    public GameObject playerLabel;
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

    private void OnEnable()
    {
        PlayerActions.actions.UI.SecondaryClick.performed += OnSecondaryClick;
    }
    private void OnDisable()
    {
        PlayerActions.actions.UI.SecondaryClick.performed -= OnSecondaryClick;
    }
    private void OnSecondaryClick(InputAction.CallbackContext context)
    {
        if (PlayerActions.actions.UI.Point.ReadValue<Vector2>().x <= Screen.width * 2 / 3f && !squadronPositionner.gameObject.activeInHierarchy)
        {
            SpawnSquadronOnMousePos();
        }
    }
    private void SpawnSquadronOnMousePos()
    {
        Vector2 mousePos = PlayerActions.actions.UI.Point.ReadValue<Vector2>();

        //Calculate cursor position on map
        float widthScaling = Screen.width / GetComponentInParent<CanvasScaler>().referenceResolution.x;
        float heightScaling = Screen.width / GetComponentInParent<CanvasScaler>().referenceResolution.x;
        squadPos.x = (mousePos.x - minimapTr.position.x) / minimapTr.sizeDelta.x / widthScaling + 0.5f;
        squadPos.y = (mousePos.y - minimapTr.position.y) / minimapTr.sizeDelta.y / heightScaling + 0.5f;
        squadPos = mapData.RealMapPosition(squadPos);
        //Create the new squad
        Game.Team team = ally.isOn ? Game.Team.Ally : Game.Team.Axis;
        squad = new Game.Squadron(aircraft.SelectedCard, team, 1, lastDifficulty * 100f, false);
        squad.startPosition.y = lastAltitude;
        squad.startHeading = lastHeading;
        squadrons.Add(squad);

        texture.Reset(texture.SelectedName);

        //Create the squadron game icon
        SquadronOnMap icon = Instantiate(squadronIcon, mousePos, Quaternion.identity, mapData.transform);
        icon.Create(squad);
    }

    private void Update()
    {
        if (playerId == -1)
        {
            player.interactable = true;
            startGame.interactable = false;
            playerLabel.SetActive(true);
        }
        else
        {
            player.interactable = player.isOn = false;
            startGame.interactable = true;
            playerLabel.SetActive(false);
        }
    }


    public void LoadValues(Game.Squadron toLoad)
    {
        squadPos = new Vector2(toLoad.startPosition.x, toLoad.startPosition.z);
        amount.text = toLoad.amount.ToString();
        ally.isOn = toLoad.team == Game.Team.Ally;
        axis.isOn = toLoad.team == Game.Team.Axis;
        aircraft.Reset(toLoad.aircraftCard.id);
        texture.Reset(toLoad.textureName);
        mods.Reset(toLoad.stations);
        player.isOn = toLoad.player;
        if (player.isOn) playerId = -1;
    }

    public void Confirm(float heading, float altitude, float difficulty, int id)
    {
        squad = squadrons.Find(x => x.hiddenId == id);
        squad.startPosition = new Vector3(squadPos.x, altitude, squadPos.y);
        squad.startHeading = heading;
        squad.difficulty = difficulty;
        squad.amount = Mathf.Min(int.Parse(amount.text), aircraft.SelectedCard.formation.aircraftPositions.Length);
        squad.team = ally.isOn ? Game.Team.Ally : Game.Team.Axis;
        squad.aircraftCard = aircraft.SelectedCard;
        squad.textureName = texture.SelectedName;
        squad.stations = mods.SelectedMods;
        squad.player = player.isOn;
        squad.airfield = -1;
        if (squad.player)
            playerId = squad.hiddenId;

        squadrons[squadrons.FindIndex(x => x.hiddenId == id)] = squad;

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
        for (int i = 0; i < squadrons.Count; i++) squadrons[i].SaveSquadron(i);

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
