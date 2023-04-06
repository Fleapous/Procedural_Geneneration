using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Procedural_Geneneration
{
    public class HeightMapVisiulizer : MonoBehaviour
    {
        [SerializeField] private float scale;
        [SerializeField] private float xMove;
        [SerializeField] private float yMove;
        private HeightmapGenerator _heightmapGenerator;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        
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
            map = _heightmapGenerator.MapGenerator(n, n, scale, 0, 0, 0, xMove * 1/100, yMove * 1/100);
            
            //make it a texture
            Texture2D mapTexture = MakeTexture(map, n, n);
            _meshRenderer.material.mainTexture = mapTexture;
        }
        
        private Texture2D MakeTexture(float[,] map, int height, int weight)
        {
            Texture2D texture = new Texture2D(weight, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < weight; j++)
                {
                    float value = map[i, j];
                    Color color = new Color(value, value, value, 1f);
                    texture.SetPixel(j, i, color);
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
