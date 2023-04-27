using System;
using System.Collections;
using System.Collections.Generic;
using Procedural_Geneneration;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class world_chunks : MonoBehaviour
{
    [SerializeField] private int viewDistance = 200;
    [SerializeField] private int chunkSize = 240;
    [SerializeField] private Transform playerPozition;
    [SerializeField] private GameObject ChunkPrefab;
    
    private int chunksInViewDistance;
    public Dictionary<Vector2, Chunk> VisitedChunks = new Dictionary<Vector2, Chunk>();
    public List<Chunk> OldChunks = new List<Chunk>();
    private void Start()
    {
        chunksInViewDistance =  Mathf.RoundToInt(viewDistance / chunkSize);
    }
    
    private void Update()
    {
        UpdateChunks();
    }

    void UpdateChunks()
    {
        Vector3 playerPos = playerPozition.position;
        int currentChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(playerPos.z / chunkSize);
        
        for (int yOffset = -chunksInViewDistance; yOffset <= chunksInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksInViewDistance; xOffset <= chunksInViewDistance; xOffset++)
            {
                Vector2 viewedChunk = new Vector2(xOffset + currentChunkX, yOffset + currentChunkY);
                //checking if our chunk is already generated
                if (VisitedChunks.ContainsKey(viewedChunk))
                {
                    // Debug.Log("been here");
                    VisitedChunks[viewedChunk].ChunkUpdate(playerPos, viewDistance);
                }
                else
                {
                    //adding the new chunk to the visited chunks
                    GameObject chunkInst = Instantiate(ChunkPrefab);
                    Chunk tmp = new Chunk(viewedChunk, chunkSize, playerPozition, chunkInst);
                    // ChunkColor tmp = new ChunkColor(viewedChink, chunkSize, playerPozition);
                    VisitedChunks.Add(viewedChunk, tmp);
                    OldChunks.Add(tmp);
                }

            }
        }

        for (int i = 0; i < OldChunks.Count; i++)
        {
            OldChunks[i].ChunkUpdate(playerPos, viewDistance);
        }
        // OldChunks.Clear();
        
    }
}

public class ChunkColor
{
    private Vector2 PVector2;
    private GameObject meshObj;
    
    public ChunkColor(Vector2 cord, int size, Transform playerPos)
    {
        // PVector3 = playerPos.TransformPoint(new Vector3(x * size, 0, y * size));
        // PVector3.y = 0;
        PVector2 = cord * size;
        Vector3 position = new Vector3(PVector2.x, 0, PVector2.y);
        Debug.Log(position);
        meshObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        meshObj.transform.position = position;
        meshObj.transform.localScale = Vector3.one * size / 10;


        Color debugColor = Random.ColorHSV();
        Renderer renderer = meshObj.GetComponent<Renderer>();
        renderer.material.color = debugColor;
    }

    public void chunkUpdate(Vector3 playerPos, int viewDistance)
    {
        Vector2 Pos2d = new Vector2(playerPos.x, playerPos.z);
        float distance = (Pos2d - PVector2).magnitude;
        meshObj.SetActive(!(distance > viewDistance));
    }
}

public class Chunk
{
    private Vector2 PVector2;
    private GameObject chunkInst;
    public Chunk(Vector2 cord, int size, Transform playerPos, GameObject chunkPrefab)
    {
        PVector2 = cord * size;
        Vector3 position = new Vector3(PVector2.x, 0, PVector2.y);
        Debug.Log(position);

        HeightMapVisiulizer heightMapVisiulizer = chunkPrefab.GetComponent<HeightMapVisiulizer>();
        heightMapVisiulizer.xMove = position.x * 100;
        heightMapVisiulizer.yMove = position.z * 100;
        chunkPrefab.transform.position = position;
        chunkPrefab.transform.localScale = Vector3.one;
        chunkInst = chunkPrefab;
    }
    
    public void ChunkUpdate(Vector3 playerPos, int viewDistance)
    {
        Vector2 Pos2d = new Vector2(playerPos.x, playerPos.z);
        float distance = (Pos2d - PVector2).magnitude;
        chunkInst.SetActive(!(distance > viewDistance));
    }
}
