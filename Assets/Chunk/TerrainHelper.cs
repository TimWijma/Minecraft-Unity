using UnityEngine;

public static class TerrainHelper
{
    public static int CalculateHeight(int worldX, int worldZ, int seedX, int seedZ)
    {
        float height = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxAmplitude = 0;

        float heightScale = 128f;
        float noiseScale = 0.01f;
        float octaves = 4;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (worldX + seedX) * noiseScale * frequency;
            float sampleZ = (worldZ + seedZ) * noiseScale * frequency;

            height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;

            maxAmplitude += amplitude;
            amplitude *= 0.5f;
            frequency *= 2;
        }

        return Mathf.FloorToInt(height / maxAmplitude * heightScale);
    }

}
