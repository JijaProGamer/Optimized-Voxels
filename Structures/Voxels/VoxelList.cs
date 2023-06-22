using UnityEngine;

public class VoxelList
{
    long[] voxels = new long[512];

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

        computedVoxel = Bits.setBits(0, 12, (int) Mathf.Round(voxel.density * 8192), computedVoxel); // Density

        computedVoxel = Bits.setBits(13, 20, voxel.color.r, computedVoxel); // Color r
        computedVoxel = Bits.setBits(21, 28, voxel.color.g, computedVoxel); // Color g
        computedVoxel = Bits.setBits(29, 36, voxel.color.b, computedVoxel); // Color b
        computedVoxel = Bits.setBits(37, 46, voxel.material, computedVoxel); // material

        voxels[index] = computedVoxel;
    }

    private void setVoxel(int x, int y, int z, Voxel voxel){
        setVoxel(x + 8 * (y + z * 8), voxel);
    }

    private Voxel getVoxel(int index){
        long computedVoxel = voxels[index];

        Voxel voxel = new Voxel
        {
            density = (float) Bits.getBits(0, 12, computedVoxel) / 8192,
            material = (int) Bits.getBits(37, 46, computedVoxel),
            color = new Color32
            {
                r = (byte) Bits.getBits(13, 20, computedVoxel),
                g = (byte) Bits.getBits(21, 28, computedVoxel),
                b = (byte) Bits.getBits(29, 36, computedVoxel),
                a = 0
            }
        };

        return voxel;
    }

    private Voxel getVoxel(int x, int y, int z){
        return getVoxel(x + 8 * (y + z * 8));
    }
};
