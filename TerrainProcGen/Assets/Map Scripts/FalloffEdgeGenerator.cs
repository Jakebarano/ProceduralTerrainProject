using UnityEngine;

public static class FalloffEdgeGenerator
{
    public static float[,] GenerateFOMap(int size)
    {
        float[,] map = new float[size, size];
        
        //loop per size index x and y (i and j)
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                //Set up the values to be between -1 and 1 inclusive.
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;
                
                //Determine is largest value (closer to -1 or 1 is considered "largest")
                float val = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                
                //Set value at index
                map[i, j] = EvalulateVal(val);
                
            }
        }
        return map;
    }

    static float EvalulateVal(float value)
    {
        float a = 3;
        float b = 2.3f;
        
        return Mathf.Pow(value, a)/(Mathf.Pow(value, a) + Mathf.Pow(b-b * value, a));
    }
}
