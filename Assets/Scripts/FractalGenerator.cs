using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public enum NoiseType
{
    None,
    Perlin,
    Simplex,
    Cellular,
    Test
}

[System.Serializable]
public struct FractalGenerator
{

    public int seed;
    public NoiseType type;
    public bool tiled;
    public bool inverted;
    [Range(0, 1)]
    public float amplitude;
    public float frequency;
    [Range(1, 16)]
    public int octaves;
    [Range(0, 1)]
    public float gain;
    public float lacunarity;
    public float min;
    public float max;

    public FractalGenerator(int seed = 0)
    {
        this.seed = seed;
        type = NoiseType.None;
        tiled = false;
        inverted = false;

        amplitude = 1;
        frequency = 2;

        octaves = 1;

        gain = 0.5f;
        lacunarity = 2;
        min = 0;
        max = 1;
    }

    public float Generate(float x, float y)
    {
        float f = frequency;
        float amp = amplitude;

        float total = 0;
        float _max = 0;
        
        float X = x + seed;
        float Y = y + seed;

        for (int o = 0; o < octaves; o++)
        {
            float value = 0;

            switch (type)
            {
                case NoiseType.Perlin:
                    if (tiled) value = (noise.pnoise(
                        new float2(X, Y) * f,
                        new float2(1, 1) * (frequency + seed)
                        ) + 1.0f) * 0.5f;
                    else value = (noise.cnoise(new float2(X, Y) * f) + 1.0f) * 0.5f;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Simplex:
                    if (tiled) value = (noise.psrnoise(
                        new float2(X, Y) * f,
                        new float2(1, 1) * (frequency + seed)
                        ) + 1.0f) * 0.5f;
                    else value = (noise.snoise(new float2(X, Y) * f) + 1.0f) * 0.5f;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Cellular:
                    if (tiled)
                    {
                        float W = 1;
                        float H = 1;
                        float wx = W - x;
                        float hy = H - y;
                        float fxy = noise.cellular(new float2(X, Y) * f).x * wx * hy;
                        float fxwy = noise.cellular(new float2(X - W, Y) * f).x * X * hy;
                        float fxwyh = noise.cellular(new float2(X - W, Y - H) * f).x * X * Y;
                        float fxyh = noise.cellular(new float2(X, Y - H) * f).x * wx * Y;

                        value = (fxy + fxwy + fxwyh + fxyh) / (W * H);
                    }
                    else value = noise.cellular(new float2(X, Y) * f).x;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Test:
                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                default:
                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
            }
            //total += amp * (Perlin.Noise((seed + x) * f, (seed + y) * f) + 1) / 2.0f;
            _max += amp;
            amp *= gain;
            f *= lacunarity;
        }
        
        float v = (amplitude == 0.0f || _max == 0.0f) ? 0 : (total / _max) * amplitude;
        return min + v * (max - min);
    }

    public float Generate(float x, float y, float z)
    {
        float f = frequency;
        float amp = amplitude;

        float total = 0;
        float _max = 0;
        
        float X = x + seed;
        float Y = y + seed;
        float Z = z + seed;

        for (int o = 0; o < octaves; o++)
        {
            float value = 0;

            switch (type)
            {
                case NoiseType.Perlin:
                    if (tiled) value = (noise.pnoise(
                        new float3(X, Y, Z) * f,
                        new float3(1, 1, 1) * (frequency + seed)
                        ) + 1.0f) * 0.5f;
                    else value = (noise.cnoise(new float3(X, Y, Z) * f) + 1.0f) * 0.5f;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Simplex:
                    if (tiled)
                    {
                        float W = 1;
                        float H = 1;
                        float D = 1;
                        float wx = W - X;
                        float hy = H - Y;
                        float dz = D - Z;
                        float fxyz = (noise.snoise(new float3(X, Y, Z) * f) + 1.0f) * 0.5f * wx * hy * dz;
                        float fxwyz = (noise.snoise(new float3(X - W, Y, Z) * f) + 1.0f) *0.5f * X * hy * dz;
                        float fxwyhz = (noise.snoise(new float3(X - W, Y - H, Z) * f) + 1.0f) *0.5f * X * Y * dz;
                        float fxyhz = (noise.snoise(new float3(X, Y - H, Z) * f) + 1.0f) *0.5f * wx * Y * dz;

                        float fxyzd = (noise.snoise(new float3(X, Y, Z - D) * f) + 1.0f) *0.5f * wx * hy * Z;
                        float fxwyzd = (noise.snoise(new float3(X - W, Y, Z - D) * f) + 1.0f) *0.5f * X * hy * Z;
                        float fxwyhzd = (noise.snoise(new float3(X - W, Y - H, Z - D) * f) + 1.0f) *0.5f * X * Y * Z;
                        float fxyhzd = (noise.snoise(new float3(X, Y - H, Z - D) * f) + 1.0f) *0.5f * wx * Y * Z;

                        value = (fxyz + fxwyz + fxwyhz + fxyhz + fxyzd + fxwyzd + fxwyhzd + fxyhzd) / (W * H * D);
                    }
                    else value = (noise.snoise(new float3(X, Y, Z) * f) + 1.0f) * 0.5f;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Cellular:
                    if (tiled)
                    {
                        float W = 1;
                        float H = 1;
                        float D = 1;
                        float wx = W - X;
                        float hy = H - Y;
                        float dz = D - Z;
                        float fxyz = noise.cellular(new float3(X, Y, Z) * f).x * wx * hy * dz;
                        float fxwyz = noise.cellular(new float3(X - W, Y, Z) * f).x * X * hy * dz;
                        float fxwyhz = noise.cellular(new float3(X - W, Y - H, Z) * f).x * X * Y * dz;
                        float fxyhz = noise.cellular(new float3(X, Y - H, Z) * f).x * wx * Y * dz;

                        float fxyzd = noise.cellular(new float3(X, Y, Z - D) * f).x * wx * hy * Z;
                        float fxwyzd = noise.cellular(new float3(X - W, Y, Z - D) * f).x * X * hy * Z;
                        float fxwyhzd = noise.cellular(new float3(X - W, Y - H, Z - D) * f).x * X * Y * Z;
                        float fxyhzd = noise.cellular(new float3(X, Y - H, Z - D) * f).x * wx * Y * Z;

                        value = (fxyz + fxwyz + fxwyhz + fxyhz + fxyzd + fxwyzd + fxwyhzd + fxyhzd) / (W * H * D);
                    }
                    else value = noise.cellular(new float3(X, Y, Z) * f).x;

                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                case NoiseType.Test:
                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
                default:
                    if (inverted) total += amp * (1.0f - value);
                    else total += amp * value;

                    break;
            }
            //total += amp * (Perlin.Noise((seed + x) * f, (seed + y) * f, (seed + z) * f) + 1) / 2.0f;
            _max += amp;
            amp *= gain;
            f *= lacunarity;
        }
        float v = (amplitude == 0.0f || _max == 0.0f) ? 0 : (total / _max) * amplitude;
        return min + v * (max - min);
    }

    public FractalGenerator Copy()
    {
        FractalGenerator copy = new FractalGenerator();
        copy.seed = seed;
        copy.type = type;
        copy.tiled = tiled;
        copy.inverted = inverted;
        copy.amplitude = amplitude;
        copy.frequency = frequency;
        copy.octaves = octaves;
        copy.gain = gain;
        copy.lacunarity = lacunarity;
        copy.min = min;
        copy.max = max;
        return copy;
    }

}
