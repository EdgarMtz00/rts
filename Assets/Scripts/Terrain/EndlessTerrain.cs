using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class EndlessTerrain : MonoBehaviour
    {
        [SerializeField] private Transform viewer;
        [SerializeField] private Material material;
    
        public const float MaxViewDist = 450;
        public static Vector2 ViewerPosition;
    
        private int _chunkSize;
        private int _chunksVisibleInViewDist;
        private static MapGenerator _mapGenerator;

        private readonly Dictionary<Vector2, TerrainChunk> _generatedTerrainChunks = new();
        private readonly List<TerrainChunk> _visibleTerrainChunks = new();

        void Start()
        {
            _mapGenerator = FindObjectOfType<MapGenerator>();
            _chunkSize = MapGenerator.ChunkSize - 1;
            _chunksVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / _chunkSize);
        }

        void Update()
        {
            Vector3 position = viewer.position;
            ViewerPosition = new Vector2(position.x, position.z);
            UpdateVisibleChunks();
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
                        if (chunk.IsVisible())
                        {
                            _visibleTerrainChunks.Add(chunk);
                        }
                    }
                    else
                    {
                        Debug.Log(viewerChunkCoord.ToString());
                        _generatedTerrainChunks.Add(viewerChunkCoord, new TerrainChunk(viewerChunkCoord, _chunkSize, transform, material));
                    }
                }
            }
        }

        public class TerrainChunk
        {
            private readonly GameObject _meshObject;
            private Bounds _bounds;

            private MapData _mapData;

            private MeshRenderer _meshRenderer;
            private MeshFilter _meshFilter;

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                Vector2 position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);

                _meshObject = new GameObject("Terrain Chunk");
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
                _meshRenderer.material = material;
                _meshObject.transform.position = positionV3;
                _meshObject.transform.parent = parent;
            
                SetVisible(false);
            
                _mapGenerator.RequestMapData(OnMapDataReceived);
            }

            public void UpdateChunk()
            {
                float viewerDistanceToNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(ViewerPosition));
                bool visible = viewerDistanceToNearestEdge <= MaxViewDist;
                SetVisible(visible);
            }

            void OnMapDataReceived(MapData mapData)
            {
                _mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                _meshFilter.mesh = meshData.CreateMesh();
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
    }
}
