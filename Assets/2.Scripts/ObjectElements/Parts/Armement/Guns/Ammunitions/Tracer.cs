using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TracerProperties
{
    public Material material;

    public Color color;

    public float length;
    public float width;
    public float scatter;
}
[RequireComponent(typeof(Projectile))]
public class Tracer : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private TracerProperties properties;

    private float tracerOffset;

    public LineRenderer line;

    private float spawnTime = 0f;
    public void InitializeTracer(TracerProperties _properties)
    {
        properties = _properties;
        projectile = GetComponent<Projectile>();

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
        float updateDis = projectile.p.baseVelocity * Time.fixedDeltaTime;
        tracerOffset = Random.Range(0.2f, updateDis);
        spawnTime = Time.time;
        Update();
    }
    private void Update()
    {
        if (line && Time.timeScale != 0f)
        {
            Vector3 tailPos = projectile.Pos(Time.time - spawnTime) + projectile.tracerDir * tracerOffset;
            float length = properties.length * Time.timeScale + properties.width * 3f;
            Vector3 frontPos = tailPos + projectile.tracerDir * length;
            line.SetPosition(0, tailPos);
            line.SetPosition(1, frontPos);

            Vector3 midPos = Vector3.Lerp(tailPos, frontPos, Random.value);
            Vector3 midOffset = Random.insideUnitSphere * properties.scatter * properties.width * Time.timeScale;
            line.SetPosition(0, tailPos);
            line.SetPosition(1, midPos + midOffset / 2f);
            line.SetPosition(2, midPos - midOffset / 2f);
            line.SetPosition(3, frontPos);
        }
    }
}