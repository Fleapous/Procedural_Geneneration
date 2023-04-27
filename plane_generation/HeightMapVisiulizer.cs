using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


public class HeightMapVisiulizer : MonoBehaviour
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
    
    private HeightmapGenerator _heightmapGenerator;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private void Start()
    {
        if(!debugNoise)
            HeightVizWrapperFunction();
    }

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

    private void OnValidate()
    {
        if (debugNoise)
            HeightVizWrapperFunction();
        
        
    }

    private void HeightVizWrapperFunction()
    {
        
        _heightmapGenerator = GetComponent<HeightmapGenerator>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        Textures texture = textures;
        //get the size of the mesh
        int size = _meshFilter.sharedMesh.vertices.Length;
        int n = (int)Mathf.Sqrt(size);
        float[,] map = new float[n, n];

        //create the HeightMap
        map = _heightmapGenerator.MapGenerator(n, n, scale, octaves,
            persistance, lacunarity, xMove * 1 / 100, yMove * 1 / 100, seed);
        //make it a texture
        Texture2D mapTexture = MakeTexture(map, n, n, texture);
        _meshRenderer.material.mainTexture = mapTexture;
    }

    private Texture2D MakeTexture(float[,] map, int height, int weight, Textures textures)
    {
        Vector3[] newHeight = _meshFilter.mesh.vertices;
        Texture2D texture = new Texture2D(weight, height);
        int k = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < weight; j++)
            {
                float value = map[i, j];
                if (showHeight)
                {
                    float heightNormal = curve.Evaluate(value);
                    newHeight[k].y = heightNormal * heightScalar;

                    if (textures.texture1Range.x <= heightNormal && heightNormal <= textures.texture1Range.y)
                        texture.SetPixel(j, i, textures.texture1);
                    else if(textures.texture2Range.x < heightNormal && heightNormal < textures.texture2Range.y)
                        texture.SetPixel(j, i, textures.texture2);
                    else if(textures.texture3Range.x <= heightNormal && heightNormal <= textures.texture3Range.y)
                        texture.SetPixel(j, i, textures.texture3);
                    k++;
                }else
                {
                    Color color = new Color(value, value, value, 1f);
                    texture.SetPixel(j, i, color);

                    newHeight[k].y = 0;
                    k++;
                }
            }
        }
        _meshFilter.mesh.vertices = newHeight;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
        
        texture.Apply();
        return texture;
    }
}

