using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "new Faction", menuName = "SOF/Game Data/Faction")]
public class Faction : ScriptableObject
{
    public GameObject crewMemberVisualModel;
    public Game.Team defaultTeam = Game.Team.Ally;
}
