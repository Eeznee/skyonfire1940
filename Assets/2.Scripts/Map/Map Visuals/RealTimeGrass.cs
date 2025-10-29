using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeGrass : MonoBehaviour
{
    public int boardSize = 3;
    public float chunkSize = 50f;
    public float chunkDensity = 0.2f;
    public float groundedDepth = 0.5f;

    private int sides;
    private float maxCompDistanceDestroy;
    private float maxCompDistanceSpawn;
    private float fullGrassDistance;

    public GrassChunk chunkPrefab;
    private GrassChunk refChunk;
    private GrassChunk[][] chunks;

    private GrassChunk InstantiateChunk(Vector3 pos)
    {
        return Instantiate(chunkPrefab, pos, Quaternion.identity, transform);
    }
    void Awake()
    {
        maxCompDistanceDestroy = Mathv.SmoothStart((boardSize - 0.95f) * chunkSize, 2);
        maxCompDistanceSpawn = Mathv.SmoothStart((boardSize - 1.05f) * chunkSize, 2);
        fullGrassDistance = maxCompDistanceSpawn / 4f;
        chunkPrefab = InstantiateChunk(Vector3.zero);
        chunkPrefab.CreateChunk(chunkSize, chunkDensity);
        sides = 2 * boardSize + 1;
    }

    private void Initialize()
    {
        refChunk = InstantiateChunk(Player.tr.position);
        refChunk.WakeChunk();
        chunks = new GrassChunk[sides][];

        for (int x = 0; x < sides; x++)
        {
            chunks[x] = new GrassChunk[sides];
            for (int y = 0; y < sides; y++)
            {
                if (x == boardSize && y == boardSize) chunks[x][y] = refChunk;
                else chunks[x][y] = InstantiateChunk(refChunk.transform.position + Mathv.HexTilePosition(x, y, chunkSize, boardSize));
                chunks[x][y].WakeChunk();
            }
        }
    }

    private void AnimateRaising(GrassChunk chunk,float dis)
    {
        Vector3 pos = chunk.transform.position;
        pos.y = GameManager.mapTool.HeightAtPoint(pos);
        float factor = Mathf.InverseLerp(fullGrassDistance, maxCompDistanceSpawn, dis);
        chunk.transform.position = pos - Vector3.up * groundedDepth * factor;
    }

    void Update()
    {
        if (!refChunk)
        {
            if (Player.tr) Initialize();
            else return;
        }
        Vector3 camPos = SofCamera.tr.position;
        camPos.y = GameManager.mapTool.HeightAtPoint(camPos);

        int closestX = boardSize;
        int closestY = boardSize;
        float closestChunkDis = (refChunk.transform.position- camPos).sqrMagnitude;

        //Get furthest and closest chunk
        for (int x = 0; x < sides; x++)
        {
            for (int y = 0; y < sides; y++)
            {
                if (chunks[x][y] != null) //If it's a real chunk, 
                {
                    float dis = (chunks[x][y].transform.position - camPos).sqrMagnitude;
                    AnimateRaising(chunks[x][y], dis);

                    //Destroy if too far
                    if (dis > maxCompDistanceDestroy && !(x == boardSize && y == boardSize)) chunks[x][y].Remove();
                    else if (dis < closestChunkDis) //check if closest
                    {
                        closestX = x;
                        closestY = y;
                        closestChunkDis = dis;
                    }
                }
                else //If it's a chunk location, check distance to spawn in
                {
                    Vector3 pos = refChunk.transform.position + Mathv.HexTilePosition(x, y, chunkSize, boardSize);
                    float dis = (pos- camPos).sqrMagnitude;
                    if (dis < maxCompDistanceSpawn)
                    {
                        chunks[x][y] = InstantiateChunk(pos);
                        chunks[x][y].WakeChunk();
                    }
                }
            }
        }

        //Update the referance chunk
        if (closestX != boardSize || closestY != boardSize)
        {
            refChunk = chunks[closestX][closestY];
            GrassChunk[][] updated = new GrassChunk[chunks.Length][];

            for (int x = 0; x < sides; x++) updated[x] = new GrassChunk[sides];

            for (int x = 0; x < sides; x++)
            {
                for (int y = 0; y < sides; y++)
                {
                    int newX = x + boardSize - closestX;
                    int newY = y + boardSize - closestY;
                    if (newX >= 0 && newY >= 0 && newX < sides && newY < sides) updated[newX][newY] = chunks[x][y];
                    else if (chunks[x][y]) chunks[x][y].Remove();
                }
            }
            chunks = updated;
        }
    }
}
