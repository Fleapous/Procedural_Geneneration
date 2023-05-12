using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Threading;

public class HeightmapVisiulizerAsync : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private float scale;
    [SerializeField] public float xMove;
    [SerializeField] public float yMove;
    [SerializeField] private int octaves;
    [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private bool showHeight = true;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float heightScalar = 1;
    [SerializeField] private Textures textures;
    [SerializeField] private bool debugNoise;
    [SerializeField] private bool ShowBioms;
    [System.Serializable]
    public class Textures
    {
        public Color texture1;
        public Color texture2;
        public Color texture3;
        public Vector2 texture1Range;
        public Vector2 texture2Range;
        public Vector2 texture3Range;
    }

    private struct Seed
    {
        public Vector2 pos;
        public Color color;
    }
        
    public List<Vector2> neighbouringChunkSeedPos;
    private HeightmapGenerator _heightmapGenerator;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private static Dictionary<Vector3, Seed> seedCollection = new Dictionary<Vector3, Seed>();
    private static object _lock = new object();
    
    
    public async void HeightVizWrapperFunction()
    {
        _heightmapGenerator = GetComponent<HeightmapGenerator>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        Textures texture = textures;
        
        //get the size of the mesh
        Vector3 chunkPos = GetComponent<Transform>().position;
        int size = _meshFilter.sharedMesh.vertices.Length;
        Vector3[] vertices = _meshFilter.mesh.vertices;
        int n = (int)Mathf.Sqrt(size);
        Texture2D newTexture = new Texture2D(n, n);
        Color32[] color32s = new Color32[n * n];
        float[,] map = new float[n, n];

        Task<float[,]> task = Task.Run(() => _heightmapGenerator.MapGenerator(n, n, scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed));
        map = await task;
        
        //make it a texture
        Vector3[] newMeshHeight = new Vector3[n * n];
        Task<Vector3[]> taskTexture = Task.Run((() => MakeTexture(chunkPos, vertices,
            texture, map, n, n, color32s)));
        newMeshHeight = await taskTexture;

        _meshFilter.mesh.vertices = newMeshHeight;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
        if (newTexture)
        {
            newTexture.SetPixels32(color32s);
            newTexture.Apply();
            _meshRenderer.material.mainTexture = newTexture;
        }
        
    }

    private Vector3[] MakeTexture(Vector3 chunkPosition, Vector3[] newHeight, Textures terrainTexture,
        float[,] map, int height, int width, Color32[] colors)
    {
        int k = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float vertex = map[i, j];
                float vertexY = curve.Evaluate(vertex);
                if (showHeight)
                {
                    newHeight[k].y = vertexY * heightScalar;
                    // colors[k] = GetPixelColor(terrainTexture, vertexY);
                    colors[k] = GetClosestSeed(chunkPosition, new Vector3(i, 0f, j), 240, 1).color;
                    k++;
                }
            }
        }

        
        return newHeight;
    }
    
    private Color32 GetPixelColor(Textures textures, float heightNormal)
    {
        if (textures.texture1Range.x <= heightNormal && heightNormal <= textures.texture1Range.y)
            return textures.texture1;
        else if (textures.texture2Range.x < heightNormal && heightNormal < textures.texture2Range.y)
            return textures.texture2;
        else if (textures.texture3Range.x <= heightNormal && heightNormal <= textures.texture3Range.y)
            return textures.texture3;
        else
            return Color.white;
    }

    private Seed GetClosestSeed(Vector3 chunkPos, Vector3 vertexPosRelative, float chunkSize, int chunkInViewDist)
    {
        lock (_lock)
        {
            int seed = Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
            System.Random random = new System.Random(seed);
            
            Seed closestSeed;
            closestSeed.color = Color.magenta;
            closestSeed.pos = new Vector2(float.MaxValue, float.MaxValue);
            
            Vector3 vertexPosGlobal = chunkPos + vertexPosRelative;
            vertexPosGlobal.y = 0f;
            float closestDist = float.MaxValue;
            // int relativeChunkPosX = Mathf.RoundToInt(chunkPos.x / chunkSize);
            // int relativeChunkPosY = Mathf.RoundToInt(chunkPos.z / chunkSize);
            // Iterate over the search area and find the positions of the chunks
            for (int yOffset = -chunkInViewDist; yOffset <= chunkInViewDist; yOffset++)
            {
                for (int xOffset = -chunkInViewDist; xOffset <= chunkInViewDist; xOffset++)
                {
                    Vector2 viewedChunk = new Vector2(xOffset * chunkSize + chunkPos.x, yOffset * chunkSize + chunkPos.z);
                    //check if seed exists in position
                    if (seedCollection.ContainsKey(viewedChunk))
                    {
                        Vector3 seedPos = new Vector3(seedCollection[viewedChunk].pos.x, 0f, seedCollection[viewedChunk].pos.y);
                        float dist = Vector3.Distance(vertexPosGlobal, seedPos);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestSeed = seedCollection[viewedChunk];
                        }
                    }
                    else
                    {
                        Seed newSeed;
                        newSeed.pos = new Vector2(random.Next(0, 241) + viewedChunk.x, random.Next(0, 241) + viewedChunk.y);
                        newSeed.color = new Color32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), 255);
                        seedCollection.Add(viewedChunk, newSeed);

                        float dist = Vector3.Distance(vertexPosGlobal, new Vector3(newSeed.pos.x, 0f, newSeed.pos.y));
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestSeed = seedCollection[viewedChunk];
                        }
                    }
                }
            }
            return closestSeed;
        }
    }
}