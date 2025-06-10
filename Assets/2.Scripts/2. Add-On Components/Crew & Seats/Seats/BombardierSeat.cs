using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Crew Seats/Bombardier Seat")]
public class BombardierSeat : CrewSeat
{
    private Bombsight bombsight;

    const float maxYawInput = 0.3f;
    public float forcedYawInput = 0f;

    public override int Priority
    {
        get
        {
            bool highPriority = Player.Squadron == aircraft.squadron;
            highPriority &= Player.bombardierSeat != null;
            return highPriority ? 4 : 1;
        }
    }
    public override Vector3 LookingDirection => -transform.root.up;

    public override void SetReferences(SofModular _complex)
    {
        base.SetReferences(_complex);
        bombsight = aircraft.bombSight;
    }

    public override void PlayerUpdate(CrewMember crew)
    {
        base.PlayerUpdate(crew);
        bombsight.Operate();
    }
    public override void PlayerFixed(CrewMember crew)
    {
        base.PlayerFixed(crew);
        forcedYawInput = -PlayerActions.bomber.Rudder.ReadValue<float>() * maxYawInput;
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);

        SofAircraft leader = GameManager.squadrons[aircraft.SquadronId][0];
        if (leader == aircraft) return;
        Bombsight leadSight = leader.bombSight;

        bombsight.CopyLeaderBombsight(leadSight);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BombardierSeat)), CanEditMultipleObjects]
public class BombardierSeatEditor : CrewSeatEditor
{
}
#endif