using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Procedural_Geneneration
{
    public class HeightMapVisiulizer : MonoBehaviour
    {
        [SerializeField] private int seed;
        [SerializeField] private float scale;
        [SerializeField] private float xMove;
        [SerializeField] private float yMove;
        [SerializeField] private int octaves;
        [SerializeField] private float persistance;
        [SerializeField] private float lacunarity;
        [SerializeField] private bool showHeight;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float heightScalar = 1;
        
        private HeightmapGenerator _heightmapGenerator;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        

        private void Start()
        {
            _heightmapGenerator = GetComponent<HeightmapGenerator>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnValidate()
        {
            _heightmapGenerator = GetComponent<HeightmapGenerator>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            
            //get the size of the mesh
            int size = _meshFilter.sharedMesh.vertices.Length;
            int n = (int)Mathf.Sqrt(size);
            float[,] map = new float[n, n];
            
            //create the HeightMap
            map = _heightmapGenerator.MapGenerator(n, n, scale, octaves,
                persistance, lacunarity, xMove * 1/100, yMove * 1/100, seed);
            //make it a texture
            Texture2D mapTexture = MakeTexture(map, n, n);
            _meshRenderer.material.mainTexture = mapTexture;
        }
        
        private Texture2D MakeTexture(float[,] map, int height, int weight)
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
                        newHeight[k].y = curve.Evaluate(value) * heightScalar;
                        
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
}
