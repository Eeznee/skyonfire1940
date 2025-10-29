using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TracerProperties
{
    public Material material;

    public Color color;

    public float width;
    public float scatter;
}
[RequireComponent(typeof(Projectile))]
public class Tracer : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private TracerProperties properties;

    public LineRenderer line;
    public void InitializeTracer(TracerProperties _properties, Projectile _projectile)
    {
        properties = _properties;
        projectile = _projectile;

        line = projectile.gameObject.AddComponent<LineRenderer>();
        line.startColor = properties.color;
        line.endColor = properties.color;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.material = properties.material;
        line.endWidth = line.startWidth = properties.width;
        line.numCapVertices = 4;
        line.positionCount = 4;
        line.useWorldSpace = true;
    }
    private void Start()
    {
        Update();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector3 head = TracerPos(-Time.fixedDeltaTime + Time.deltaTime * 0.7f);
        Vector3 tail = TracerPos(-Time.fixedDeltaTime);

        Vector3 midPos = Vector3.Lerp(tail, head, Random.value);
        Vector3 midOffset = properties.scatter * properties.width * Time.timeScale * Random.insideUnitSphere;

        line.SetPosition(0, tail);
        line.SetPosition(1, midPos + midOffset * 0.5f);
        line.SetPosition(2, midPos - midOffset * 0.5f);
        line.SetPosition(3, head);
    }

    public Vector3 TracerPos(float deltaTime)
    {
        return projectile.Pos(Time.time + deltaTime) - SofCamera.Velocity * deltaTime;
    }
}