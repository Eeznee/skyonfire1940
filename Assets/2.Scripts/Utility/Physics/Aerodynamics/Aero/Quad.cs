using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Quad
{
    public Quad(Transform _tr,Vector3 lt, Vector3 lb, Vector3 tt, Vector3 tb)
    {
        tr = _tr;
        leadingTop = lt;
        leadingBot = lb;
        trailingTop = tt;
        trailingBot = tb;

        botAeroPos = Vector3.Lerp(trailingBot, leadingBot, liftLine);
        topAeroPos = Vector3.Lerp(trailingTop, leadingTop, liftLine);
        botToTopDir = (topAeroPos - botAeroPos).normalized;
        float sign = Mathf.Abs(botToTopDir.x) < 0.1f ? 1f :  Mathf.Sign(botToTopDir.x);
        aeroDir = botToTopDir * sign;
        centerAero = (botAeroPos + topAeroPos) * 0.5f;
        centerMass = (leadingBot + leadingTop + trailingBot + trailingTop) * 0.25f;


        area = Mathv.SquareArea(leadingBot, leadingTop, trailingTop, trailingBot);
        span = (botAeroPos - topAeroPos).magnitude;
        botChord = (leadingBot - trailingBot).magnitude;
        topChord = (leadingTop - trailingTop).magnitude;
        chordDir = (leadingTop + leadingBot - trailingTop - trailingBot).normalized;
    }

    public Quad[] Split(float fraction)
    {
        Vector3 leadingFraction = Vector3.Lerp(leadingBot, leadingTop, fraction);
        Vector3 trailingFraction = Vector3.Lerp(trailingBot, trailingTop, fraction);
        return new Quad[] { 
            new Quad(tr, leadingFraction, leadingBot, trailingFraction, trailingBot),
            new Quad(tr, leadingTop, leadingFraction, trailingTop, trailingFraction),
        };
    }

    public static float Overlap(Quad main,Quad sub)
    {
        Vector3 dir = main.BotToTopDir(true);
        Vector3 subMaxDir = sub.BotAeroPos(true) - main.CenterAero(true);
        Vector3 subMinDir = sub.TopAeroPos(true) - main.CenterAero(true);
        float minSub = Vector3.Dot(subMaxDir, dir);
        float maxSub = Vector3.Dot(subMinDir, dir);
        minSub = Mathf.Max(minSub, -main.Span * 0.5f);
        maxSub = Mathf.Min(maxSub, main.Span * 0.5f);  
        return Mathf.Clamp01((maxSub - minSub) / main.Span);
    }

    public Transform tr;
    public const float liftLine = 0.75f;
    protected Vector3 leadingTop;
    protected Vector3 leadingBot;
    protected Vector3 trailingTop;
    protected Vector3 trailingBot;

    public Vector3 LeadingTop(bool world) { return world ? tr.TransformPoint(leadingTop) : leadingTop; }
    public Vector3 LeadingBot(bool world) { return world ? tr.TransformPoint(leadingBot) : leadingBot; }
    public Vector3 TrailingTop(bool world) { return world ? tr.TransformPoint(trailingTop) : trailingTop; }
    public Vector3 TrailingBot(bool world) { return world ? tr.TransformPoint(trailingBot) : trailingBot; }


    protected Vector3 botAeroPos;
    protected Vector3 topAeroPos;
    protected Vector3 botToTopDir;
    protected Vector3 aeroDir;
    protected Vector3 centerAero;
    protected Vector3 centerMass;

    public Vector3 BotAeroPos(bool world) { return world ? tr.TransformPoint(botAeroPos) : botAeroPos; }
    public Vector3 TopAeroPos(bool world) { return world ? tr.TransformPoint(topAeroPos) : topAeroPos; }
    public Vector3 BotToTopDir(bool world) { return world ? tr.TransformDirection(botToTopDir) : botToTopDir; }
    public Vector3 AeroDir(bool world) { return world ? tr.TransformDirection(aeroDir) : aeroDir; }
    public Vector3 CenterAero(bool world) { return world ? tr.TransformPoint(centerAero) : centerAero; }
    public Vector3 CenterMass(bool world) { return world ? tr.TransformPoint(centerMass) : centerMass; }

    protected float area;
    protected float span;
    protected float botChord;
    protected float topChord;
    protected Vector3 chordDir;

    public float Area { get { return area; } }
    public float Span { get { return span; } }
    public float BotChord { get { return botChord; } }
    public float TopChord { get { return topChord; } }
    public float MidChord { get { return (botChord + topChord) * 0.5f; } }
    public Vector3 ChordDir(bool world) { return world ? tr.TransformDirection(chordDir) : chordDir; }
#if UNITY_EDITOR
    public void Draw(Color fill, Color borders, bool drawAeroCenter)
    {
        Handles.color = borders;
        if (drawAeroCenter) Handles.DrawLine(BotAeroPos(true), TopAeroPos(true));
        Features.DrawControlHandles(LeadingBot(true), LeadingTop(true), TrailingTop(true), TrailingBot(true), fill, borders);
    }
#endif
}