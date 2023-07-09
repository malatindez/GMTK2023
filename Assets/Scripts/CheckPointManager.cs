using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CheckPointManager
{
    public static int ActiveCheckpoint { get; set; }

    private static Vector3? _requestedSpawnPosition;

    public static bool TryGetSpawnPosition(out Vector3 position)
    {
        if (_requestedSpawnPosition == null)
        {
            position = default(Vector3);
            return false;
        }
        else
        {
            position = _requestedSpawnPosition.Value;
            return true;
        }
    }

    public static void RequestSpawn(Vector3 spawnPosition)
    {
        _requestedSpawnPosition = spawnPosition;
    }
}
