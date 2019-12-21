using System;
using UnityEngine;
using System.Collections.Generic;

namespace DefaultNamespace
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDistance = 450 ;
        public Transform viewer;
        public Material _material;

        public static Vector2 viewerPosition;
        private static MapGenerator mapGenerator;
        private int chunkSize;
        private int visibleChunks;
        
        Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();

        private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            visibleChunks = Mathf.RoundToInt(maxViewDistance / chunkSize);
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks()
        {
            foreach (TerrainChunk chunk in terrainChunksVisibleLastUpdate)
            {
                chunk.SetVisible(false);
            }
            
            terrainChunksVisibleLastUpdate.Clear();
            
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffset = -visibleChunks; yOffset < visibleChunks; yOffset++)
            {
                for (int xOffset = -visibleChunks; xOffset < visibleChunks; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (terrainChunkDict.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                        if (terrainChunkDict[viewedChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, _material));
                    }
                }
            }
        }

        public class TerrainChunk
        {
            private GameObject _meshObject;
            private Vector2 _position;
            private Bounds _bounds;

            private MapData _mapData;
            private MeshRenderer _meshRenderer;
            private MeshFilter _meshFilter;
            
            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                _position = coord * size;
                _bounds = new Bounds(_position, Vector2.one * size);
                Vector3 posV3 = new Vector3(_position.x, 0, _position.y);
                
                _meshObject = new GameObject("Terrain Chunk");
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
                _meshRenderer.material = material;
                
                _meshObject.transform.position = posV3;
                _meshObject.transform.parent = parent;
                SetVisible(false);
                
                mapGenerator.RequestMapData(OnMapDataReceived);
            }

            void OnMapDataReceived(MapData data)
            {
                mapGenerator.RequestMeshData(data, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData data)
            {
                _meshFilter.mesh = data.CreateMesh();
            }
            
            public void UpdateTerrainChunk()
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
                SetVisible(visible);
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