using System;
using UnityEngine;

public static class NoiseEmitter
{
    public static event Action<Vector3, float> OnNoise;

    public static void Emit(Vector3 position, float radius)
    {
        OnNoise?.Invoke(position, radius);
    }
}
