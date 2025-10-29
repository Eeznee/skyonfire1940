using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SparSettings
{
    [Range(0f, 2f)] public float verticalThickness = 0.2f;
    [Range(0f, 0.5f)] public float horizontalThickness = 0.1f;
    [Range(0f, 1f)] public float chordPosition = 0.5f;
    [Range(-1f, 1f)] public float verticalOffset = 0f;
    [Range(0f, 5f)] public float rootExtension = 0.5f;
    [Range(0f, 1f)] public float tipShortening = 0f;
    [Range(0f, 0.9f)] public float interWingExtension = 0f;


    public SparSettings()
    {
        verticalThickness = 0.2f;
        horizontalThickness = 0.1f;
        chordPosition = 0.5f;
        verticalOffset = 0f;
        rootExtension = 0.5f;
        tipShortening = 0f;
        interWingExtension = 0f;
    }

    public void CreateBoxCollider(Wing wing)
    {
        SparPosition sparPosition = new SparPosition(wing, this);

        GameObject sparObject = new GameObject(wing.name + " spar " + chordPosition.ToString("0.00"));
        sparObject.layer = wing.DefaultLayer();
        sparObject.transform.parent = wing.transform;
        sparObject.transform.position = sparPosition.worldPos;
        sparObject.transform.rotation = sparPosition.worldRot;

        BoxCollider boxCollider = sparObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = sparPosition.boxSize;
        boxCollider.center = Vector3.zero;
        boxCollider.sharedMaterial = StaticReferences.Instance.aircraftPhysicMaterial;
    }
#if UNITY_EDITOR
    public void DrawGizmos(Wing wing)
    {
        SparPosition sparPosition = new SparPosition(wing, this);

        Handles.matrix = Matrix4x4.TRS(sparPosition.worldPos, sparPosition.worldRot, Vector3.one);

        Handles.color = new Color(0.2f, 1f, 0.2f, 0.5f); ;
        Handles.DrawWireCube(Vector3.zero, sparPosition.boxSize);

        Handles.matrix = Matrix4x4.identity;
    }
#endif
    private struct SparPosition
    {
        public Vector3 rootPos;
        public Vector3 topPos;
        public float length;

        public Vector3 worldPos;
        public Quaternion worldRot;

        public Vector3 up;
        public Vector3 forward;
        public Vector3 right;

        public Vector3 boxSize;


        public SparPosition(Wing wing, SparSettings sparSetting)
        {
            SurfaceQuad quad = wing.quad;

            float averageScale = Mathf.Abs(wing.shape.scale.y) * (wing.shape.tipScale * 0.01f + 1f) * 0.5f;
            float scaleFactor = averageScale / Mathf.Abs(wing.root.shape.scale.y);

            rootPos = Vector3.Lerp(quad.trailingBot.WorldPos, quad.leadingBot.WorldPos, sparSetting.chordPosition);
            topPos = Vector3.Lerp(quad.trailingTop.WorldPos, quad.leadingTop.WorldPos, sparSetting.chordPosition);

            if (wing.child != null)
            {
                topPos = Vector3.Lerp(topPos, rootPos, sparSetting.interWingExtension);
            }
            else
            {
                topPos = Vector3.Lerp(topPos, rootPos, sparSetting.tipShortening);
            }

            if (wing.parent != null)
            {
                SurfaceQuad parentQuad = wing.parent.quad;
                Vector3 parentRootPos = Vector3.Lerp(parentQuad.trailingBot.WorldPos, parentQuad.leadingBot.WorldPos, sparSetting.chordPosition);
                Vector3 parentTopPos = Vector3.Lerp(parentQuad.trailingTop.WorldPos, parentQuad.leadingTop.WorldPos, sparSetting.chordPosition);

                rootPos = Vector3.Lerp(parentTopPos, parentRootPos, sparSetting.interWingExtension);
            }
            else
            {
                rootPos = rootPos + (rootPos - topPos).normalized * sparSetting.rootExtension;
            }

            length = (topPos - rootPos).magnitude;

            right = (topPos - rootPos).normalized * wing.shape.left;
            up = Vector3.Cross(wing.quad.chordDir.WorldDir, right);
            forward = Vector3.Cross(right, up);

            worldPos = (rootPos + topPos) * 0.5f + up * sparSetting.verticalOffset * scaleFactor;
            worldRot = Quaternion.LookRotation(forward, up);

            boxSize = new Vector3(length, scaleFactor * sparSetting.verticalThickness, scaleFactor * sparSetting.horizontalThickness);
        }
    }
}
