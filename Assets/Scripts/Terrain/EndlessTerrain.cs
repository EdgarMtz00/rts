using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class EndlessTerrain : MonoBehaviour
    {
        private const float Scale = 1;
        
        private const float MovementThresholdForChunkUpdate = 25f;
        private const float SqrMovementThreshold = MovementThresholdForChunkUpdate * MovementThresholdForChunkUpdate;
        
        [SerializeField] private Transform viewer;
        [SerializeField] private Material material;

        public static float MaxViewDist;
        public static Vector2 ViewerPosition;
        private Vector2 _previousViewerPosition;
        public LODInfo[] detailLevels;

        private int _chunkSize;
        private int _chunksVisibleInViewDist;
        private static MapGenerator _mapGenerator;

        private readonly Dictionary<Vector2, TerrainChunk> _generatedTerrainChunks = new();
        private static readonly List<TerrainChunk> _visibleTerrainChunks = new();

        void Start()
        {
            _mapGenerator = FindObjectOfType<MapGenerator>();

            MaxViewDist = detailLevels[^1].visibleDistanceThreshold;
            _chunkSize = MapGenerator.ChunkSize - 1;
            _chunksVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / _chunkSize);
            
            UpdateVisibleChunks();
        }

        void Update()
        {
            Vector3 position = viewer.position;
            ViewerPosition = new Vector2(position.x, position.z) / Scale;
            
            if ((ViewerPosition - _previousViewerPosition).sqrMagnitude > SqrMovementThreshold)
            {
                _previousViewerPosition = ViewerPosition;
                UpdateVisibleChunks();
            }
        }

        void UpdateVisibleChunks()
        {
            foreach (TerrainChunk t in _visibleTerrainChunks)
            {
                t.SetVisible(false);
            }

            _visibleTerrainChunks.Clear();

            int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / _chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.y / _chunkSize);

            for (int yOffset = -_chunksVisibleInViewDist; yOffset <= _chunksVisibleInViewDist; yOffset++)
            {
                for (int xOffset = -_chunksVisibleInViewDist; xOffset <= _chunksVisibleInViewDist; xOffset++)
                {
                    Vector2 viewerChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (_generatedTerrainChunks.ContainsKey(viewerChunkCoord))
                    {
                        TerrainChunk chunk = _generatedTerrainChunks[viewerChunkCoord];
                        chunk.UpdateChunk();
                    }
                    else
                    {
                        _generatedTerrainChunks.Add(viewerChunkCoord,
                            new TerrainChunk(viewerChunkCoord, _chunkSize, detailLevels, transform, material));
                    }
                }
            }
        }

        public class TerrainChunk
        {
            private readonly GameObject _meshObject;
            private Bounds _bounds;

            private MapData _mapData;
            private bool _mapDataReceived;
            private int _previousLODIndex = -1;

            private MeshRenderer _meshRenderer;
            private MeshFilter _meshFilter;
            private readonly LODInfo[] _detailLevels;
            private LODMesh[] _lodMeshes;
            
            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                this._detailLevels = detailLevels;
                Vector2 position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);

                _meshObject = new GameObject("Terrain Chunk");
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
                _meshRenderer.material = material;
                _meshObject.transform.position = positionV3 * Scale;
                _meshObject.transform.parent = parent;
                _meshObject.transform.localScale = Vector3.one * Scale; 

                SetVisible(true);

                _lodMeshes = new LODMesh[detailLevels.Length];
                for (int i = 0; i < detailLevels.Length; i++)
                {
                    _lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
                }

                _mapGenerator.RequestMapData(position, OnMapDataReceived);
            }

            public void UpdateChunk()
            {
                if (_mapDataReceived)
                {
                    float viewerDistanceToNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(ViewerPosition));
                    bool visible = viewerDistanceToNearestEdge <= MaxViewDist;

                    if (visible)
                    {
                        int lodIndex = 0;
                        for (int i = 0; i < _detailLevels.Length - 1; i++)
                        {
                            if (viewerDistanceToNearestEdge > _detailLevels[i].visibleDistanceThreshold)
                            {
                                lodIndex = i + 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                        if (lodIndex != _previousLODIndex)
                        {
                            LODMesh lodMesh = _lodMeshes[lodIndex];
                            if (lodMesh.HasMesh)
                            {
                                _previousLODIndex = lodIndex;
                                _meshFilter.mesh = lodMesh.Mesh;
                            }
                            else if (!lodMesh.HasRequestedMesh)
                            {
                                lodMesh.RequestMesh(_mapData);
                            }
                        }
                        
                        _visibleTerrainChunks.Add(this);
                    }

                    SetVisible(visible);
                }
            }

            void OnMapDataReceived(MapData mapData)
            {
                _mapData = mapData;
                _mapDataReceived = true;
                
                Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.ChunkSize,
                    MapGenerator.ChunkSize);
                _meshRenderer.material.mainTexture = texture;
                
                UpdateChunk();
            }

            public void SetVisible(bool visible)
            {
                _meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return _meshObject.activeSelf;
            }
        }

        class LODMesh
        {
            public Mesh Mesh;
            public bool HasRequestedMesh;
            public bool HasMesh;
            readonly int _lod;
            readonly Action _updateCallback;

            public LODMesh(int lod, Action updateCallback)
            {
                this._lod = lod;
                _updateCallback = updateCallback;
            }

            public void RequestMesh(MapData mapData)
            {
                HasRequestedMesh = true;
                _mapGenerator.RequestMeshData(mapData, _lod, OnMeshDataReceived);
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                Mesh = meshData.CreateMesh();
                HasMesh = true;

                _updateCallback();
            }
        }

        [Serializable]
        public struct LODInfo
        {
            public int lod;
            public float visibleDistanceThreshold;
        }
    }
}