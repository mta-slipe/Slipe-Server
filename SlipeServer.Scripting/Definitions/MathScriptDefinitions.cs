using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class MathScriptDefinitions
{
    [ScriptFunctionDefinition("getDistanceBetweenPoints2D")]
    public float GetDistanceBetweenPoints2D(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    [ScriptFunctionDefinition("getDistanceBetweenPoints3D")]
    public float GetDistanceBetweenPoints3D(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    [ScriptFunctionDefinition("getEasingValue")]
    public float GetEasingValue(float progress, string easingType, float easingPeriod = 0, float easingAmplitude = 0, float easingOvershoot = 1.70158f)
    {
        return CalculateEasing(progress, easingType, easingPeriod, easingAmplitude, easingOvershoot);
    }

    [ScriptFunctionDefinition("interpolateBetween")]
    public Vector3 InterpolateBetween(float x1, float y1, float z1, float x2, float y2, float z2, float progress, string easingType, float easingPeriod = 0, float easingAmplitude = 0, float easingOvershoot = 1.70158f)
    {
        float t = CalculateEasing(progress, easingType, easingPeriod, easingAmplitude, easingOvershoot);
        return new Vector3(x1 + (x2 - x1) * t, y1 + (y2 - y1) * t, z1 + (z2 - z1) * t);
    }

    private static float CalculateEasing(float t, string easingType, float period, float amplitude, float overshoot)
    {
        t = Math.Clamp(t, 0f, 1f);

        return easingType.ToLowerInvariant() switch
        {
            "linear" => t,
            "inquad" => t * t,
            "outquad" => t * (2f - t),
            "inoutquad" => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t,
            "incubic" => t * t * t,
            "outcubic" => (t - 1f) * (t - 1f) * (t - 1f) + 1f,
            "inoutcubic" => t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f,
            "inquart" => t * t * t * t,
            "outquart" => 1f - (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f),
            "inoutquart" => t < 0.5f ? 8f * t * t * t * t : 1f - 8f * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f),
            "inquint" => t * t * t * t * t,
            "outquint" => 1f + (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f),
            "inoutquint" => t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f),
            "insine" => 1f - MathF.Cos(t * MathF.PI / 2f),
            "outsine" => MathF.Sin(t * MathF.PI / 2f),
            "inoutsine" => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            "inexpo" => t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f),
            "outexpo" => t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t),
            "inoutexpo" => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ? MathF.Pow(2f, 20f * t - 10f) / 2f : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f,
            "incirc" => 1f - MathF.Sqrt(1f - t * t),
            "outcirc" => MathF.Sqrt(1f - (t - 1f) * (t - 1f)),
            "inoutcirc" => t < 0.5f ? (1f - MathF.Sqrt(1f - 4f * t * t)) / 2f : (MathF.Sqrt(1f - (2f * t - 2f) * (2f * t - 2f)) + 1f) / 2f,
            "inelastic" => EaseInElastic(t, period, amplitude),
            "outelastic" => EaseOutElastic(t, period, amplitude),
            "inoutelastic" => EaseInOutElastic(t, period, amplitude),
            "inback" => t * t * ((overshoot + 1f) * t - overshoot),
            "outback" => (t - 1f) * (t - 1f) * ((overshoot + 1f) * (t - 1f) + overshoot) + 1f,
            "inoutback" => InOutBack(t, overshoot),
            "inbounce" => 1f - EaseOutBounce(1f - t),
            "outbounce" => EaseOutBounce(t),
            "inoutbounce" => t < 0.5f ? (1f - EaseOutBounce(1f - 2f * t)) / 2f : (1f + EaseOutBounce(2f * t - 1f)) / 2f,
            _ => t
        };
    }

    private static float EaseInElastic(float t, float period, float amplitude)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        if (period == 0f) period = 0.3f;
        float s;
        if (amplitude == 0f || amplitude < 1f) { amplitude = 1f; s = period / 4f; }
        else s = period / (2f * MathF.PI) * MathF.Asin(1f / amplitude);
        return -(amplitude * MathF.Pow(2f, 10f * (t - 1f)) * MathF.Sin((t - 1f - s) * (2f * MathF.PI) / period));
    }

    private static float EaseOutElastic(float t, float period, float amplitude)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        if (period == 0f) period = 0.3f;
        float s;
        if (amplitude == 0f || amplitude < 1f) { amplitude = 1f; s = period / 4f; }
        else s = period / (2f * MathF.PI) * MathF.Asin(1f / amplitude);
        return amplitude * MathF.Pow(2f, -10f * t) * MathF.Sin((t - s) * (2f * MathF.PI) / period) + 1f;
    }

    private static float EaseInOutElastic(float t, float period, float amplitude)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        if (period == 0f) period = 0.45f;
        float s;
        if (amplitude == 0f || amplitude < 1f) { amplitude = 1f; s = period / 4f; }
        else s = period / (2f * MathF.PI) * MathF.Asin(1f / amplitude);
        if (t < 0.5f)
            return -(amplitude * MathF.Pow(2f, 10f * (2f * t - 1f)) * MathF.Sin((2f * t - 1f - s) * (2f * MathF.PI) / period)) / 2f;
        return amplitude * MathF.Pow(2f, -10f * (2f * t - 1f)) * MathF.Sin((2f * t - 1f - s) * (2f * MathF.PI) / period) / 2f + 1f;
    }

    private static float InOutBack(float t, float overshoot)
    {
        float c = overshoot * 1.525f;
        return t < 0.5f
            ? 2f * t * t * ((c + 1f) * 2f * t - c) / 2f
            : ((2f * t - 2f) * (2f * t - 2f) * ((c + 1f) * (2f * t - 2f) + c) + 2f) / 2f;
    }

    private static float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
        if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
}
