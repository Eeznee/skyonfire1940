using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipTrailFX : MonoBehaviour
{
    public struct TrackedPos
    {
        public TrackedPos(ShipTrailFX trail)
        {
            Transform tr = trail.transform;
            posRelativeToMap = tr.position - GameManager.refPos;
            posRelativeToMap.y = height;

            waveDir = tr.right;
            waveDir.y = 0f;
            waveDir.Normalize();

            startOffset = trail.StartOffset;

            time = Time.time;

            waveTravelSpeed = trail.perpendicularTravelSpeed;
        }
        public Vector3 posRelativeToMap;
        public Vector3 waveDir;
        public float startOffset;
        public float time;
        private float waveTravelSpeed;
        public float Offset(float left)
        {
            return (startOffset + (Time.time - time) * waveTravelSpeed) * left;
        }
        public float PerlinOffset(float left)
        {
            float noise = Mathf.PerlinNoise(posRelativeToMap.x * 0.02f + left * 1000f, posRelativeToMap.z * 0.02f);

            float timeLerp = Mathf.Clamp01((Time.time - time) * 0.2f);
            return noise * timeLerp * 6f * left;
        }
        public Vector3 Position(float left)
        {
            Vector3 worldPos = posRelativeToMap + GameManager.refPos + (Offset(left) + PerlinOffset(left)) * waveDir;
            worldPos.y = height;
            return worldPos;
        }
    }
    private SofShip ship;
    private BoatMovement boatMovement;

    public LineRenderer rightTrail;
    public LineRenderer leftTrail;

    public int posAmount = 10;
    public float totalTime = 10f;
    public float perpendicularTravelSpeed = 2f;
    public List<TrackedPos> tracked;


    public const float height = 0.2f;

    public float StartOffset => ship.width * 0.2f;
    private bool BoatMoving => boatMovement.currentSpeed > 1f;

    private void Start()
    {
        ship = GetComponentInParent<SofShip>();
        boatMovement = GetComponentInParent<BoatMovement>();
        if (boatMovement == null || ship == null)
        {
            Destroy(gameObject);
            return;
        }

        ship.UpdateBounds();
        transform.localPosition = new Vector3(0f, 0f, ship.BowPoint - 0.5f);

        tracked = new List<TrackedPos>();
    }

    public Vector3[] Positions(float left)
    {
        if (tracked.Count == 0) return new Vector3[0];

        Vector3[] pos = new Vector3[tracked.Count + 1];

        for (int i = 0; i < tracked.Count; i++)
        {
            pos[i] = tracked[i].Position(left);
        }

        if (BoatMoving)
        {
            Vector3 sideVector = transform.right;
            sideVector.y = 0f;

            pos[^1] = new Vector3(transform.position.x, height, transform.position.z);
            pos[^1] += StartOffset * left * sideVector;
        }
        else
        {
            pos[^1] = pos[^2];
        }

        if (tracked.Count == posAmount)
        {
            float timeLerpT = (Time.time - tracked[^1].time) / (totalTime / posAmount);
            pos[0] = Vector3.Lerp(pos[0], pos[1], Mathf.Clamp01(timeLerpT));
        }

        return pos;
    }

    int cycleUpdate = 10;
    int cycle;

    private void LateUpdate()
    {
        cycle++;
        if (cycle % cycleUpdate != 0) return;

        if (BoatMoving)
        {
            if (tracked.Count == 0 || Time.time - tracked[^1].time > totalTime / posAmount)
            {
                RegisterPos();
            }
        }
        else if (tracked.Count > 0)
        {
            if (Time.time - tracked[^1].time > (posAmount + 1 - tracked.Count) * totalTime / posAmount)
            {
                tracked.RemoveAt(0);
            }
        }
        else return;

        Vector3[] rightPositions = Positions(1f);
        Vector3[] leftPositions = Positions(-1f);

        if (rightPositions.Length != rightTrail.positionCount)
        {
            rightTrail.positionCount = rightPositions.Length;
            leftTrail.positionCount = leftPositions.Length;
        }

        rightTrail.SetPositions(rightPositions);
        leftTrail.SetPositions(leftPositions);
    }

    private void RegisterPos()
    {
        if (tracked.Count == posAmount)
        {
            tracked.RemoveAt(0);
        }
        tracked.Add(new TrackedPos(this));
    }
}
