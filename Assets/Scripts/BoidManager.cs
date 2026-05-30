using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Collections;


public class BoidManager : BoidSpawner
{
    public List<BoidGO> boids;
    public List<BoidGO> targets;
    public List<BoidGO> obstacles;
    public BoidSpawner[] spawners;

    public LayerMask terrainMask;
    
    NativeArray<BoidSettings> boidSettings;
    NativeArray<Boid> boidData;
    NativeArray<Boid> targetData;
    NativeArray<Boid> obstacleData;
    NativeArray<float3> neighborCenters;
    NativeArray<float3> neighborHeadings;
    NativeArray<float3> avoidanceHeadings;
    NativeArray<float3> targetPositions;
    NativeArray<float3> obstaclePositions;
    NativeArray<float> numNeighbors;
    NativeArray<float> numAvoided;

    float3[] obstacleAvoidanceHeadings;
    float3[] boundaryHeadings;
    float3[] ceilingHeadings;
    float3[] floorHeadings;
    int[] rayIndices;
    public bool debugRaycasting;
    

    public struct Boid
    {
        public int index;
        public float3 position;
        public float3 heading;
    }

    public struct BoidSettings
    {
        public int index;
        public int priority;
        public float detectionRadius;
        public float avoidanceRadius;
    }

    private void OnEnable()
    {
        SpawnBoids(runtime: true);

        InitializeData();
    }

    void SpawnBoids(bool runtime)
    {
        if (spawnAtRuntime == runtime)
        {
            AddBoids(Spawn());
        }

        if(spawners.Length > 0)
        {
            foreach(BoidSpawner spawner in spawners)
            {
                if (spawner.spawnAtRuntime == runtime)
                {
                    AddBoids(spawner.Spawn());
                }
            }
        }
    }

    [Button]
    public void PreSpawnBoids()
    {
        DespawnBoids();
        
        SpawnBoids(runtime: false);
    }

    [Button]
    public void DespawnBoids()
    {
        for(int i = boids.Count - 1; i >= 0; i--)
        {
            if (boids[i].spawned)
            {
                BoidGO boid = boids[i];
                boids.RemoveAt(i);
                DestroyImmediate(boid.gameObject);
            }
        }
    }

    void InitializeData()
    {
        obstacleAvoidanceHeadings = new float3[boids.Count];
        boundaryHeadings = new float3[boids.Count];
        ceilingHeadings = new float3[boids.Count];
        floorHeadings = new float3[boids.Count];
        rayIndices = new int[boids.Count];

        boidData = new NativeArray<Boid>(boids.Count, Allocator.Persistent);
        targetData = new NativeArray<Boid>(targets.Count, Allocator.Persistent);
        obstacleData = new NativeArray<Boid>(obstacles.Count, Allocator.Persistent);
        neighborCenters = new NativeArray<float3>(boids.Count, Allocator.Persistent);
        neighborHeadings = new NativeArray<float3>(boids.Count, Allocator.Persistent);
        targetPositions = new NativeArray<float3>(boids.Count, Allocator.Persistent);
        obstaclePositions = new NativeArray<float3>(boids.Count, Allocator.Persistent);
        avoidanceHeadings = new NativeArray<float3>(boids.Count, Allocator.Persistent);
        numNeighbors = new NativeArray<float>(boids.Count, Allocator.Persistent);
        numAvoided = new NativeArray<float>(boids.Count, Allocator.Persistent);

        boidSettings = new NativeArray<BoidSettings>(boids.Count, Allocator.Persistent);
        
        for (int i = 0; i < boids.Count; i++)
        {
            boidSettings[i] = new BoidSettings
            {
                index = boids[i].GetSpawnIndex(),
                priority = boids[i].boidType.priority,
                detectionRadius = boids[i].boidType.detectionRadius,
                avoidanceRadius = boids[i].boidType.avoidanceRadius
            };
        }

        
    }

    void DisposeData()
    {
        boidData.Dispose();
        neighborCenters.Dispose();
        neighborHeadings.Dispose();
        avoidanceHeadings.Dispose();
        numNeighbors.Dispose();
        numAvoided.Dispose();
        boidSettings.Dispose();
        
        targetData.Dispose();
        targetPositions.Dispose();
        obstaclePositions.Dispose();
    }


    private void OnDisable()
    {
        DisposeData();
    }

    public void AddBoids(BoidGO[] newBoids)
    {
        // BoidGO[] temp = new BoidGO[boids.Length];
        // System.Array.Copy(boids, temp, boids.Length);
        // boids = new BoidGO[boids.Length + newBoids.Length];
        // System.Array.Copy(temp, 0, boids, 0, temp.Length);
        // System.Array.Copy(newBoids, 0, boids, temp.Length, newBoids.Length);
        boids.AddRange(newBoids);
    }

    private void FixedUpdate()
    {
        UpdateBoids();
    }

    

    void UpdateBoids()
    {
        for (int boidIndex = 0; boidIndex < boids.Count; boidIndex++)
        {
            boidData[boidIndex] = new Boid
            {
                position = boids[boidIndex].transform.position,
                heading = boids[boidIndex].transform.forward,

            };
        }

        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
        {
            targetData[targetIndex] = new Boid
            {
                position = targets[targetIndex].transform.position,
                heading = targets[targetIndex].transform.forward
            };
        }
        
        for (int obstacleIndex = 0; obstacleIndex < obstacles.Count; obstacleIndex++)
        {
            obstacleData[obstacleIndex] = new Boid
            {
                position = obstacles[obstacleIndex].transform.position,
                heading = obstacles[obstacleIndex].transform.forward
            };
        }

        InitialBoidJob boidJob = new()
        {
            boidSettings = boidSettings,
            boidData = boidData,
            targetData = targetData,
            obstacleData = obstacleData,
            neighborCenters = neighborCenters,
            neighborHeadings = neighborHeadings,
            avoidanceHeadings = avoidanceHeadings,
            targetPositions = targetPositions,
            obstaclePositions = obstaclePositions,
            numNeighbors = numNeighbors,
            numAvoided = numAvoided,
        };
        var boidJobHandle = boidJob.ScheduleParallel(boidData.Length, 64, default);

        for(int boidIndex = 0; boidIndex < boids.Count; boidIndex++)
        {
            BoidGO boid = boids[boidIndex];
            obstacleAvoidanceHeadings[boidIndex] = float3.zero;
            boundaryHeadings[boidIndex] = float3.zero;
            ceilingHeadings[boidIndex] = float3.zero;
            floorHeadings[boidIndex] = float3.zero;

            #region RAY_OLD
            float distance = float.PositiveInfinity;
            Ray ray = new();
            int startIndex = 0;//boid.GetRandomRayStartIndex(true);
            int rayIndex = startIndex;
            for (int i = 0; i < boid.boidType.numRays - 1; i++)
            {
                if(rayIndex == boid.boidType.numRays)
                {
                    rayIndex = 0;
                }
                
                ray = new(boid.transform.position, boid.transform.rotation * boid.boidType.GetRayDirection(rayIndex));
                if (!Physics.SphereCast(ray, boid.boidType.raySphereRadius, out RaycastHit hitInfo, boid.boidType.rayDistance, terrainMask))
                {
                    if(debugRaycasting)
                    {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * hitInfo.distance);
                    }
                    distance = hitInfo.distance;
                    break;
                }
            
                rayIndex++;
            }
            
            
            rayIndices[boidIndex] = rayIndex;
            if (rayIndex > startIndex)
            {
                obstacleAvoidanceHeadings[boidIndex] = ray.direction;
                //obstacleAvoidanceHeadings[boidIndex] += (obstacleAvoidanceHeadings[boidIndex] - boidData[boidIndex].heading) * boid.boidType.obstacleAversion;
                // obstacleAvoidanceHeadings[boidIndex] *= (1 - distance / boid.boidType.rayDistance);
            }
            #endregion
            


            float dstToCeiling = boid.boidType.ceilingHeight - boid.transform.position.y;
            if (dstToCeiling < 0)
            {
                //ceilingHeading = Vector3.Lerp(boid.transform.forward, Vector3.down, dstToCeiling / ceilingTurnDistance);
                ceilingHeadings[boidIndex] += new float3(0, -1, 0);
            }
            float dstToFloor = boid.transform.position.y - boid.boidType.floorHeight;
            if (dstToFloor < 0)
            {
                floorHeadings[boidIndex] += new float3(0, 1, 0);
            }
            float3 pos = boid.transform.position;
            if (math.distancesq(boid.GetSpawnPosition(), pos) > boid.boidType.boundaryRadius * boid.boidType.boundaryRadius)
            {
                boundaryHeadings[boidIndex] = math.normalize((float3)boid.GetSpawnPosition() - pos);
            }
        }

        boidJobHandle.Complete();

        for (int boidIndex = 0; boidIndex < boids.Count; boidIndex++)
        {
            if (numNeighbors[boidIndex] > 0)
            {
                neighborCenters[boidIndex] /= numNeighbors[boidIndex];
                neighborHeadings[boidIndex] /= numNeighbors[boidIndex];

                if (numAvoided[boidIndex] > 0)
                {
                    avoidanceHeadings[boidIndex] /= numAvoided[boidIndex];
                }
            }
           
            // OLD boid

            BoidGO boid = boids[boidIndex];

            float3 neighborOffset = neighborCenters[boidIndex] - boidData[boidIndex].position;
            float3 obstacleOffset =boidData[boidIndex].position - obstaclePositions[boidIndex];
            
            float3 acceleration = float3.zero;
            float3 velocity = boid.GetCurrentSpeed() * boidData[boidIndex].heading;
            float3 cohesionHeading = (numNeighbors[boidIndex] == 0 ? float3.zero : boid.boidType.cohesionWeight) * boid.boidType.SteerTowards(neighborOffset, velocity);
            float3 alignmentHeading = boid.boidType.alignmentWeight * boid.boidType.SteerTowards(neighborHeadings[boidIndex], velocity);
            float3 avoidanceHeading = boid.boidType.avoidanceWeight * boid.boidType.SteerTowards(avoidanceHeadings[boidIndex], velocity);
            float3 boundaryHeading = boid.boidType.boundaryWeight * boid.boidType.SteerTowards(boundaryHeadings[boidIndex], velocity);
            float3 ceilingHeading = boid.boidType.ceilingWeight * boid.boidType.SteerTowards(ceilingHeadings[boidIndex], velocity);
            float3 floorHeading = boid.boidType.floorWeight * boid.boidType.SteerTowards(floorHeadings[boidIndex], velocity);
            float3 obstacleHeading = (math.lengthsq(obstacleOffset) > boid.boidType.fleeRadius * boid.boidType.fleeRadius ? 0 : boid.boidType.fleeWeight) * boid.boidType.SteerTowards(obstacleOffset, velocity);
            acceleration += cohesionHeading + alignmentHeading + avoidanceHeading;
            acceleration += boundaryHeading + ceilingHeading + floorHeading + obstacleHeading;
            if (!obstacleAvoidanceHeadings[boidIndex].Equals(float3.zero))
            {
                float3 collisionAvoidHeading = boid.boidType.obstacleWeight * boid.boidType.SteerTowards(obstacleAvoidanceHeadings[boidIndex], velocity);
                acceleration += collisionAvoidHeading;
                acceleration -= boidData[boidIndex].heading * boid.boidType.obstacleSlowdown;
            }
            

            velocity += acceleration * Time.deltaTime;
            float speed = math.length(velocity);
            float3 dir = velocity / speed;
            speed = math.clamp(speed, boid.boidType.minSpeed, boid.boidType.maxSpeed);
            velocity = dir * speed;
            
            boid.Move(dir, speed);
        }


    }

    

    [BurstCompile]
    public struct InitialBoidJob : IJobFor
    {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<BoidSettings> boidSettings;
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<Boid> boidData;
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<Boid> targetData;
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<Boid> obstacleData;
        public NativeArray<float3> neighborCenters;
        public NativeArray<float3> neighborHeadings;
        public NativeArray<float3> avoidanceHeadings;
        public NativeArray<float3> targetPositions;
        public NativeArray<float3> obstaclePositions;
        public NativeArray<float> numNeighbors;
        public NativeArray<float> numAvoided;

        public void Execute(int index)
        {
            neighborCenters[index] = 0;
            neighborHeadings[index] = 0;
            avoidanceHeadings[index] = 0;
            targetPositions[index] = 0;
            // obstaclePositions[index] = 0;
            numNeighbors[index] = 0;
            numAvoided[index] = 0;

            for (int i = 0; i < boidData.Length; i++)
            {
                if (i == index) continue;

                float3 neighborPosition = boidData[i].position;
                float dstsq = math.distancesq(boidData[index].position, neighborPosition);
                if (dstsq < boidSettings[index].detectionRadius * boidSettings[index].detectionRadius)
                {

                    if (boidSettings[index].priority == boidSettings[i].priority)
                    {
                        numNeighbors[index]++;
                        neighborCenters[index] += neighborPosition;
                        neighborHeadings[index] += boidData[i].heading;
                    }

                    if (dstsq < boidSettings[i].avoidanceRadius * boidSettings[i].avoidanceRadius &&
                        boidSettings[index].priority <= boidSettings[i].priority)
                    {
                        numAvoided[index]++;
                        float3 neighborDir = neighborPosition - boidData[index].position;
                        avoidanceHeadings[index] -= neighborDir / dstsq;
                    }
                }
            }

            float3 nearest = targetData[0].position;
            float minDstSqr = float.PositiveInfinity;
            for (int i = 0; i < targetData.Length; i++)
            {
                float dst = math.distancesq(targetData[i].position, boidData[index].position);
                if (dst < minDstSqr)
                {
                    minDstSqr = dst;
                    nearest = targetData[i].position;
                }
            }
            targetPositions[index] = nearest;
            
            nearest = obstacleData[0].position;
            minDstSqr = float.PositiveInfinity;
            for (int i = 0; i < obstacleData.Length; i++)
            {
                float dst = math.distancesq(obstacleData[i].position, boidData[index].position);
                if (dst < minDstSqr)
                {
                    minDstSqr = dst;
                    nearest = obstacleData[i].position;
                }
            }
            obstaclePositions[index] = nearest;

        }
    }

    [BurstCompile]
    public struct SteerBoidJob : IJobFor
    {
        public void Execute(int index)
        {
            
        }
    }




}



