using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    private const float GlobalScale = 1f;
    //Variables/Params for Endless Mode
    public LODInfo[] detailLevelsArr;
    public static float MaxViewDist;

    private const float ViewerMoveDstDeltaForTerrUpdate = 25f;
    private const float SqrViewerMoveDstDeltaForTerrUpdate = ViewerMoveDstDeltaForTerrUpdate * ViewerMoveDstDeltaForTerrUpdate;
    public Transform viewerTransform;
    public static Vector2 ViewerPos;
    Vector2 oldViewerPos;
    
    private int _aSingleChunkSize;
    private int _chunksVisibleInDist;
    
    //Map material
    public Material meshMaterial;
    
    //Endless Mode Containers
    Dictionary<Vector2, TerrainChunk> _terrainChunksDict = new Dictionary<Vector2, TerrainChunk>();
    public static List<TerrainChunk> _terrainChunksVisibleSinceLastUpdate = new List<TerrainChunk>();
    
    //Static Variables/Params
    static NoiseMapGenerator _noiseMapGenerator;

    public void ForceLastVisibleChunksDictReset()
    {
        _terrainChunksVisibleSinceLastUpdate.Clear();
    }
    void Start()
    {
        _noiseMapGenerator = FindFirstObjectByType<NoiseMapGenerator>();                         //Find the NoiseMapGenerator Object
        MaxViewDist = detailLevelsArr[detailLevelsArr.Length - 1].visibleDstThreshold;
        _aSingleChunkSize = NoiseMapGenerator.mapChunkSize - 1;                                  //Set Initial Single Chunk Size (241)
        _chunksVisibleInDist = Mathf.RoundToInt(MaxViewDist / _aSingleChunkSize);             //Determine the number of Chunks to be visible in a given direction (see code below).
        UpdateVisibleChunks();                                                                  //Call once on start to create initially viewable chunks for user.
    }

    void Update()
    {
        ViewerPos = new Vector2(viewerTransform.position.x, viewerTransform.position.z) / GlobalScale;

        if ((oldViewerPos - ViewerPos).sqrMagnitude > SqrViewerMoveDstDeltaForTerrUpdate)
        {
            oldViewerPos = ViewerPos;
            UpdateVisibleChunks();
        }
    }

    public void UpdateVisibleChunks()
    {
        //Remove Chunks outside the user's view distance after each update so it is not overloaded.
        foreach (TerrainChunk chunk in _terrainChunksVisibleSinceLastUpdate)
        {
            chunk.SetVisible(false);
        }
        _terrainChunksVisibleSinceLastUpdate.Clear();
        
        //Load Chunks based on view distance of User. (View Distance should become a setting).
        int currChunkCoordX = Mathf.RoundToInt(ViewerPos.x/_aSingleChunkSize);
        int currChunkCoordY = Mathf.RoundToInt(ViewerPos.y/_aSingleChunkSize);

        for (int yOffset = -_chunksVisibleInDist; yOffset <= _chunksVisibleInDist; yOffset++)
        {
            for (int xOffset = -_chunksVisibleInDist; xOffset <= _chunksVisibleInDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currChunkCoordX + xOffset, currChunkCoordY + yOffset);

                if (_terrainChunksDict.ContainsKey(viewedChunkCoord))
                {
                    _terrainChunksDict[viewedChunkCoord].UpdateChunk();
                }
                else
                {
                    _terrainChunksDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, _aSingleChunkSize, detailLevelsArr,transform, meshMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        private const float DEFAULT_PLANE_SIZE = 10.0f; //default size of a plane primitive.
        
        //Game Object +  Components
        GameObject _meshObject;
        MeshRenderer _meshRenderer;
        MeshFilter _meshFilter;
        
        Vector2 position;
        Bounds _chunkBounds;
        
        LODInfo[] _detailLevels;
        MeshLOD[] _lodMeshes;
        
        DrawnMapData _mapData;
        private bool _mapDataRecved;
        private int _previousLODIndex = -1;
        
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parentTransform, Material material)
        {
            _detailLevels = detailLevels;
            
            position = coord * size;
            Vector3 posV3 = new Vector3(position.x, 0, position.y);
            _chunkBounds = new Bounds(position, (Vector2.one * size));

            _meshObject = new GameObject("TerrainChunk");
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshRenderer.material = material;
            
            _meshObject.transform.position = posV3 * GlobalScale;
            _meshObject.transform.localScale = Vector3.one * GlobalScale;
            
            //meshObject.transform.localScale = Vector3.one * size / DEFAULT_PLANE_SIZE;  //TODO: Implement logic for if this is in non-mesh mode
            _meshObject.transform.SetParent(parentTransform);
            SetVisible(false);
            
            //Create Mesh LODs
            _lodMeshes = new MeshLOD[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                _lodMeshes[i] = new MeshLOD(detailLevels[i].lod, UpdateChunk);
            }
            
            //Send Map Data request
            _noiseMapGenerator.RqstMapData(position, OnMapDataRecvd);
        } 
        
        //Threading Functions for TerrainChunk Generation
        void OnMapDataRecvd(DrawnMapData mapData)
        {
            //Validation for Receiving Map Data function call success NOT actual validation of data.
            print("Map data received");
            
            //Mesh based Logic
            this._mapData = mapData;
            _mapDataRecved = true;

            Texture2D tex = MapTextureGenerator.TextureFromColorMap(mapData.colorMap, NoiseMapGenerator.mapChunkSize, NoiseMapGenerator.mapChunkSize);
            _meshRenderer.material.mainTexture = tex;
            
            UpdateChunk();
        }
        
        //General Functions for TerrainChunk Class
        public void UpdateChunk()
        {
            //Recieve Check
            if (_mapDataRecved)
            {
                //Directional Checks for visible chunks & LOD levels
                float viewerDistFromNearestEdge = Mathf.Sqrt(_chunkBounds.SqrDistance(ViewerPos));
                bool visible = viewerDistFromNearestEdge <= MaxViewDist;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < _detailLevels.Length - 1; i++)
                    {
                        if (viewerDistFromNearestEdge > _detailLevels[i].visibleDstThreshold)
                            lodIndex = i + 1;
                        else
                            break;
                    }
                    
                    if (lodIndex != _previousLODIndex)
                    {
                        MeshLOD lodMesh = _lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            _previousLODIndex = lodIndex;
                            _meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRqstedMesh)
                        {
                            lodMesh.RqstMeshData(_mapData);
                        }
                    }
                    _terrainChunksVisibleSinceLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
            else
            {
                
            }
        }
        
        //Visibility functions for all terrain chunks.
        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }
    }
    
    //Endless Mode LOD
    class MeshLOD
    {
        public Mesh mesh;
        public bool hasRqstedMesh;
        public bool hasMesh;
        int _lod;                                           //Might need to use this when in endless mode, and the "levelofDetail" variable when in single chunk mode.

        Action updateCallback;
        
        public MeshLOD(int lod, Action updateCallback)
        {
            this._lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecvd(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RqstMeshData(DrawnMapData mapData)
        {
            hasRqstedMesh = true;
            _noiseMapGenerator.RqstMeshData(mapData, _lod, OnMeshDataRecvd);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}
