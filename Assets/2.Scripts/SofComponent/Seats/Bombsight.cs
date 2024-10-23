using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sof Components/Crew Seats/Bombsight")]
public class Bombsight : SofComponent
{
    public float fov = 12f;
    public float maxAngle = 80f;
    public float minAngle = -20f;
    public float rotationSpeed = 10f;
    public float maxSideAngle = 30f;
    public float maxYawInput = 0.3f;


    private float relativeAltitude = 1000f;
    public float verticalAngle { get; private set; }
    public float horizontalAngle { get; private set; }

    //0 is manual, 1 is free and 2 is auto
    public int viewMode { get; private set; }
    public int amountSelection { get; private set; }
    public int intervalSelection { get; private set; }
    public bool releaseSequence { get; private set; }



    readonly int[] dropAmounts = { 1, 2, 4, 8, 16, 100 };
    readonly float[] dropIntervals = { 0f, 0.05f, 0.1f, 0.2f, 0.5f };

    private float targetAltitude = 0f;



    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        verticalAngle = 60f;
        horizontalAngle = 0f;
        viewMode = 0;
        amountSelection = 0;
        intervalSelection = 2;
    }
    private void Update()
    {
        if (viewMode == 2) 
            verticalAngle = Mathf.Atan(Mathf.Tan(verticalAngle * Mathf.Deg2Rad) - Time.deltaTime * data.gsp.Get / relativeAltitude) * Mathf.Rad2Deg;
    }
    public void Operate()
    {
        relativeAltitude = data.altitude.Get - targetAltitude;

        SetAngles();
        RotateBombsight();

        if (tr.forward.y >= 0f) targetAltitude = 0f;
        else
        {
            float distanceForward = (targetAltitude - data.altitude.Get) / tr.forward.y;
            Vector3 point = tr.position + tr.forward * distanceForward;
            targetAltitude = GameManager.map.HeightAtPoint(point);
        }
    }
    public void CopyLeaderBombsight(Bombsight leadSight)
    {
        intervalSelection = leadSight.intervalSelection;
        amountSelection = leadSight.amountSelection;

        if (aircraft.hydraulics.bombBay.stateInput != leadSight.aircraft.hydraulics.bombBay.stateInput) aircraft.hydraulics.SetBombBay();
        if (leadSight.releaseSequence && !releaseSequence) StartReleaseSequence();
    }

    public float HitAnglePrediction()
    {
        float timeToFall = Mathf.Sqrt(2f / -Physics.gravity.y * relativeAltitude);
        float horizontalDistance = timeToFall * data.gsp.Get;
        return Mathf.Atan(horizontalDistance / relativeAltitude) * Mathf.Rad2Deg;
    }

    private void SetAngles()
    {
        Vector2 cameraInput = PlayerActions.bomber.Rotate.ReadValue<Vector2>();

        switch (viewMode)
        {
            case 0:
                verticalAngle = HitAnglePrediction();
                horizontalAngle = 0f;
                break;
            case 1:
                verticalAngle += cameraInput.y * Time.deltaTime * rotationSpeed;
                horizontalAngle += cameraInput.x * Time.deltaTime * rotationSpeed;
                break;
            case 2:
                verticalAngle += cameraInput.y * Time.deltaTime * rotationSpeed;
                horizontalAngle = 0f;
                break;
        }

        verticalAngle = Mathf.Clamp(verticalAngle, minAngle, maxAngle);
        horizontalAngle = Mathf.Clamp(horizontalAngle, -maxSideAngle, maxSideAngle);
    }
    private void RotateBombsight()
    {
        tr.localRotation = Quaternion.identity;
        Quaternion stabilisedRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(aircraft.transform.forward, Vector3.up));
        tr.rotation = Quaternion.RotateTowards(tr.rotation, stabilisedRot, 15f);
        tr.Rotate(Vector3.left * (verticalAngle - 90f));
        tr.Rotate(Vector3.up * horizontalAngle);
    }

    public void ToggleInterval()
    {
        intervalSelection = (intervalSelection + 1) % dropIntervals.Length;
    }
    public void ToggleAmount()
    {
        amountSelection = (amountSelection + 1) % dropAmounts.Length;
    }
    public void ToggleMode()
    {
        viewMode = (viewMode + 1) % 3;
    }


    public void StartReleaseSequence()
    {
        StartCoroutine(ReleaseSequence());
    }

    IEnumerator ReleaseSequence()
    {
        if (releaseSequence) yield break;

        releaseSequence = true;
        int totalBombs = dropAmounts[amountSelection];
        int bombsReleased = 0;
        float interval = dropIntervals[intervalSelection];
        float releaseCounter = interval * totalBombs;
        float counter = 0f;

        while (counter <= releaseCounter)
        {
            while ((interval == 0f || bombsReleased <= counter / interval) && bombsReleased < totalBombs)
            {
                aircraft.armament.DropBomb();
                bombsReleased++;
            }
            counter += Time.deltaTime;
            yield return null;
        }

        releaseSequence = false;
        yield return null;
    }
}
