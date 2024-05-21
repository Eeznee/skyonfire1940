using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BombardierSeat : CrewSeat
{
    private Bombsight bombsight;

    const float maxYawInput = 0.3f;

    public override int Priority => 1;
    public override Vector3 LookingDirection => -transform.root.up;

    public override void SetReferences(SofComplex _complex)
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
        aircraft.inputs.current.yaw = -PlayerActions.bomber.Rudder.ReadValue<float>() * maxYawInput;
    }
    public override void AiUpdate(CrewMember crew)
    {
        base.AiUpdate(crew);

        SofAircraft leader = GameManager.squadrons[aircraft.squadronId][0];
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