using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    public static int idCounter = 0;
    public enum ControlType { AI, Player }
    public enum Team { Ally = 0, Axis = 1, Neutral = 2 }

    [System.Serializable]
    public class Squadron
    {
        public AircraftCard aircraftCard;
        public Team team;
        public int id;
        public int amount;
        public bool includePlayer;
        public float startHeading = 0f;
        public float difficulty;
        public Vector3 startPosition = new Vector3(0f, 0f, 2000f);
        public int airfield = -1;

        [HideInInspector]public int hiddenId;

        public Squadron(AircraftCard card, Team t, int a,float dif, bool player)
        {
            aircraftCard = card;
            team = t;
            amount = a;
            difficulty = dif;
            includePlayer = player;
            
            hiddenId = idCounter;
            idCounter++;
        }
    }
}
