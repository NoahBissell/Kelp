using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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
    public int numPoints;

    public Interactor interactor;
    public Point[] points;
    Stick[] sticks;

    public float stepSize;

    public float numIter;
    public Vector3 dir;

    public ComputeShader cs;

    public Material material;

    ComputeBuffer pointsBuffer;
    ComputeBuffer posBuffer;
    ComputeBuffer sticksBuffer;


    private void Start()
    {
        points = new Point[numPoints];
        sticks = new Stick[numPoints - 1];

        Vector3 pos = transform.position;
        points[0] = new Point()
        {
            position = pos,
            prevPosition = pos,
            locked = 1,
        };
        pos -= stepSize * dir.normalized;
        for (int i = 1; i < numPoints; i++)
        {
            points[i] = new Point()
            {
                position = pos,
                prevPosition = pos,
                locked = i == numPoints - 1 ? 1 : 0,
            };

            sticks[i - 1] = new Stick()
            {
                pointAIdx = i - 1,
                pointBIdx = i,
                length = stepSize,
            };
            
            pos -= stepSize * dir.normalized;
        }

        pointsBuffer = new ComputeBuffer(numPoints, sizeof(int) + sizeof(float) * 6);
        sticksBuffer = new ComputeBuffer(numPoints - 1, sizeof(float) + sizeof(int) * 2);
        posBuffer = new ComputeBuffer(numPoints, sizeof(float) * 3);

        pointsBuffer.SetData(points);
        sticksBuffer.SetData(sticks);

        cs.SetBuffer(0, "pointsBuffer", pointsBuffer);
        cs.SetBuffer(1, "pointsBuffer", pointsBuffer);
        cs.SetBuffer(1, "sticksBuffer", sticksBuffer);
        cs.SetBuffer(1, "ropePositions", posBuffer);
        cs.SetFloat("gravity", gravity);
        cs.SetInt("numPoints", numPoints);
        cs.SetInt("numSticks", numPoints - 1);

        material.SetBuffer("_ropePositions", posBuffer);
    }

    private void Update()
    {
        cs.SetFloat("deltaTime", Time.deltaTime);
        UpdatePoints();
        UpdateSticks();

        // pointsBuffer.GetData(points);
    }

    void UpdatePoints()
    {
        cs.Dispatch(0, numPoints / 64 + 1, 1, 1);
    }


    void UpdateSticks()
    {
        for (int iter = 0; iter < numIter; iter++)
        {
            for (int i = 0; i < 2; i++)
            {
                cs.SetInt("firstGroup", i);
                cs.Dispatch(1, (numPoints - 1) / 2 / 64 + 1, 1, 1);
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

        for (int i = 0; i < numPoints; i++)
        {
            Gizmos.DrawSphere(points[i].position.xyz, 0.1f);
        }
    }
}