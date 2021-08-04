using Unity.Mathematics;
using UnityEngine;

public static class MousePositionUtils
{
    public static float3 MouseToTerrainPositionECS()
    {
        float3 position = float3.zero;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 10000, LayerMask.GetMask("Level")))
        {
            position = info.point;
        }
        return position;
    }

    public static Vector3 MouseToTerrainPosition()
    {
        Vector3 position = Vector3.zero;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 10000, LayerMask.GetMask("Level")))
        {
            position = info.point;
        }
        return position;
    }

    public static RaycastHit CameraRay()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit info))
        {
            return info;
        }
        return new RaycastHit();
    }
}
