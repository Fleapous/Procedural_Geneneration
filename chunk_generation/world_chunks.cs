using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class world_chunks : MonoBehaviour
{
    [SerializeField] private int viewDistance = 200;
    [SerializeField] private int chunkSize = 240;
    [SerializeField] private float noiseOffset;
    [SerializeField] private Transform playerPozition;
    [SerializeField] private GameObject ChunkPrefab;
    
    [Header("Biom Generation Visualization tools")]
        [SerializeField] private bool BiomVisualization;
        [Tooltip("seed of generation of Voronoi seeds")]
        [SerializeField] private int Seed;
        [SerializeField] private float sphereScale;
        [SerializeField] private float heightOffset;
        [SerializeField] private bool ShowPlane;
        [SerializeField] private bool KeepChunksVisable;
    
    private int chunksInViewDistance;
    public Dictionary<Vector2, Chunk> VisitedChunks = new Dictionary<Vector2, Chunk>();
    public List<Chunk> OldChunks = new List<Chunk>();
    
    //for debuging on chunk generation
    public Dictionary<Vector2, ChunkColor> VisitedChunksColor = new Dictionary<Vector2, ChunkColor>();
    public List<ChunkColor> OldChunksColor = new List<ChunkColor>();
    
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
                if (!BiomVisualization)
                {
                    if (VisitedChunks.ContainsKey(viewedChunk))
                    {
                        // Debug.Log("been here");
                        VisitedChunks[viewedChunk].ChunkUpdate(playerPos, viewDistance);
                    }
                    else
                    {
                        //adding the new chunk to the visited chunks
                        GameObject chunkInst = Instantiate(ChunkPrefab);
                        Chunk tmp = new Chunk(viewedChunk, chunkSize, playerPozition, chunkInst, noiseOffset);
                        VisitedChunks.Add(viewedChunk, tmp);
                        OldChunks.Add(tmp);
                    }
                }
                if (BiomVisualization)
                {
                    if (VisitedChunksColor.ContainsKey(viewedChunk))
                    {
                        // Debug.Log("been here");
                        VisitedChunksColor[viewedChunk].ChunkUpdate(playerPos, viewDistance, KeepChunksVisable);
                    }
                    else
                    {
                        //adding the new chunk to the visited chunks
                        ChunkColor tmp = new ChunkColor(viewedChunk, chunkSize, playerPozition, ShowPlane);
                        VisitedChunksColor.Add(viewedChunk, tmp);
                        OldChunksColor.Add(tmp);
                        
                        //creating a sphere on the randomly chosen position on each chunk
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = tmp.PickRandomPos(Seed, heightOffset);
                        sphere.transform.localScale = Vector3.one * sphereScale;
                        var renderer = sphere.GetComponent<Renderer>();
                        var material = new Material(Shader.Find("Standard"));
                        material.color = Color.red;
                        renderer.material = material;
                    }
                }

            }
        }

        if (!BiomVisualization)
        {
            for (int i = 0; i < OldChunks.Count; i++)
            {
                OldChunks[i].ChunkUpdate(playerPos, viewDistance);
            }
        }
        if (BiomVisualization)
        {
            for (int i = 0; i < OldChunksColor.Count; i++)
            {
                OldChunksColor[i].ChunkUpdate(playerPos, viewDistance, KeepChunksVisable);
            }
        }
    }
}

public class ChunkColor
{
    private Vector2 PVector2;
    private GameObject meshObj;
    
    public ChunkColor(Vector2 cord, int size, Transform playerPos, bool ShowPlane)
    {
        // PVector3 = playerPos.TransformPoint(new Vector3(x * size, 0, y * size));
        // PVector3.y = 0;
        PVector2 = cord * size;
        Vector3 position = new Vector3(PVector2.x, 0, PVector2.y);
        Debug.Log(position);
        meshObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        meshObj.transform.position = position;
        meshObj.transform.localScale = Vector3.one * size / 10;
        meshObj.AddComponent<MeshFilter>();


        Color debugColor = Random.ColorHSV();
        Renderer renderer = meshObj.GetComponent<Renderer>();
        renderer.enabled = true;
    }

    public void ChunkUpdate(Vector3 playerPos, int viewDistance, bool keepChunksVisible)
    {

        Vector2 Pos2d = new Vector2(playerPos.x, playerPos.z);
        float distance = (Pos2d - PVector2).magnitude;
        
        if(!keepChunksVisible)
            meshObj.SetActive(!(distance > viewDistance));
    }

    public Vector3 PickRandomPos(int seed, float heightOffset)
    {
        var meshFilter = meshObj.GetComponent<MeshFilter>();
        var mesh = meshFilter.mesh;
        
        // UnityEngine.Random.InitState(seed);
        int vertexIndex = Random.Range(0, mesh.vertexCount);
        Vector3 globalPos = meshFilter.transform.TransformPoint(mesh.vertices[vertexIndex]);
        globalPos.y = heightOffset;
        return globalPos;
        
    }
}

public class Chunk
{
    private Vector2 PVector2;
    private GameObject chunkInst;
    public Chunk(Vector2 cord, int size, Transform playerPos, GameObject chunkPrefab, float noiseOffset)
    {
        PVector2 = cord * size;
        Vector3 position = new Vector3(PVector2.x, 0, PVector2.y);

        HeightMapVisiulizer heightMapVisiulizer = chunkPrefab.GetComponent<HeightMapVisiulizer>();
        heightMapVisiulizer.xMove = position.x * noiseOffset;
        heightMapVisiulizer.yMove = position.z * noiseOffset;
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
