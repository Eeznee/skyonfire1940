using UnityEngine;
using UnityEngine.UI;
public class UIGraphRenderer : Graphic
{
    public Vector2[] points;

    public float thickness = 5f;
    public float scale = 200f;

    protected override void Start()
    {
        base.Start();
        SetVerticesDirty();
    }
    public void UpdatePlot(Vector2[] newPoints)
    {
        points = newPoints;
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 previous = points[Mathf.Max(i - 1,0)];
            Vector2 next = points[Mathf.Min(i + 1,points.Length-1)];

            float angle = Mathf.Atan2(next.y - previous.y, next.x - previous.x) * Mathf.Rad2Deg + 90f;

            DrawPointVertices(points[i], angle, vh);
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            int p = i * 2;
            vh.AddTriangle(p + 0, p + 1, p + 3);
            vh.AddTriangle(p + 3, p + 2, p + 0);
        }
    }
    private void DrawPointVertices(Vector2 point, float angle, VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;

        vertex.color = color;


        vertex.position = new Vector3(point.x,point.y,0f) * scale;
        vertex.position += Quaternion.Euler(0f, 0f, angle) * new Vector3(-thickness * 0.5f, 0f);
        vh.AddVert(vertex);

        vertex.position = new Vector3(point.x, point.y, 0f) * scale;
        vertex.position += Quaternion.Euler(0f, 0f, angle) * new Vector3(thickness * 0.5f, 0f);
        vh.AddVert(vertex);
    }
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
