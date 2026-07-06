using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RopeManager : MonoBehaviour
{
    [System.Serializable]
    public struct Point
    {
        public float3 position, prevPosition;
        public int locked;
    };

    public struct Stick
    {
        public int pointAIdx, pointBIdx;
        public float length;
    };

    [System.Serializable]
    public class Interactor
    {
        public Transform transform;
        public float radius;
    }

    public float gravity;
    public int numRopePoints;
    public int numRopes;
    public float radius;

    public Interactor interactor;
    public Point[] points;
    Stick[] sticks;
    private Vector3[] ropePositions;

    public float stepSize;

    public float numIter;
    public Vector3 dir;

    public ComputeShader cs;

    public Material material;

    ComputeBuffer pointsBuffer;
    ComputeBuffer posBuffer;
    ComputeBuffer sticksBuffer;
    ComputeBuffer ropePosBuffer;

    public Mesh mesh;


    private void Start()
    {
        points = new Point[numRopePoints * numRopes];
        sticks = new Stick[(numRopePoints - 1) * numRopes];
        ropePositions = new Vector3[numRopes];
        for (int ropeIdx = 0; ropeIdx < numRopes; ropeIdx++)
        {
            int startIdx = ropeIdx * numRopes;
            Vector3 pos = transform.position + Random.insideUnitSphere * radius;
            ropePositions[ropeIdx] = pos;
            points[startIdx] = new Point()
            {
                position = pos,
                prevPosition = pos,
                locked = 1,
            };
            pos -= stepSize * dir.normalized;
            for (int i = 1; i < numRopePoints; i++)
            {
                points[startIdx + i] = new Point()
                {
                    position = pos,
                    prevPosition = pos,
                    locked = i == numRopePoints - 1 ? 1 : 0,
                };

                sticks[startIdx + i - 1] = new Stick()
                {
                    pointAIdx = i - 1,
                    pointBIdx = i,
                    length = stepSize,
                };

                pos -= stepSize * dir.normalized;
            }
        }

        pointsBuffer = new ComputeBuffer(points.Length, sizeof(int) + sizeof(float) * 6);
        sticksBuffer = new ComputeBuffer(sticks.Length, sizeof(float) + sizeof(int) * 2);
        posBuffer = new ComputeBuffer(points.Length, sizeof(float) * 3);
        ropePosBuffer = new ComputeBuffer(ropePositions.Length, sizeof(float) * 3);

        pointsBuffer.SetData(points);
        sticksBuffer.SetData(sticks);
        ropePosBuffer.SetData(ropePositions);
        
        cs.SetBuffer(0, "_Positions", ropePosBuffer);
        cs.SetBuffer(0, "ropePoints", pointsBuffer);
        cs.SetBuffer(1, "ropePoints", pointsBuffer);
        cs.SetBuffer(1, "ropeSticks", sticksBuffer);
        cs.SetBuffer(1, "ropePositions", posBuffer);
        cs.SetFloat("gravity", gravity);
        cs.SetInt("numPoints", numRopePoints);
        cs.SetInt("numSticks", numRopePoints - 1);

        material.SetBuffer("_ropePositions", posBuffer);
        material.SetBuffer("_Positions", ropePosBuffer);
        material.SetInt("_numPoints", numRopePoints);
        material.SetInt("_numRopes", numRopes);
    }
    
    

    private void Update()
    {
        cs.SetFloat("deltaTime", Time.deltaTime);
        UpdatePoints();
        UpdateSticks();

        // pointsBuffer.GetData(points);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, new Bounds(transform.position, Vector3.one * radius), numRopes);
    }

    void UpdatePoints()
    {
        cs.Dispatch(0, numRopePoints / 64 + 1, 1, 1);
    }


    void UpdateSticks()
    {
        for (int iter = 0; iter < numIter; iter++)
        {
            for (int i = 0; i < 2; i++)
            {
                cs.SetInt("firstGroup", i);
                cs.Dispatch(1, (numRopePoints - 1) / 2 / 64 + 1, 1, 1);
            }
        }
    }

    void OnDestroy()
    {
        pointsBuffer.Release();
        sticksBuffer.Release();
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;

        for (int i = 0; i < numRopePoints; i++)
        {
            Gizmos.DrawSphere(points[i].position.xyz, 0.1f);
        }
    }
}