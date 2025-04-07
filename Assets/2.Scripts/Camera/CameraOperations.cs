using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraOperations
{
    const float minUp = 15f;

    public static Vector3 Position(CameraLogic logic, Vector3 customPos, Vector3 offset)
    {
        Vector3 basePos = logic.BasePosition();
        if (logic.Adjustment == CamAdjustment.Position)
            basePos += logic.RelativeTransform().rotation * customPos;
        if (offset == Vector3.zero) return basePos;

        return GroundClipPos(basePos, offset);
    }
    public static Quaternion RotateRelative(ref Vector2 axis, Vector3 defaultForward, Vector3 defaultUp)
    {
        Vector2 inputs = CameraInputs.CameraInput();

        float yLimit = SofCamera.subCam.logic.BasePosMode == CamPos.FirstPerson ? 84f : 180f;
 
        axis.x += Mathf.Sign(90f - Mathf.Abs(axis.y)) * inputs.x;
        axis.y = Mathf.Clamp(axis.y + inputs.y, -yLimit, yLimit);
        Vector3 lookRotation = defaultForward;
        lookRotation = Quaternion.AngleAxis(axis.x, defaultUp) * lookRotation;
        lookRotation = Quaternion.AngleAxis(axis.y,Vector3.Cross(defaultUp,lookRotation)) * lookRotation;
        return Quaternion.LookRotation(lookRotation, defaultUp);
    }
    public static Quaternion RotateWorld(Quaternion rotation, Vector3 defaultForward, Vector3 defaultUp)
    {
        Vector2 inputs = CameraInputs.CameraInput();

        Vector3 up = defaultUp;
        float upAngle = Vector3.Angle(up, rotation.Forward());

        if (upAngle < minUp || upAngle > 180f - minUp) up = rotation.Up();
        else
        {
            bool backwards = Vector3.Angle(up, rotation.Up()) > 90f;
            if (!SofCamera.lookAround && Vector3.Angle(defaultForward, rotation.Forward()) < 90f) backwards = false;
            if (backwards) up = -up;
        }
        rotation = Quaternion.LookRotation(rotation * Vector3.forward, up);
        rotation *= Quaternion.Euler(Vector3.forward * inputs.x * Mathf.Cos(upAngle * Mathf.Deg2Rad));
        rotation *= Quaternion.Euler(Vector3.up * inputs.x * Mathf.Sin(upAngle * Mathf.Deg2Rad));
        rotation *= Quaternion.Euler(Vector3.right * inputs.y);
        return rotation;
    }

    public static Vector3 GroundClipPos(Vector3 basePos, Vector3 offset)
    {
        Vector3 finalPos = basePos + SofCamera.tr.forward * offset.z + SofCamera.tr.up * offset.y;

        if (GameManager.map.RelativeHeight(finalPos) < offset.magnitude + 50f) //Ground clipping
        {
            LayerMask mask = LayerMask.GetMask("Terrain", "Default", "Water");
            Vector3 direction = finalPos - basePos;
            float distance = direction.magnitude;
            direction /= distance;
            float raycastOffset = 5f * Mathv.SmoothStart((1f - Vector3.Dot(Vector3.up, -direction)), 4);
            if (Physics.Raycast(basePos, direction, out RaycastHit hit, distance + raycastOffset, mask))
                finalPos = hit.point - direction * raycastOffset;
        }
        return finalPos;
    }
}
