using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SquadronOnMap : MonoBehaviour
{
    [Header("UI")]
    public Transform direction;
    public Button iconButton;
    public Image sideIcon;
    public Sprite allies;
    public Sprite axis;
    public GameObject Submenu;
    public Text aircrafts;
    public Text altitude;
    public Text difficulty;
    public RectTransform player;
    [Header("References")]
    public SquadronPositionner positionner;

    [HideInInspector] public Game.Squadron squad;


    private void OnEnable()
    {
        positionner.missionCreator.OnPlayerSquadronChanged += UpdatePlayerState;
        iconButton.onClick.RemoveAllListeners();
        iconButton.onClick.AddListener(StartEditing);
    }
    private void OnDisable()
    {
        positionner.missionCreator.OnPlayerSquadronChanged -= UpdatePlayerState;
    }

    public void Create(Game.Squadron _squad)
    {
        squad = _squad;
        gameObject.SetActive(true);
        StartEditing();
    }
    public void UpdateInterface(float heading, bool team)
    {
        direction.rotation = Quaternion.identity;
        direction.Rotate(-Vector3.forward * heading);

        sideIcon.sprite = team ? allies : axis;

        UpdatePlayerState();
    }
    public void StartEditing()
    {
        Submenu.SetActive(false);
        positionner.missionCreator.LoadValues(squad);
        positionner.EditSquad(this);
    }
    public void StopEditing()
    {
        Submenu.SetActive(true);
        aircrafts.text = squad.aircraftCard.completeName + " x " + squad.amount;
        altitude.text = "Alt : " + squad.startPosition.y * UnitsConverter.altitude.Multiplier + " " + UnitsConverter.altitude.Symbol;
        difficulty.text = "Difficulty : " + (squad.difficulty * 100f).ToString("0");
    }
    public void UpdatePlayerState()
    {
        player.gameObject.SetActive(positionner.missionCreator.playerSquad == squad);
    }
}
