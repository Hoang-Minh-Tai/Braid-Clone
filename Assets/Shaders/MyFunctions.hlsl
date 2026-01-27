void TintColor_float(float4 uvColor, float4 tintColor, out float3 color)
{
    color = uvColor.rgb * tintColor.rgb;
}

float hash_float(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise_float(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = hash_float(i);
    float b = hash_float(i + float2(1.0, 0.0));
    float c = hash_float(i + float2(0.0, 1.0));
    float d = hash_float(i + float2(1.0, 1.0));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}

float fbm_float(float2 p, float strength)
{
    float f = 0.0;
    float amp = strength;

    for (int i = 0; i < 5; i++)
    {
        f += amp * noise_float(p);
        p *= 2.0;
        amp *= 0.5;
    }
    return f;
}

void pattern_float(float2 p, float strength, float time, out float result)
{
    float2 q = float2(
        fbm_float(p + float2(0.0, 0.0) - time * 0.1, strength),
        fbm_float(p + float2(5.2, 1.3) - time * 0.1, strength)
    );

    float2 r = float2(
        fbm_float(p + 4.0 * q + float2(1.7, 9.2) + time * 0.5, strength),
        fbm_float(p + 4.0 * q + float2(8.3, 2.8) + time * 0.5, strength)
    );

    result = fbm_float(p + 4.0 * r, strength);
}


void borderMask_float(float2 st, float width, out float mask)
{
    float maskX = smoothstep(0.0, width, st.x) - smoothstep(1 - width, 1.0, st.x);
    float maskY = smoothstep(0.0, width, st.y) - smoothstep(1 - width, 1.0, st.y);
    mask = maskX * maskY;
}

void fade_float(float2 st, float radius, out float fade)
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(st, center);
    
    fade = 1 - smoothstep(radius - 1, radius, dist);
}

