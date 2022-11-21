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
        }

        [SerializeField] private int chunkSize = 241;
        [SerializeField] private float noiseScale;
        [SerializeField] private int octaves;
        [Range(0, 1)] [SerializeField] private float persistance;
        [SerializeField] private float lacunarity;
        [SerializeField] private int seed;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float heightMultiplier;
        [SerializeField] private AnimationCurve meshHeightCurve;

        public bool autoUpdate;
        public TerrainType[] regions;
        public DrawMode drawMode;
        [Range(0, 6)]
        public int levelOfDetail;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseScale, octaves, persistance,
                lacunarity, seed, offset);

            Color[] colorMap = new Color[chunkSize * chunkSize];
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorMap[y * chunkSize + x] = regions[i].color;
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
                display.DrawMapTexture(TextureGenerator.TextureFromColorMap(colorMap, chunkSize, chunkSize));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve, levelOfDetail),
                    TextureGenerator.TextureFromColorMap(colorMap, chunkSize, chunkSize));
            }
        }

        private void Start()
        {
            GenerateMap();
        }

        private void OnValidate()
        {
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