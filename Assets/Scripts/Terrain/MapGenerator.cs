using System;
using UnityEngine;

namespace Terrain
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }

    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh
        };


        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private float noiseScale;
        [SerializeField] private int octaves;
        [Range(0, 1)] [SerializeField] private float persistance;
        [SerializeField] private float lacunarity;
        [SerializeField] private int seed;
        [SerializeField] private Vector2 offset;

        public bool autoUpdate;
        public TerrainType[] regions;
        public DrawMode drawMode;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, octaves, persistance,
                lacunarity, seed, offset);

            Color[] colorMap = new Color[mapHeight * mapWidth];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorMap[y * mapWidth + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = FindObjectOfType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawMapTexture(TextureGenerator.TextureFromHeighMap(noiseMap));
            }
            else if (drawMode == DrawMode.ColorMap)
            {
                display.DrawMapTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap),
                    TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
            }
        }

        private void OnValidate()
        {
            if (mapHeight < 1)
            {
                mapHeight = 1;
            }

            if (mapWidth < 1)
            {
                mapWidth = 1;
            }

            if (octaves < 0)
            {
                octaves = 0;
            }

            if (lacunarity < 1)
            {
                lacunarity = 1;
            }
        }
    }
}