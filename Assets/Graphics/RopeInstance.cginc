#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
StructuredBuffer<float3> _Positions;
StructuredBuffer<float3> _ropePositions;
int _numPoints;
int _numRopes;
#endif

float _Step;

void ConfigureProcedural () {
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float3 position = _Positions[unity_InstanceID];

    //UNITY_MATRIX_M = 0.0;
    float4x4 m = 0;

    m._m00_m11_m22_m33 = float4(1, 1, 1, 1); // identity
    m._m03_m13_m23 = position;              // translation

    // unity_ObjectToWorld = m;
    #endif
}


void RopePosition_float(in float t, in float angle, in float radius, in float3 pp, out float3 position, out float3 normal) {
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float idxFrac = unity_InstanceID * _numPoints + t * _numPoints;
    int lowerIdx = floor(idxFrac);
    int upperIdx = lowerIdx + 1;
    float3 posL = _Positions[lowerIdx];
    float3 posU = _Positions[upperIdx];
    #else
    float3 posL = float3(0, t, 0);
    float3 posU = float3(0, t + 0.1, 0);
    #endif
   

    // float3 pos = lerp(posL, posU, idxFrac - lowerIdx);
    float3 tangent = normalize(posU - posL);
    normal = normalize(cross(float3(0, 0, 1), tangent));
    float3 binormal = normalize(cross(tangent, normal));

    position = posL + normal * cos(angle) * radius + binormal * sin(angle) * radius;
    //position = pp + posL;
}

void ShaderGraphFunction_float(in float3 i, out float3 o)
{
    o = i;
}
