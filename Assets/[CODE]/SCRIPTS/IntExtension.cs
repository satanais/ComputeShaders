using UnityEngine;

public static class IntExtension
{
    public static int Snap(this int value, uint snapValue)
    {
        float division = 1.0f * value / snapValue;
        return Mathf.RoundToInt(division) * (int)snapValue;
    }
    public static int SnapUp(this int value, uint snapValue)
    {
        float division = 1.0f * value / snapValue;
        return Mathf.CeilToInt(division) * (int)snapValue;
    }
    public static int SnapDown(this int value, uint snapValue)
    {
        float division = 1.0f * value / snapValue;
        return Mathf.FloorToInt(division) * (int)snapValue;
    }
}
