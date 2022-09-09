using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacement : MonoBehaviour
{
    public Texture2D texture;
    public float density = 0.1f;
    public Vegetation tree;

    public float width = 2048f;
    public float height = 2048f;

    private void Start()
    {
        RemoveAllTrees();
        CreateTrees();
    }

    public void RemoveAllTrees()
    {
        int childCount = transform.childCount;
        for(int i = childCount -1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i));
        }
    }

    public void CreateTrees()
    {
        for (int x = 0; x < width * density; x++)
        {
            for (int z = 0; z < height * density; z++)
            {
                Vector3 pos = new Vector3();
                pos.x = transform.position.x + x / density;
                pos.z = transform.position.z + z / density;
                Vector2 pos2d = new Vector2();
                pos2d.x = texture.width * x / density / width;
                pos2d.y = texture.height * z / density / height;
                Color px = texture.GetPixel((int)pos2d.x, (int)pos2d.y);
                if (px.a > Random.value) PlaceTree(pos);
            }
        }
    }
    public void PlaceTree(Vector3 basePos)
    {
        Vector3 pos = basePos + Random.insideUnitSphere * 0.5f / density;
        Instantiate(tree.gameObject, pos, Quaternion.identity, transform);
    }
}
