using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class world_chunks : MonoBehaviour
{
    [SerializeField] int viewDistance = 200;
    private int chunksInViewDistance;
    [SerializeField] private int chunkSize = 240;
    [SerializeField] private Transform playerPozition;

    public Dictionary<Vector2, ChunkColor> VisitedChunks = new Dictionary<Vector2, ChunkColor>();
    public List<ChunkColor> OldChunks = new List<ChunkColor>();
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
                Vector2 viewedChink = new Vector2(xOffset + currentChunkX, yOffset + currentChunkY);
                //checking if our chunk is already generated
                if (VisitedChunks.ContainsKey(viewedChink))
                {
                    Debug.Log("been here");
                    VisitedChunks[viewedChink].chunkUpdate(playerPos, viewDistance);
                }
                else
                {
                    //adding the new chunk to the visited chunks
                    ChunkColor tmp = new ChunkColor(viewedChink, chunkSize, playerPozition);
                    VisitedChunks.Add(viewedChink, tmp);
                    OldChunks.Add(tmp);
                }

            }
        }

        for (int i = 0; i < OldChunks.Count; i++)
        {
            OldChunks[i].chunkUpdate(playerPos, viewDistance);
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
