using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    public enum Team { Ally = 0, Axis = 1, Neutral = 2 }

    [System.Serializable]
    public class Squadron
    {
        //Essentials
        public AircraftCard aircraftCard;
        public Team team;
        public int amount;
        [HideInInspector] public int id;

        //Optional
        public float startHeading = 0f;
        public float difficulty;
        public Vector3 startPosition = new Vector3(0f, 0f, 2000f);
        public int airfield = -1;

        //Aircraft customization
        public int[] stations;
        [HideInInspector] public string textureName;

        public Squadron CreateCopyOf()
        {
            Squadron copy = new Squadron();

            copy.aircraftCard = aircraftCard;
            copy.team = team;
            copy.amount = amount;
            copy.id = id;
            copy.startHeading = startHeading;
            copy.startPosition = startPosition;
            copy.difficulty = difficulty;
            copy.airfield = airfield;
            copy.stations = stations;
            copy.textureName = textureName;

            return copy;
        }
        public Squadron()
        {

        }
        public Squadron(AircraftCard card, bool _team, int _amount,float dif)
        {
            aircraftCard = card;
            team = _team ? Team.Ally : Team.Axis;
            amount = _amount;
            difficulty = dif;
            stations = new int[card.sofAircraft.Stations.Length];
        }
        public static Squadron LoadSquadron(int id)
        {
            string start = "Squadron" + id;
            Squadron squadron = new Squadron();
            squadron.id = id;
            squadron.team = (Team)PlayerPrefs.GetInt(start + "Team");
            squadron.aircraftCard = StaticReferences.Instance.defaultAircrafts.list[PlayerPrefs.GetInt(start + "Aircraft")];
            squadron.amount = PlayerPrefs.GetInt(start + "Amount");

            squadron.airfield = PlayerPrefs.GetInt(start + "Airfield");
            Vector3 position;
            position.x = PlayerPrefs.GetFloat(start + "PosX");
            position.y = PlayerPrefs.GetFloat(start + "PosY");
            position.z = PlayerPrefs.GetFloat(start + "PosZ");
            squadron.startPosition = position;
            squadron.startHeading = PlayerPrefs.GetFloat(start + "Heading");
            squadron.difficulty = PlayerPrefs.GetFloat(start + "Difficulty");

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

            PlayerPrefs.SetFloat(start + "PosX", startPosition.x);
            PlayerPrefs.SetFloat(start + "PosY", startPosition.y);
            PlayerPrefs.SetFloat(start + "PosZ", startPosition.z);
            PlayerPrefs.SetFloat(start + "Heading", startHeading);
            PlayerPrefs.SetFloat(start + "Difficulty", Mathf.Clamp01(difficulty));
            PlayerPrefs.SetInt(start + "Airfield", airfield); //Airfield are not supported yet on mission editor

            for (int i = 0; i < aircraftCard.sofAircraft.Stations.Length; i++)
                PlayerPrefs.SetInt(start + "Station" + i, i < stations.Length ? stations[i] : 0);
            PlayerPrefs.SetString(start + "TextureName", textureName);
        }
    }
}
