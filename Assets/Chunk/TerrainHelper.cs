using UnityEngine;

public static class TerrainHelper
{
    public static AnimationCurve heightCurve = new AnimationCurve(
        new Keyframe(0, 0), new Keyframe(0.5f, 0.8f), new Keyframe(1, 1));

    public static int CalculateHeight(int worldX, int worldZ, int seedX, int seedZ)
    {
        float height = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxAmplitude = 0;

        float heightScale = 128f;
        float noiseScale = 0.01f;
        int octaves = 4;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (worldX + seedX) * noiseScale * frequency;
            float sampleZ = (worldZ + seedZ) * noiseScale * frequency;

            // Introduce offset to simulate erosion-like features
            sampleX += Mathf.PerlinNoise(worldX * 0.005f, worldZ * 0.005f) * 10f;
            sampleZ += Mathf.PerlinNoise(worldX * 0.005f + 100, worldZ * 0.005f + 100) * 10f;

            float perlin = Mathf.PerlinNoise(sampleX, sampleZ);
            float billow = Mathf.Abs(perlin * 2 - 1); // Billow noise for rolling hills
            float ridged = 1 - billow; // Ridged noise for mountains
            float combinedNoise = (perlin + ridged) * 0.5f; // Hybrid multi-fractal

            height += combinedNoise * amplitude;
            maxAmplitude += amplitude;
            amplitude *= 0.5f;
            frequency *= 2;
        }

        height /= maxAmplitude;

        // Apply continental shaping
        float continentFactor = Mathf.PerlinNoise(worldX * 0.0002f, worldZ * 0.0002f);
        height *= Mathf.Clamp01(continentFactor * 1.5f);

        // Apply biome variation
        float biomeNoise = Mathf.PerlinNoise(worldX * 0.001f, worldZ * 0.001f);
        if (biomeNoise < 0.3f)
        {
            height *= 0.5f; // Lowlands
        }
        else if (biomeNoise > 0.7f)
        {
            height *= 1.5f; // Mountains
        }

        // Final height remap using a curve
        return Mathf.FloorToInt(heightCurve.Evaluate(height) * heightScale);
    }

}
