#pragma multi_compile WATER_DECAL_PARTIAL WATER_DECAL_COMPLETE
#pragma multi_compile WATER_ONE_BAND WATER_TWO_BANDS WATER_THREE_BANDS
#pragma multi_compile _ WATER_LOCAL_CURRENT

// Required to be defined for some includes
#define WATER_SIMULATION

// SRP generic includes
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Water/Shaders/SampleWaterSurface.hlsl"

// The set of input position we need to process

// We allow up to 10 steps to figure out the height of the point
#define SEARCH_ITERATION_COUNT 8

// We consider that we found the point if we were able to
#define SEARCH_DISTANCE_THRESHOLD 0.001

void GetDisplacement_float(float3 position, float scale, float3 offset, out float3 displacement)
{
    int stepCount;
    float currentError;
    float height = FindVerticalDisplacement(position * scale + offset, SEARCH_ITERATION_COUNT, SEARCH_DISTANCE_THRESHOLD, stepCount, currentError);
    
    displacement = float3(height, height, height);
    // displacement += float3(sin(position.x), cos(position.y), sin(position.z));
}