using System;
using System.Collections.Generic;
using System.Threading;
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

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colorMap;

        public MapData(float[,] heightMap, Color[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }

    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private float noiseScale;
        [SerializeField] private int octaves;
        [Range(0, 1)] [SerializeField] private float persistance;
        [SerializeField] private float lacunarity;
        [SerializeField] private int seed;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float heightMultiplier;
        [SerializeField] private AnimationCurve meshHeightCurve;
        private Queue<MapThreadInfo<MapData>> _mapDataThreadQueue = new();
        private Queue<MapThreadInfo<MeshData>> _meshDataThreadQueue = new();

        public const int ChunkSize = 241;
        public bool autoUpdate;
        public TerrainType[] regions;
        [Range(0, 6)] public int levelOfDetail;

        public MapData GenerateMapData(Vector2 center)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(ChunkSize, ChunkSize, noiseScale, octaves, persistance,
                lacunarity, seed, center + offset);

            Color[] colorMap = new Color[ChunkSize * ChunkSize];
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight >= regions[i].height)
                        {
                            colorMap[y * ChunkSize + x] = regions[i].color;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return new MapData(noiseMap, colorMap);
        }

        public void DrawMap()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(mapData.colorMap, ChunkSize, ChunkSize));
        }

        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            void ThreadStart()
            {
                MapDataThread(center, callback);
            }

            new Thread(ThreadStart).Start();
        }

        private void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            MapData mapData = GenerateMapData(center);
            lock (_mapDataThreadQueue)
            {
                _mapDataThreadQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            void ThreadStart()
            {
                MeshDataThread(mapData, lod, callback);   
            }
            
            new Thread(ThreadStart).Start();
        }

        private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData =
                MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, lod);
            lock (_meshDataThreadQueue)
            {
                _meshDataThreadQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if (_mapDataThreadQueue.Count > 0)
            {
                for (int i = 0; i < _mapDataThreadQueue.Count; i++)
                {
                    MapThreadInfo<MapData> threadInfo = _mapDataThreadQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }

            if (_meshDataThreadQueue.Count > 0) 
            {
                for (int i = 0; i < _meshDataThreadQueue.Count; i++)
                {
                    MapThreadInfo<MeshData> threadInfo = _meshDataThreadQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
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

        struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }
    }
}