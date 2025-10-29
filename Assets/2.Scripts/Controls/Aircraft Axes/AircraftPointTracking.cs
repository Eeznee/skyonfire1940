using UnityEngine;

public partial class AircraftInputs
{
    private Vector2 abstractControls = Vector3.zero;
    private Vector3 multipliers = Vector3.one;
    private Vector3 previousLocalTarget = Vector3.forward;

    const float rollYawAlpha = 30f;
    private const float maxMultiplierValue = 16f;
    private const float minMultiplierValue = 1f / 256f;
    private const int simSteps = 3;
    private const float timeStepMultiplier = 4.4f;
    private const int iterationsSteps = 3;

    public void PreciseTracking(Vector3 targetDirection, AircraftAxes forcedAxes, float maintainBankFactor)
    {
        targetDirection.Normalize();
        float offAngle = Vector3.Angle(aircraft.rb.linearVelocity, targetDirection);
        float offAngleFactor = Mathf.Clamp01(offAngle / rollYawAlpha);

        maxPitch = MaxPitchBrokenForTracking(Mathf.Sign(aircraft.controls.current.pitch));
        if (!float.IsNaN(forcedAxes.pitch)) forcedAxes.pitch = Mathf.Clamp(forcedAxes.pitch, -MaxPitch, MaxPitch);

        AircraftAxes controlsFound = AircraftAxes.zero;

        for (int i = 0; i < iterationsSteps; i++)
        {
            controlsFound = GetControls(abstractControls, MaxPitch, offAngleFactor, maintainBankFactor, !float.IsNaN(forcedAxes.roll));
            ApplyForcedAxis(ref controlsFound, forcedAxes);
            controlsFound.Clamp(MaxPitch);

            FlightConditions simConditions = FullSimulation(controlsFound, simSteps, Time.fixedDeltaTime * timeStepMultiplier);

            Vector3 localTarget = Quaternion.Inverse(simConditions.rotation) * targetDirection;

            bool verticalOvershoots = Mathf.Sign(previousLocalTarget.y * localTarget.y) == -1f;
            bool horizontalOvershoots = Mathf.Sign(previousLocalTarget.x * localTarget.x) == -1f;
            multipliers.y = Mathf.Clamp(multipliers.y * (verticalOvershoots ? 0.33f : 2f), minMultiplierValue, maxMultiplierValue);
            multipliers.x = Mathf.Clamp(multipliers.x * (horizontalOvershoots ? 0.33f : 2f), minMultiplierValue, maxMultiplierValue);

            abstractControls.y += multipliers.y * localTarget.y;
            abstractControls.x += multipliers.x * localTarget.x;
            abstractControls.ClampUnitSquare();

            previousLocalTarget = localTarget;
        }

        controlsFound = InterpolateForGrounded(controlsFound, previousLocalTarget);
        ApplyForcedAxis(ref controlsFound, forcedAxes);

        aircraft.controls.SetTargetInput(controlsFound, false, PitchCorrectionMode.Raw);
    }
    const float levelRollLimit = 0.3f;
    private AircraftAxes GetControls(Vector2 abstractControls, float maxPitch, float offAngleFactor, float maintainBankFactor, bool forceRoll)
    {
        AircraftAxes controls;
        controls.yaw = -abstractControls.x * (1f - offAngleFactor);

        float maintainBank = Mathf.Clamp(aircraft.data.rollRate.Get * -0.02f, -1f, 1f);
        float levelRoll = Mathf.Clamp(aircraft.tr.right.y * 5f + aircraft.data.rollRate.Get * -0.02f, -levelRollLimit, levelRollLimit);
        float passiveRoll = Mathf.Lerp(levelRoll, maintainBank, maintainBankFactor);

        float aggressiveRoll = abstractControls.x;

        controls.roll = Mathf.Lerp(passiveRoll, aggressiveRoll, offAngleFactor);

        //float slowDownRoll = aircraft.controls.axesSpeed.roll * Time.fixedDeltaTime * simSteps * timeStepMultiplier * Mathf.Lerp(1f, 0.4f, maintainBankFactor * (1f - offAngleFactor));
        //controls.roll = Mathf.MoveTowards(aircraft.controls.current.roll, controls.roll, slowDownRoll);

        controls.pitch = abstractControls.y * maxPitch;
        if (!forceRoll) controls.pitch *= Mathf.Clamp01(1.1f - 0.2f * Mathf.Abs(controls.roll) * offAngleFactor);

        return controls;
    }
    private AircraftAxes InterpolateForGrounded(AircraftAxes controlsFound, Vector3 localTarget)
    {
        float limitSpeed = aircraft.stats.MinTakeOffSpeedNoFlaps * 0.5f;
        if (aircraft.data.tas.Get > limitSpeed) return controlsFound;

        AircraftAxes final = new AircraftAxes();

        float vertical = Mathf.Clamp(localTarget.y * 5f, -1f, 1f);
        float horizontal = Mathf.Clamp(localTarget.x * 5f, -1f, 1f);

        if (aircraft.data.tas.Get < 0.2f)
        {
            final = new AircraftAxes(vertical, horizontal, -horizontal);
            return final;
        }

        float lerpFactor = aircraft.data.tas.Get / limitSpeed;

        final.pitch = Mathf.Lerp(vertical, controlsFound.pitch, lerpFactor);
        final.roll = Mathf.Lerp(horizontal, controlsFound.roll, lerpFactor);
        final.yaw = Mathf.Lerp(0f, controlsFound.yaw, lerpFactor);

        return final;
    }
    private static void ApplyForcedAxis(ref AircraftAxes applyTo, AircraftAxes forcedAxes)
    {
        if (!float.IsNaN(forcedAxes.pitch)) applyTo.pitch = forcedAxes.pitch;
        if (!float.IsNaN(forcedAxes.roll)) applyTo.roll = forcedAxes.roll;
        if (!float.IsNaN(forcedAxes.yaw)) applyTo.yaw = forcedAxes.yaw;
    }

    public FlightConditions FullSimulation(AircraftAxes controlsToTry, int steps, float dt)
    {
        FlightConditions flightConditions = new FlightConditions(aircraft, true);

        for (int simStep = 0; simStep < steps; simStep++)
        {
            flightConditions.SimulateControls(controlsToTry, false, dt);
            flightConditions.ApplyForces(dt);
        }

        return flightConditions;
    }
    /*
    public static bool ReachableControls(SofAircraft aircraft, AircraftAxes toReach, float timeDelta)
    {
        AircraftAxes end = aircraft.controls.SimulateControls(aircraft.data.ias.Get, aircraft.controls.current, toReach, timeDelta);

        if (end.pitch - toReach.pitch == 0f) return true;
        if (end.roll - toReach.roll == 0f) return true;
        if (end.yaw - toReach.yaw == 0f) return true;

        return false;
    }
    */
    static float TotalTimeToReachTarget(float angleOffTarget, float rollRate, float turnRate, Vector3 flattenedTarget, Vector3 transformUp)
    {
        float rollAmountToFace = Vector3.Angle(transformUp, flattenedTarget);

        float timeToReachTarget = 0f;
        timeToReachTarget += rollAmountToFace / rollRate * Mathf.Clamp01(angleOffTarget / 20f);
        timeToReachTarget += angleOffTarget / turnRate;

        return timeToReachTarget;
    }


    private float targetShiftAverage;
    private Vector3 previousTarget = Vector3.forward;

    const float simpleTrackingSpeedFactor = 1f;
    const float simpleTrackingPitchAdjustFactor = 0.05f;
    const float simpleTrackingBankToRollFactor = 0.005f;
    const float maxRoll = 0.8f;


    public AircraftAxes SimpleTrackingAxes(Vector3 targetDirection, float bankTarget, float passiveBankFactor, bool preventCrash)
    {
        float bankLimit = Mathf.Infinity;
        if (preventCrash) PreventGroundCrash(ref targetDirection, out bankLimit);

        AircraftAxes controlsFound = aircraft.controls.current;
        Vector3 currentLocalTarget = aircraft.tr.InverseTransformDirection(targetDirection);
        Vector3 localAngularVel = aircraft.tr.InverseTransformDirection(aircraft.rb.angularVelocity);
        float offAngleFactor = Vector3.Angle(aircraft.tr.forward, targetDirection) / 12f;

        //PITCH
        float directionShiftAngularSpeed = -aircraft.tr.InverseTransformDirection(targetDirection - previousTarget).y / Time.fixedDeltaTime;
        targetShiftAverage = Mathf.MoveTowards(targetShiftAverage, directionShiftAngularSpeed, Time.fixedDeltaTime);

        float targetAngularVel = -currentLocalTarget.y * simpleTrackingSpeedFactor + targetShiftAverage;
        controlsFound.pitch += (localAngularVel.x - targetAngularVel) * simpleTrackingPitchAdjustFactor;
        controlsFound.pitch = Mathf.Clamp(controlsFound.pitch, -MaxPitch, MaxPitch);

        //ROLL
        float currentBank = Mathv.Angle180(aircraft.transform.rotation.eulerAngles.z);

        bankTarget = Mathf.Clamp(bankTarget, -bankLimit, bankLimit);
        float passiveBank = Mathv.Angle180(Mathf.LerpAngle(0f, bankTarget - currentBank, passiveBankFactor));
        float passiveInput = Mathf.Clamp(Mathv.Angle180(-passiveBank) * simpleTrackingBankToRollFactor * 0.8f, -maxRoll, maxRoll);

        float activeBank = Mathf.Atan2(currentLocalTarget.x, currentLocalTarget.y) * Mathf.Rad2Deg;
        //if (Mathf.Abs(activeBank) > 120f && offAngleFactor < 1.5f) activeBank = 180f + activeBank;
        activeBank = Mathf.Clamp(Mathv.Angle180(activeBank - currentBank), -bankLimit, bankLimit);
        //if (Mathf.Abs(activeBank - currentBank) > 180f && bankLimit < 160f) activeBank = Mathf.MoveTowards(currentBank, activeBank, 170f);
        float activeInput = Mathf.Clamp(Mathv.Angle180(activeBank + currentBank) * simpleTrackingBankToRollFactor, -maxRoll, maxRoll);

        float combinedInput = Mathf.Lerp(activeInput, passiveInput, Mathf.Max(1f - offAngleFactor, 0.15f));


        //float combinedBank = Mathf.LerpAngle(activeBank, passiveBank, Mathf.Max(1f - offAngleFactor, 0.15f));
        //combinedBank = Mathf.Clamp(Mathv.Angle180(combinedBank), -bankLimit, bankLimit);

        controlsFound.roll = combinedInput;// Mathv.Angle180(combinedBank + currentBank) * simpleTrackingBankToRollFactor;

        previousTarget = targetDirection;

        return controlsFound;
    }
    public void SimpleTrackingPos(Vector3 targetPos, float bankTarget, float passiveBankFactor, bool preventCrash)
    {
        SimpleTracking(targetPos - aircraft.tr.position, bankTarget, passiveBankFactor, preventCrash);
    }
    public void SimpleTracking(Vector3 targetDirection, float bankTarget, float passiveBankFactor, bool preventCrash)
    {
        aircraft.controls.SetTargetInput(SimpleTrackingAxes(targetDirection, bankTarget, passiveBankFactor, preventCrash),false, PitchCorrectionMode.Raw);
    }

    const float frontDistance = 400f;
    private void PreventGroundCrash(ref Vector3 targetDir, out float bankLimit)
    {
        bankLimit = Mathf.Infinity;
        targetDir.Normalize();

        float timeToAvoidCrash = 1.5f * (90f / aircraft.stats.MaxTurnRate + 180f / aircraft.stats.RollRateCurrentSpeed());
        float distanceToAvoidCrash = timeToAvoidCrash * aircraft.data.gsp.Get;
        Vector3 targetPos = aircraft.transform.position + targetDir * distanceToAvoidCrash;

        if (targetPos.y > distanceToAvoidCrash + 200f) return;

        Vector3 frontPoint = aircraft.transform.position + aircraft.transform.forward * frontDistance;
        float maxMapHeight = Mathf.Max(GameManager.mapTool.HeightAtPoint(frontPoint), aircraft.data.altitude.Get - aircraft.data.relativeAltitude.Get);
        float minAltitudeAllowed = maxMapHeight + (maxMapHeight < 1f ? 20f : 40f); //min altitude tolerance is lower when flying above sea

        if (targetPos.y < minAltitudeAllowed)
        {
            if (aircraft.transform.position.y < minAltitudeAllowed) distanceToAvoidCrash *= Mathf.InverseLerp(maxMapHeight - 5f, minAltitudeAllowed, aircraft.transform.position.y);
            float alpha = Mathf.Acos((aircraft.transform.position.y - minAltitudeAllowed) / distanceToAvoidCrash);
            Vector3 towards = targetDir + Vector3.up * 10f;
            targetDir = Vector3.RotateTowards(Vector3.down, towards, alpha, 0f);

            //PreventBackwardLoop(ref targetDir);
        }
        //Debug.DrawRay(aircraft.tr.position, targetDir.normalized * distanceToAvoidCrash, Color.red);

        bankLimit = Mathf.Lerp(0f, 180f, Mathf.InverseLerp(maxMapHeight, minAltitudeAllowed + 40f, aircraft.transform.position.y + aircraft.data.vsp.Get * 2f));
        if (bankLimit >= 180f) bankLimit = Mathf.Infinity;
    }

    private void PreventBackwardLoop(ref Vector3 targetDir)
    {
        Vector3 flattenedDir = targetDir - Vector3.up * targetDir.y;
        Vector3 flattenedForward = aircraft.transform.forward - Vector3.up * aircraft.transform.forward.y;

        float dotProduct = Vector3.Dot(flattenedForward, flattenedDir);
        if (dotProduct < 0f)
        {
            Vector3 perpendicularDir = Vector3.Cross(Vector3.up, flattenedForward).normalized;
            perpendicularDir *= flattenedDir.magnitude;
            perpendicularDir *= Mathf.Sign(Vector3.Dot(flattenedDir, perpendicularDir));

            targetDir = perpendicularDir + Vector3.up * targetDir.y;
        }
    }
}
