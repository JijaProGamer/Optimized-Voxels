using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Chunk {
    public Vector3Int position;
    public bool wasInit;

    int[] voxels;

    public void __init(){
        wasInit = true;
        voxels = new int[512 * 2];
    }

    public void setVoxel(int index, Voxel voxel){
        int computedVoxel = 0;
        int materialVoxel = 0;

        computedVoxel |= (int) (voxel.density * 128) & 0x7F;
        computedVoxel |= (voxel.material << 7) & 0xF80;

        materialVoxel |= ((voxel.color.r / 2));
        materialVoxel |= ((voxel.color.g / 2) << 8);
        materialVoxel |= ((voxel.color.b / 2) << 16); 

        voxels[index * 2] = computedVoxel;
        voxels[index * 2 + 1] = materialVoxel;
    }

    public void setVoxel(int x, int y, int z, Voxel voxel){
        setVoxel(x + 8 * (y + z * 8), voxel);
    }

    public Voxel getVoxel(int index){
        int computedVoxel = voxels[index * 2];
        int materialVoxel = voxels[index * 2 + 1];

        Voxel voxel = new Voxel
        {
            density = (float) (computedVoxel & 0x7F) / 128,
            material = (computedVoxel & 0xF80) >> 7,
            color = new Color32
            {
                r = (byte) ((materialVoxel & 0xFF) * 2),
                g = (byte) (((materialVoxel >> 8) & 0xFF) * 2),
                b = (byte) (((materialVoxel >> 16) & 0xFF) * 2),
                a = 0
            }
        };

        return voxel;
    }

    public Voxel getVoxel(int x, int y, int z){
        return getVoxel(x + 8 * (y + z * 8));
    }
};