using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public Vector3Int position;
    public ChunkMesh mesh = new ChunkMesh();
    long[] voxels = new long[512 * 2];

    public int biome;
    public float temperature;
    public float moisture;

    public Voxel this[int x, int y, int z]
    {
        get { return getVoxel(x + 8 * (y + z * 8)); }
        set { setVoxel(x + 8 * (y + z * 8), value); }
    }

    public Voxel this[int i]
    {
        get { return getVoxel(i); }
        set { setVoxel(i, value); }
    }

    private void setVoxel(int index, Voxel voxel){
        long computedVoxel = 0;
        long materialVoxel = 0;

        computedVoxel = Bits.setBits(0, 8, (int) Mathf.Round(voxel.density * 512), computedVoxel); // Density

        materialVoxel = Bits.setBits(0, 7, voxel.color.r, materialVoxel); // Color r
        materialVoxel = Bits.setBits(8, 15, voxel.color.g, materialVoxel); // Color g
        materialVoxel = Bits.setBits(16, 23, voxel.color.b, materialVoxel); // Color b
        materialVoxel = Bits.setBits(24, 35, voxel.material, materialVoxel); // material

        voxels[index * 2] = computedVoxel;
        voxels[index * 2 + 1] = materialVoxel;
    }

    private void setVoxel(int x, int y, int z, Voxel voxel){
        setVoxel(x + 8 * (y + z * 8), voxel);
    }

    private Voxel getVoxel(int index){
        long computedVoxel = voxels[index * 2];
        long materialVoxel = voxels[index * 2 + 1];

        Voxel voxel = new Voxel
        {
            density = (float) Bits.getBits(0, 8, computedVoxel) / 512,
            material = (int) Bits.getBits(24, 35, materialVoxel),
            color = new Color32
            {
                r = (byte) Bits.getBits(0, 7, materialVoxel),
                g = (byte) Bits.getBits(8, 15, materialVoxel),
                b = (byte) Bits.getBits(16, 23, materialVoxel),
                a = 0
            }
        };

        return voxel;
    }

    private Voxel getVoxel(int x, int y, int z){
        return getVoxel(x + 8 * (y + z * 8));
    }
};