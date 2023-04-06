using System;
using UnityEngine;

namespace Procedural_Geneneration
{
    public class HeightmapGenerator : MonoBehaviour
    {
        public float[,] MapGenerator(int height, int weight, float scale,
            float octaves, float persistance, float lacunarity, float xDrift, float yDrift)
        {
            float[,] map = new float[height, weight];
            float[,] mapNormalized = new float[height, weight];

            float maxFloat = float.MinValue;
            float minFloat = float.MaxValue;
            //row z
            for (int i = 0; i < height; i++)
            {
                //column x
                for (int j = 0; j < weight; j++)
                {
                    float X = (float)i / scale + xDrift;
                    float Y = (float)j / scale + yDrift;

                    float perlinNumber = Mathf.PerlinNoise(X, Y);

                    //for normalization
                    if (perlinNumber > maxFloat)
                    {
                        maxFloat = perlinNumber;
                    }else if (perlinNumber < minFloat)
                    {
                        minFloat = perlinNumber;
                    }
                    
                    //adding the value to map
                    map[i, j] = perlinNumber;
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < weight; j++)
                {
                    float originalValue = map[i, j];
                    mapNormalized[i, j] = (originalValue - minFloat) / (maxFloat - minFloat);
                }
            }
            
            //height map that normalized
            return mapNormalized; 
        }
    }
}
