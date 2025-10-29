using System;
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
    public Slider amount;
    public TexturesDropdown texture;
    public ModsDropdown mods;

    [Header("Other")]
    public GameObject playerLabel;
    public Button startGame;

    private float lastAltitude = 500f;
    private float lastHeading = 0f;
    private float lastDifficulty = 0.4f;

    List<Game.Squadron> squadrons = new List<Game.Squadron>();

    public Game.Squadron playerSquad;

    public Action OnPlayerSquadronChanged;

    private void Start()
    {
        playerSquad = null;
        squadronPositionner.missionCreator = this;
        minimapTr = mapData.GetComponent<RectTransform>();

        ControlsManager.uiActions.Main.SecondaryClick.performed += OnSecondaryClick;
    }
    private void OnSecondaryClick(InputAction.CallbackContext context)
    {
        if (ControlsManager.uiActions.Main.Point.ReadValue<Vector2>().x > Screen.width * 2 / 3f) return;
        if (squadronPositionner.gameObject.activeInHierarchy) return;

        SpawnSquadronOnMousePos();
    }

    public Vector2 MouseToWorldPos(Vector2 mousePos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapTr, mousePos, null, out Vector2 relativePos);

        relativePos.x = Mathf.InverseLerp(minimapTr.rect.xMin, minimapTr.rect.xMax, relativePos.x);
        relativePos.y = Mathf.InverseLerp(minimapTr.rect.yMin, minimapTr.rect.yMax, relativePos.y);

        return mapData.RealMapPosition(relativePos);
    }

    private void SpawnSquadronOnMousePos()
    {
        Vector2 mousePos = ControlsManager.uiActions.Main.Point.ReadValue<Vector2>();

        Vector2 squadPos = MouseToWorldPos(mousePos);

        foreach (Game.Squadron squadron in squadrons)
            if ((new Vector2(squadron.startPosition.x, squadron.startPosition.z) - squadPos).magnitude < 15f) return;

        Game.Squadron newSquadron = new Game.Squadron(aircraft.SelectedCard, ally.isOn, 1, lastDifficulty * 100f);
        newSquadron.startPosition = new Vector3(squadPos.x, lastAltitude, squadPos.y);

        ApplyValuesToSquadron(newSquadron, lastHeading, lastAltitude, lastDifficulty);
        squadrons.Add(newSquadron);

        Instantiate(squadronIcon, mousePos, Quaternion.identity, mapData.transform).Create(newSquadron);
    }

    public void LoadValues(Game.Squadron squadronToLoad)
    {
        amount.value = squadronToLoad.amount;
        ally.isOn = squadronToLoad.team == Game.Team.Ally;
        axis.isOn = squadronToLoad.team == Game.Team.Axis;

        aircraft.ResetAndSelect(squadronToLoad.aircraftCard.id);
        texture.ResetAndSelect(squadronToLoad.textureName);
        mods.ResetAndSelect(squadronToLoad.stations);
    }

    public void ApplyValuesToSquadron(Game.Squadron squad, float heading, float altitude, float difficulty)
    {
        squad.amount = Mathf.Min(Mathf.RoundToInt(amount.value), aircraft.SelectedCard.formation.aircraftPositions.Length);
        squad.team = ally.isOn ? Game.Team.Ally : Game.Team.Axis;
        squad.aircraftCard = aircraft.SelectedCard;
        squad.textureName = texture.SelectedName;
        squad.stations = mods.SelectedMods;
        squad.airfield = -1;

        squad.startHeading = heading;
        squad.startPosition.y = altitude;
        squad.difficulty = difficulty;
    }
    public void UpdateSquadronValues(float heading, float altitude, float difficulty, Game.Squadron squad)
    {
        ApplyValuesToSquadron(squad, heading, altitude, difficulty);

        UpdatePlayerState();

        lastAltitude = altitude;
        lastHeading = heading;
        lastDifficulty = difficulty;
    }
    public void Remove(Game.Squadron _squad)
    {
        squadrons.Remove(_squad);

        if (_squad == playerSquad) playerSquad = null;

        UpdatePlayerState();
    }
    public void SetPlayer(Game.Squadron _squad)
    {
        playerSquad = _squad;
        OnPlayerSquadronChanged?.Invoke();
        UpdatePlayerState();
    }
    public void UpdatePlayerState()
    {
        bool hasPlayer = playerSquad != null;

        startGame.interactable = hasPlayer;
        playerLabel.SetActive(!hasPlayer);
    }

    public void StartMission()
    {
        if(squadronPositionner.currentSquadron != null) squadronPositionner.Confirm();

        PlayerPrefs.SetInt("Winter", winter.isOn ? 1 : 0);
        PlayerPrefs.SetInt("War", war.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("Hour", hour.value);
        PlayerPrefs.SetInt("SquadronsAmount", squadrons.Count);
        for (int i = 0; i < squadrons.Count; i++)
        {
            if (squadrons[i] == playerSquad) PlayerPrefs.SetInt("PlayerSquadron", i);
            squadrons[i].SaveSquadron(i);
        }

        SceneManager.LoadScene(mapData.assignedScene);
    }
}
