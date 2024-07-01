namespace Graphics.Core;

public class HashHelper
{
    public static int Combine(params int[] hashes)
    {
        int hash = 17;

        foreach (int h in hashes)
        {
            hash = hash * 31 + h;
        }

        return hash;
    }
}
