using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    public Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks,VoxelData.WorldSizeInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private ChunkCoord playerChunkCoord;
    private ChunkCoord playerLastChunkCoord;
    private void Start()
    {
        Random.InitState(seed);
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight +2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }
    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if(!playerChunkCoord.Equals(playerLastChunkCoord))
        CheckViewDistance();
    }
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /*IMMUTABLE PASS*/

        //if outside World, return air.
        if (!IsVoxelInWorld(pos))
            return 0;
        //if bottom block of chunk, return bedrock.
        if (yPos == 0)
            return 1;

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(VoxelData.ChunkHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z),500, 0.25f));

        if (yPos <= terrainHeight)
            return 3;
        else
            return 0;

        

    }
    private void GenerateWorld()
    {
        for(var x =(VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistranceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistranceInChunks; x++)
        {
            for (var z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistranceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistranceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
        player.position = spawnPosition;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (var x = coord.x - VoxelData.ViewDistranceInChunks; x < coord.x + VoxelData.ViewDistranceInChunks; x++)
        {
            for (var z = coord.z - VoxelData.ViewDistranceInChunks; z < coord.z + VoxelData.ViewDistranceInChunks; z++)
            {
                if(IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if (chunks[x, z] == null)
                        CreateNewChunk(x,z);
                    else if(!chunks[x,z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
                for(var i=0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }  

        foreach(var c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
        }
    }
    private void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x,z));
    }
      
    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }
}



[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexute;
    public int frontFaceTexute;
    public int topFaceTexute;
    public int bottomFaceTexute;
    public int leftFaceTexute;
    public int rightFaceTexute;

    //Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexute;
            case 1:
                return frontFaceTexute;     
            case 2:
                return topFaceTexute;
            case 3:
                return bottomFaceTexute;
            case 4:
                return leftFaceTexute;
            case 5:
                return rightFaceTexute;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }

}
