#nullable enable

using UnityEngine;

public class Chunk {
    public Vector3Int position;
    public ChunkMesh mesh = new ChunkMesh();
    public BareMesh? bareMesh;
    public VoxelList voxels = new VoxelList();
};