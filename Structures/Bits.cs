public class Bits {
    static public long setBits(int start, int end, int value, long old)
    {
        long mask = (((1L << (end - start + 1)) - 1) << start);
        long clearedOld = old & ~mask;
        long result = clearedOld | ((long) value << start);

        return result;
    }

    static public long getBits(int start, int end, long value)
    {
        int mask = ((1 << (end - start + 1)) - 1) << start;
        long result = (value & mask) >> start;

        return result;
    }
};