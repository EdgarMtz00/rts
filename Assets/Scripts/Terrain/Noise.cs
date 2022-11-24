using System;
using UnityEngine;

namespace Terrain
{
    public class Noise
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance,
            float lacunarity, int seed, Vector2 offset)
        {
            float amplitude = 1;
            float frequency = 1;
            
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            
            float maxPossibleHeight = 0;
            
            float[,] noiseMap = new float[mapWidth, mapHeight];

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            if (scale <= 0)
            {
                scale = 0.0001f;
            }

            for (int i = 0; i < octaves; i++) {
                float offsetX = prng.Next (-100000, 100000) + offset.x;
                float offsetY = prng.Next (-100000, 100000) - offset.y;
                octaveOffsets [i] = new Vector2 (offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float noiseHeight = 0;
                    amplitude = 1;
                    frequency = 1;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                        float sampleY = (y- halfHeight + octaveOffsets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }   
            }

            return noiseMap;
        }
    }
}