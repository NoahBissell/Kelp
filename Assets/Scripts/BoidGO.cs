using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public abstract class BoidGO : MonoBehaviour
{
    public BoidType boidType;
    int randomRayStartIndex;
    protected float currentSpeed;
    float3 desiredHeading;
    float desiredSpeed = -1f;
    public int spawnIndex;
    public bool spawned = false;
    public Vector3 spawnPosition;
    //Vector3[] rayDirections;

    // Start is called before the first frame update
    public virtual void Start()
    {
        currentSpeed = boidType.minSpeed;
        randomRayStartIndex = boidType.GetRandomRayIndex();
    }

    public float3 GetDesiredHeading()
    {
        return desiredHeading;
    }

    public float GetDesiredSpeed()
    {
        return desiredSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public void SetDesiredHeading(Vector3 h)
    {
        desiredHeading = h;
    }

    public void SetDesiredSpeed(float speed)
    {
        desiredSpeed = speed;
    }

    public void SetSpawnIndex(int index)
    {
        spawnIndex = index;
    }

    public int GetRandomRayStartIndex(bool cycle)
    {
        if (cycle)
        {
            randomRayStartIndex++;
            if (randomRayStartIndex == boidType.maxRandomRayIndex)
            {
                randomRayStartIndex = 0;
            }
        }
        return randomRayStartIndex;
    }

   


    public abstract void Move(Vector3 targetHeading, float speed);

    public int GetSpawnIndex()
    {
        return spawnIndex;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }
    public void SetSpawnPosition(Vector3 transformPosition)
    {
        spawnPosition = transformPosition;
    }
}
