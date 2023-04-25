using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Procedural_Geneneration
{
    public class HeightmapGenerator : MonoBehaviour
    {
        public float[,] MapGenerator(int height, int width, float scale,
            int octaves, float persistance, float lacunarity, float xDrift, float yDrift, int seed)
        {
            System.Random rng = new System.Random (seed);

            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = rng.Next(-100000, 100000) + xDrift;
                float offsetY = rng.Next(-100000, 100000) + yDrift;
                octaveOffsets [i] = new Vector2 (offsetX, offsetY);
            }

            float halfHeight = height / 2;
            float halfWidth = width / 2;
            
            float[,] map = new float[height, width];
            float[,] mapNormalized = new float[height, width];

            float maxFloat = float.MinValue;
            float minFloat = float.MaxValue;
            //row z
            for (int i = 0; i < height; i++)
            {
                //column x
                for (int j = 0; j < width; j++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    //octaves
                    for (int k = 0; k < octaves; k++)
                    {
                        // float X = (float)i / scale * frequency + octaveOffsets[k].x;
                        // float Y = (float)j / scale * frequency + octaveOffsets[k].y;
                        
                        float X = (j - halfWidth + octaveOffsets[k].x) / scale * frequency;
                        float Y = (i - halfHeight + octaveOffsets[k].y) / scale * frequency;
                    
                        float perlinNumber = Mathf.PerlinNoise(X, Y) * 2 - 1;
                        noiseHeight += perlinNumber * amplitude;
                    
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    
                    //for normalization
                    if (noiseHeight > maxFloat)
                    {
                        maxFloat = noiseHeight;
                    }else if (noiseHeight < minFloat)
                    {
                        minFloat = noiseHeight;
                    }
                    
                    //adding the value to map
                    map[i, j] = noiseHeight;
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float originalValue = map[i, j];
                    mapNormalized[i, j] = (originalValue - minFloat) / (maxFloat - minFloat);
                    // mapNormalized[i, j] = Mathf.InverseLerp (minFloat, maxFloat, map[i, j]);
                }
            }
            
            //height map that normalized
            return mapNormalized; 
        }
        
    }
}
