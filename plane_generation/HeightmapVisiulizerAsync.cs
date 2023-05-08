using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
        
    public List<Vector2> neighbouringChunkSeedPos;
    private HeightmapGenerator _heightmapGenerator;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    // public struct NewMesh
    // {
    //     public Vector3[] NewVertexHeight;
    //     public Color32[] NewColor32s;
    // };
    //
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
                    colors[k] = GetPixelColor(terrainTexture, vertexY);
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
    
}