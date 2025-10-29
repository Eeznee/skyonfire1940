using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SurfaceQuad
{
    public struct Position
    {
        private SurfaceQuad quad;
        private Vector3 localValue;
        private Vector3 posRelativeToAircraft;

        public Vector3 LocalPos => localValue;
        public Vector3 WorldPos => quad.tr.TransformPoint(localValue);

        

        public Vector3 Pos(FlightConditions flightConditions)
        {
            return flightConditions.position + flightConditions.rotation * posRelativeToAircraft;
        }

        public Position(SurfaceQuad _quad, Vector3 _localValue)
        {
            quad = _quad;
            localValue = _localValue;
            posRelativeToAircraft = quad.airframe.localPos + quad.airframe.localRot * localValue;
        }
    }
    public struct Direction
    {

        private SurfaceQuad quad;
        private Vector3 localValue;
        private Vector3 dirRelativeToAircraft;

        public Vector3 LocalDir => localValue;
        public Vector3 WorldDir => quad.tr.TransformDirection(localValue);

        public Vector3 Dir(FlightConditions flightConditions)
        {
            return flightConditions.rotation * dirRelativeToAircraft;
        }

        public Direction(SurfaceQuad _quad, Vector3 _localValue)
        {
            quad = _quad;
            localValue = _localValue;
            dirRelativeToAircraft = quad.airframe.localRot * localValue;
        }
    }


    public SofAirframe airframe { get; private set; }
    public Transform tr { get; private set; }

    public float area { get; private set; }
    public float span { get; private set; }
    public float botChord { get; private set; }
    public float topChord { get; private set; }
    public float midChord { get; private set; }


    public Position leadingTop;
    public Position leadingBot;
    public Position trailingTop;
    public Position trailingBot;

    public Position botAeroPos;
    public Position topAeroPos;

    public Direction botToTopDir;
    public Direction aeroDir;
    public Direction chordDir;
    public Direction upDir;

    public Position centerAero;
    public Position centerMass;

    public Direction controlSurfaceAxis;


    public SurfaceQuad(SofAirframe _airframe,Vector3 _leadingTop, Vector3 _leadingBot, Vector3 _trailingTop, Vector3 _trailingBot)
    {
        airframe = _airframe;
        tr = _airframe.transform;

        leadingTop = new Position(this, _leadingTop);
        leadingBot = new Position(this, _leadingBot);
        trailingTop = new Position(this, _trailingTop);
        trailingBot = new Position(this, _trailingBot);

        Vector3 _botAeroPos = Vector3.Lerp(_trailingBot, _leadingBot, Aerodynamics.liftLine);
        Vector3 _topAeroPos = Vector3.Lerp(_trailingTop, _leadingTop, Aerodynamics.liftLine);
        Vector3 _botToTopDir = (_topAeroPos - _botAeroPos).normalized;

        botAeroPos = new Position(this, _botAeroPos);
        topAeroPos = new Position(this, _topAeroPos);
        botToTopDir = new Direction(this, _botToTopDir);

        float sign = Mathf.Abs(_botToTopDir.x) < 0.1f ? 1f :  Mathf.Sign(_botToTopDir.x);
        aeroDir = new Direction(this, _botToTopDir * sign);
        chordDir = new Direction(this, (_leadingTop + _leadingBot - _trailingTop - _trailingBot).normalized);
        upDir = new Direction(this, Vector3.Cross(chordDir.LocalDir, aeroDir.LocalDir).normalized);

        centerAero = new Position(this, (_botAeroPos + _topAeroPos) * 0.5f);
        centerMass = new Position(this, (_leadingBot + _leadingTop + _trailingBot + _trailingTop) * 0.25f);

        controlSurfaceAxis = new Direction(this, (_leadingTop - _leadingBot).normalized);

        area = Mathv.SquareArea(_leadingBot, _leadingTop, _trailingTop, _trailingBot);
        span = (_botAeroPos - _topAeroPos).magnitude;
        botChord = (_leadingBot - _trailingBot).magnitude;
        topChord = (_leadingTop - _trailingTop).magnitude;
        midChord = (botChord + topChord) * 0.5f;
    }


    public static float Overlap(SurfaceQuad main, SurfaceQuad sub)
    {
        Vector3 dir = main.botToTopDir.WorldDir;
        Vector3 subMaxDir = sub.botAeroPos.WorldPos - main.centerAero.WorldPos;
        Vector3 subMinDir = sub.topAeroPos.WorldPos - main.centerAero.WorldPos;
        float minSub = Vector3.Dot(subMaxDir, dir);
        float maxSub = Vector3.Dot(subMinDir, dir);
        minSub = Mathf.Max(minSub, -main.span * 0.5f);
        maxSub = Mathf.Min(maxSub, main.span * 0.5f);
        return Mathf.Clamp01((maxSub - minSub) / main.span);
    }
    public SurfaceQuad[] Split(float fraction)
    {
        Vector3 leadingFraction = Vector3.Lerp(leadingBot.LocalPos, leadingTop.LocalPos, fraction);
        Vector3 trailingFraction = Vector3.Lerp(trailingBot.LocalPos, trailingTop.LocalPos, fraction);
        return new SurfaceQuad[] {
            new SurfaceQuad(airframe, leadingFraction, leadingBot.LocalPos, trailingFraction, trailingBot.LocalPos),
            new SurfaceQuad(airframe, leadingTop.LocalPos, leadingFraction, trailingTop.LocalPos, trailingFraction),
        };
    }

#if UNITY_EDITOR
    public void Draw(Color fill, Color borders, bool drawAeroCenter)
    {
        Handles.color = borders;
        if (drawAeroCenter) Handles.DrawLine(botAeroPos.WorldPos, topAeroPos.WorldPos);
        DrawControlHandles(leadingBot.WorldPos, leadingTop.WorldPos, trailingTop.WorldPos, trailingBot.WorldPos, fill, borders);
    }
    public static void DrawControlHandles(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color face, Color outline)
    {
        Vector3[] v = new Vector3[4];
        v[0] = A;
        v[1] = B;
        v[2] = C;
        v[3] = D;
        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(v, face, outline);
    }
#endif
}