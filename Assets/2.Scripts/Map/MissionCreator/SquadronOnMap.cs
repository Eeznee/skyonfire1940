using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SquadronOnMap : MonoBehaviour
{
    [Header("UI")]
    public Transform direction;
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

    [HideInInspector]public Game.Squadron assignedSquad;
    [HideInInspector]public int hiddenId = 0;


    public void Create(Game.Squadron squad)
    {
        sideIcon.sprite = (squad.team == Game.Team.Ally) ? allies : axis;

        assignedSquad = squad;
        hiddenId = squad.hiddenId;

        UpdateHeading(squad.startHeading);
        Edit();
    }
    public void UpdateHeading(float heading)
    {
        direction.rotation = Quaternion.identity;
        direction.Rotate(Vector3.forward * heading);
    }
    public void Edit()
    {
        Submenu.SetActive(false);
        positionner.EditSquad(this);
    }
    public void LoadValues()
    {
        positionner.missionCreator.LoadValues(assignedSquad);
    }

    public void StopEdit()
    {
        Submenu.SetActive(true);
        aircrafts.text = assignedSquad.aircraftCard.completeName + " x " + assignedSquad.amount;
        player.gameObject.SetActive(assignedSquad.player);
        altitude.text = "Alt : " + assignedSquad.startPosition.y * UnitsConverter.altitude.Multiplier + " " + UnitsConverter.altitude.Symbol;
        difficulty.text = "Difficulty : " + (assignedSquad.difficulty * 100f).ToString("0");
    }
}
