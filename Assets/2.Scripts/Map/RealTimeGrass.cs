using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeGrass : MonoBehaviour
{
    public int boardSize = 3;
    public float chunkSize = 50f;
    public float chunkDensity = 0.2f;
    public float groundedDepth = 0.5f;

    int sides;
    float maxCompDistanceDestroy;
    float maxCompDistanceSpawn;
    float fullGrassDistance;

    public GrassChunk chunkPrefab;
    GrassChunk refChunk;
    GrassChunk[][] chunks;


    void Start()
    {
        maxCompDistanceDestroy = Mathf.Pow((boardSize - 0.95f) * chunkSize, 2);
        maxCompDistanceSpawn = Mathf.Pow((boardSize - 1.05f) * chunkSize, 2);
        fullGrassDistance = maxCompDistanceSpawn / 4f;
        chunkPrefab = Instantiate(chunkPrefab, GameManager.player.tr.position, Quaternion.identity);
        chunkPrefab.CreateChunk(chunkSize, chunkDensity);
        sides = 2 * boardSize + 1;

        Initialize();
    }

    private void Initialize()
    {
        refChunk = Instantiate(chunkPrefab, GameManager.player.tr.position, Quaternion.identity);
        chunks = new GrassChunk[sides][];

        for (int x = 0; x < sides; x++)
        {
            chunks[x] = new GrassChunk[sides];
            for (int y = 0; y < sides; y++)
            {
                if (x == boardSize && y == boardSize) chunks[x][y] = refChunk;
                else chunks[x][y] = Instantiate(chunkPrefab, refChunk.transform.position + Mathv.HexTilePosition(x, y, chunkSize, boardSize), Quaternion.identity);
                chunks[x][y].WakeChunk();
            }
        }
    }

    private void AnimateRaising(GrassChunk chunk,float dis)
    {
        Vector3 pos = chunk.transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        float factor = Mathf.InverseLerp(fullGrassDistance, maxCompDistanceSpawn, dis);
        chunk.transform.position = pos - Vector3.up * groundedDepth * factor;
    }

    void Update()
    {
        Vector3 playerPos = GameManager.player.tr ? GameManager.player.tr.position : Camera.main.transform.position;
        playerPos.y = GameManager.map.HeightAtPoint(playerPos);

        int closestX = boardSize;
        int closestY = boardSize;
        float closestChunkDis = (refChunk.transform.position- playerPos).sqrMagnitude;

        //Get furthest and closest chunk
        for (int x = 0; x < sides; x++)
        {
            for (int y = 0; y < sides; y++)
            {
                if (chunks[x][y] != null) //If it's a real chunk, 
                {
                    float dis = (chunks[x][y].transform.position - playerPos).sqrMagnitude;
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
                    float dis = (pos- playerPos).sqrMagnitude;
                    if (dis < maxCompDistanceSpawn)
                    {
                        chunks[x][y] = Instantiate(chunkPrefab, pos, Quaternion.identity);
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
