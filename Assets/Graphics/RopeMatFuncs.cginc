StructuredBuffer<float3> _ropePositions;
int _numPoints;

void RopePosition_float(in float t, in float angle, in float radius, out float3 position, out float3 normal) {
    float idxFrac = t * _numPoints;
    int lowerIdx = floor(idxFrac);
    int upperIdx = lowerIdx + 1;
    // float3 posL = _ropePositions[lowerIdx];
    // float3 posU = _ropePositions[upperIdx];
    float3 posL = float3(0, t, 0);
    float3 posU = float3(0, t + 0.1, 0);

    // float3 pos = lerp(posL, posU, idxFrac - lowerIdx);
    float3 tangent = normalize(posU - posL);
    normal = cross(float3(0, 1, 0), tangent);
    float3 binormal = cross(tangent, normal);

    position = posL + normal * cos(angle) * radius + binormal * sin(angle) * radius;
}