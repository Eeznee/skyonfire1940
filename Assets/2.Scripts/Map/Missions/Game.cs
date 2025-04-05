using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    private static int idCounter = 0;
    public enum Team { Ally = 0, Axis = 1, Neutral = 2 }

    [System.Serializable]
    public class Squadron
    {
        //Essentials
        public AircraftCard aircraftCard;
        public Team team;
        public int id;
        public int amount;
        public bool player;
        [HideInInspector] public int hiddenId;

        //Optional
        public float startHeading = 0f;
        public float difficulty;
        public Vector3 startPosition = new Vector3(0f, 0f, 2000f);
        public int airfield = -1;

        //Aircraft customization
        public float fuel;
        public int[] stations;
        public string textureName;
        public float convergeance;
        public float rocketsFuzeMeters;
        public float bombsFuzeSeconds;

        public Squadron()
        {
            hiddenId = idCounter;
            idCounter++;
        }
        public Squadron(AircraftCard card, Team _team, int _amount,float dif, bool _player)
        {
            aircraftCard = card;
            team = _team;
            amount = _amount;
            difficulty = dif;
            player = _player;
            stations = new int[card.sofAircraft.Stations.Length];

            hiddenId = idCounter;
            idCounter++;
        }
        public static Squadron LoadSquadron(int id)
        {
            string start = "Squadron" + id;
            Squadron squadron = new Squadron();
            squadron.id = id;
            squadron.team = (Team)PlayerPrefs.GetInt(start + "Team");
            squadron.aircraftCard = StaticReferences.Instance.defaultAircrafts.list[PlayerPrefs.GetInt(start + "Aircraft")];
            squadron.amount = PlayerPrefs.GetInt(start + "Amount");
            squadron.player = PlayerPrefs.GetInt(start + "Player") == 1;

            squadron.airfield = PlayerPrefs.GetInt(start + "Airfield");
            Vector3 position;
            position.x = PlayerPrefs.GetFloat(start + "PosX");
            position.y = PlayerPrefs.GetFloat(start + "PosY");
            position.z = PlayerPrefs.GetFloat(start + "PosZ");
            squadron.startPosition = position;
            squadron.startHeading = PlayerPrefs.GetFloat(start + "Heading");
            squadron.difficulty = PlayerPrefs.GetFloat(start + "Difficulty");

            squadron.fuel = PlayerPrefs.GetFloat(start + "Fuel", 1f);
            squadron.convergeance = PlayerPrefs.GetFloat(start + "Convergeance", 250f);
            squadron.rocketsFuzeMeters = PlayerPrefs.GetFloat(start + "RocketsFuze", 0f);
            squadron.bombsFuzeSeconds = PlayerPrefs.GetFloat(start + "BombsFuze", 3f);
            squadron.textureName = PlayerPrefs.GetString(start + "TextureName", "");
            Station[] stations = squadron.aircraftCard.aircraft.GetComponent<SofAircraft>().Stations;
            squadron.stations = new int[stations.Length];
            for (int i = 0; i < stations.Length; i++)
                squadron.stations[i] = PlayerPrefs.GetInt(start + "Station" + i, 0);

            return squadron;
        }
        public void SaveSquadron(int id)
        {
            string start = "Squadron" + id;
            PlayerPrefs.SetInt(start + "Team", (int)team);
            PlayerPrefs.SetInt(start + "Aircraft", aircraftCard.id);
            PlayerPrefs.SetInt(start + "Amount", amount);
            //PlayerPrefs.SetInt(start + "Player", player ? 1 : 0);
            if (player) PlayerPrefs.SetInt("PlayerSquadron", id);

            PlayerPrefs.SetFloat(start + "PosX", startPosition.x);
            PlayerPrefs.SetFloat(start + "PosY", startPosition.y);
            PlayerPrefs.SetFloat(start + "PosZ", startPosition.z);
            PlayerPrefs.SetFloat(start + "Heading", startHeading);
            PlayerPrefs.SetFloat(start + "Difficulty", Mathf.Clamp01(difficulty));
            PlayerPrefs.SetInt(start + "Airfield", airfield); //Airfield are not supported yet on mission editor

            PlayerPrefs.SetFloat(start + "Fuel", Mathf.Clamp01(fuel));
            PlayerPrefs.SetFloat(start + "Convergeance", convergeance);
            PlayerPrefs.SetFloat(start + "RocketsFuze", rocketsFuzeMeters);
            PlayerPrefs.SetFloat(start + "BombsFuze", bombsFuzeSeconds);
            for (int i = 0; i < aircraftCard.sofAircraft.Stations.Length; i++)
                PlayerPrefs.SetInt(start + "Station" + i, i < stations.Length ? stations[i] : 0);
            PlayerPrefs.SetString(start + "TextureName", textureName);
        }
    }
}
