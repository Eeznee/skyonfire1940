using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LodModule : ObjectElement
{
    const float disFull = 100f;
    const float disSimple = 500f;
    const float disHalf = 2000f;

    private float fullSqr;
    private float simpleSqr;
    private float halfSqr;

    //0 is full fidelity, 1 is simplified, 2 is half, 3 is minimum, and 4 is invisible
    private int lod = 0;
    private bool switched = false;
    public int LOD(){ return lod; }
    public bool Switched() { return switched; }

    private void Awake()
    {
        fullSqr = disFull * disFull;
        simpleSqr = disSimple * disSimple;
        halfSqr = disHalf * disHalf;
    }
    void Update()
    {
        switched = false;
        float dis = (data.position - PlayerCamera.camPos).sqrMagnitude;

        int newLod = 0;
        if (dis > fullSqr) newLod++;
        if (dis > simpleSqr) newLod++;
        if (dis > halfSqr) newLod++;
        if (newLod < 3)
        {
            Bounds bounds = new Bounds(data.position, Vector3.one * 10f);
            if (!GeometryUtility.TestPlanesAABB(PlayerCamera.frustrumPlanes, bounds)) newLod = 4;
        }
        if (PlayerManager.player.aircraft == aircraft || PlayerCamera.viewMode < 0 || Time.timeScale == 0f) newLod = 0;

        if (newLod != lod) {
            lod = newLod;
            switched = true;
        }
    }
}
