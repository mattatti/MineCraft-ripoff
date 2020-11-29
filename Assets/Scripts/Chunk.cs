﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Chunk {
	public ChunkCoord coord;
	private GameObject chunkObject; 
    private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private int vertexIndex = 0;
    private readonly List<Vector3> vertices = new List<Vector3>();
	private readonly List<int> triangles = new List<int>();
	private readonly List<Vector2> uvs = new List<Vector2>();
	private byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
	private readonly World world;
 
	public Chunk(ChunkCoord _coord, World _world)
    {
		coord = _coord;
		world = _world;
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshRenderer.material = world.material;
		chunkObject.transform.SetParent(world.transform);
		chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
		chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

		PopulateVoxelMap();
		CreateMeshData();
		CreateMesh();
	}
	void PopulateVoxelMap() {
		for (var y = 0; y < VoxelData.ChunkHeight; y++) {
			for (var x = 0; x < VoxelData.ChunkWidth; x++) {
				for (var z = 0; z < VoxelData.ChunkWidth; z++) {
					voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + Position);
				}
			}
		}
	}

	void CreateMeshData() {
		for (var y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (var x = 0; x < VoxelData.ChunkWidth; x++)
			{
				for (var z = 0; z < VoxelData.ChunkWidth; z++)
				{
					if(world.blockTypes[voxelMap[x,y,z]].isSolid)
					AddVoxelDataToChunk(new Vector3(x, y, z));
				}
			}
		}
	}

	public bool IsActive
    {
		get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
    }

	public Vector3 Position 
    {
		get { return chunkObject.transform.position; }		 
	}

	bool IsVoxelInChunk(int x,int y, int z)
    {
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
			return false;
		else
		return true;
	}

	bool CheckVoxel(Vector3 pos)
    {
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);
		if (!IsVoxelInChunk(x,y,z) )
			return world.blockTypes[world.GetVoxel(pos + Position)].isSolid;
		return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

	void AddVoxelDataToChunk(Vector3 pos) {
		for (int p = 0; p < 6; p++) {
			if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
			{

				byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
				vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

				AddTexture(world.blockTypes[blockID].GetTextureID(p));

				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex+1);
				triangles.Add(vertexIndex+2);
				triangles.Add(vertexIndex+2);
				triangles.Add(vertexIndex+1);
				triangles.Add(vertexIndex+3);
				vertexIndex += 4;
			}
		}
	}
	void CreateMesh()
    {
		Mesh mesh = new Mesh
		{
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			uv = uvs.ToArray()
		};
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}
	void AddTexture(int textureID)
    {
		float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
		float x = textureID - y * VoxelData.TextureAtlasSizeInBlocks;
		x *= VoxelData.NormalizeBlockTextureSize;
		y *= VoxelData.NormalizeBlockTextureSize;

		y = 1f - y - VoxelData.NormalizeBlockTextureSize;

		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + VoxelData.NormalizeBlockTextureSize));
		uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y));
		uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y + VoxelData.NormalizeBlockTextureSize));
	}

}

public class ChunkCoord
{
	public int x;
	public int z;

	public ChunkCoord(int _x, int _z)
    {
		x = _x;
		z = _z;
    }

	public bool Equals(ChunkCoord other)
    {
		if (other == null)
			return false;
		else if (other.x == x && other.z == z)
			return true;
		else return false;
    }
} 
