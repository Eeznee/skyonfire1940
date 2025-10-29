using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Visibility
{
    private CrewSeat seat;
    private SofObject sofObject;
    public float front = 1f;
    public float sides = 1f;
    public float rear = 0.5f;
    public float top = 1f;
    public float bottom = 0f;

    const float pastDelay = 2f;
    const float deckVisibility = 0.5f;
    const float minSpottingDistance = 250f;
    const float maxSpottingDistance = 4000f;    //Valid for a 12m wing span aircraft or below
    const float maxSpottingWingSpan = 12f;
    public void Initialize(CrewSeat s)
    {
        seat = s;
        sofObject = s.sofObject;
        front = Mathf.Clamp01(front);
        sides = Mathf.Clamp01(sides);
        rear = Mathf.Clamp01(rear);
        top = Mathf.Clamp01(top);
        bottom = Mathf.Clamp01(bottom);
    }
    public float TargetVisibility(SofAircraft target, float distance)
    {
        Transform tr = seat.sofObject.transform;
        Vector3 pastPos = target.transform.position - target.rb.linearVelocity * pastDelay;
        Vector3 localPos = tr.InverseTransformPoint(pastPos);

        //Orientation of target relative to seat affects visibility
        float orientation = Mathf.Abs(localPos.x) * sides;
        orientation += Mathf.Abs(localPos.y) * (localPos.y > 0f ? top : bottom);
        orientation += Mathf.Abs(localPos.z) * (localPos.z > 0f ? front : rear);
        float sum = Mathf.Abs(localPos.x) + Mathf.Abs(localPos.y) + Mathf.Abs(localPos.z);
        orientation /= sum;

        //Ground affects visibility
        float ground = -(pastPos - tr.position).y / distance;
        ground = ground * ground - 2f * ground + 1f; //Creates a curve like this \_/
        ground = Mathf.Lerp(deckVisibility, 1f, ground);

        //TODO : Sun affects visibility
        float visibility = orientation * ground;
        return visibility;
    }
    public float SpotDistance(SofAircraft target, float spottingStrength, float visibility)
    {
        float wingSpanMultiplier = Mathf.Min(1f, target.stats.wingSpan / maxSpottingWingSpan);
        float minDistance = seat.target == target ? minSpottingDistance * 2.5f : minSpottingDistance;
        float spottingDistance = Mathf.Lerp(minDistance, maxSpottingDistance, visibility * spottingStrength);
        spottingDistance *= wingSpanMultiplier;
        return  spottingDistance;
    }
    public List<SofAircraft> Spot()
    {
        //Mark all ennemies on the map
        List<SofAircraft> ennemies = (sofObject.tag == "Ally") ? GameManager.axisAircrafts : GameManager.allyAircrafts;
        if (ennemies.Count == 0 || sofObject.tag == "Neutral") return null;

        List<SofAircraft> spotted = new List<SofAircraft>();

        for (int i = 0; i < ennemies.Count; i++)
        {
            float distance = Vector3.Distance(ennemies[i].transform.position, sofObject.transform.position);
            float visibility = TargetVisibility(ennemies[i], distance);
            float spotDistance = SpotDistance(ennemies[i], 1f, visibility);
            if (distance < spotDistance) spotted.Add(ennemies[i]);
        }
        return spotted;
    }
}