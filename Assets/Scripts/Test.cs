using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class Test : MonoBehaviour
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

    private void OnValidate()
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
        
    }

    private void Update()
    {
        UpdatePoints();
        UpdateSticks();
    }

    void UpdatePoints()
    {
        for (int i = 0; i < numPoints; i++)
        {
            Point p = points[i];
            if (p.locked == 1) continue;

            float3 positionBeforeUpdate = p.position;
            p.position += (p.position - p.prevPosition);
            p.position += new float3(0, -1, 0) * gravity * Time.deltaTime * Time.deltaTime;
            if (math.distance(p.position, interactor.transform.position) <= interactor.radius)
            {
                float3 offset = p.position - (float3)interactor.transform.position;
                p.position = (float3)interactor.transform.position + math.normalize(offset) *  interactor.radius;
            }
            p.prevPosition = positionBeforeUpdate;

            points[i] = p;
        }
    }


    void UpdateSticks()
    {
        for (int iter = 0; iter < numIter; iter++)
        {
            for (int i = 0; i < numPoints - 1; i++)
            {
                Stick s = sticks[i];
                Point a = points[s.pointAIdx];
                Point b = points[s.pointBIdx];
                float3 stickCenter = (a.position + b.position) * 0.5f;
                float3 stickDir = math.normalize(b.position - a.position);

                if(a.locked == 0)
                    a.position = stickCenter - stickDir * s.length * 0.5f;
                if(b.locked == 0)
                    b.position = stickCenter + stickDir * s.length * 0.5f;

                points[s.pointAIdx] = a;
                points[s.pointBIdx] = b;
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < numPoints; i++)
        {
            Gizmos.DrawSphere(points[i].position.xyz, 0.1f);
        }
    }
}