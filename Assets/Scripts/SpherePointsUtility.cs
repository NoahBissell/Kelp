using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePointsUtility : MonoBehaviour
{
    // For playing around in scene view
    [SerializeField] int numPoints;
    [SerializeField] float turnFraction;
    [SerializeField] float pointsRadius;
    [SerializeField] float maxAngle;

    [SerializeField] bool disk;
    [SerializeField] bool sphere;

    [SerializeField] Vector3[] points;

    private void OnValidate()
    {

        points = new Vector3[numPoints];
        if (disk)
        {
            points = CalculateDiskPoints(numPoints, turnFraction);
        }
        else if(sphere)
        {
            points = CalculateSpherePoints(numPoints, turnFraction);
        }

    }

    public static Vector3[] CalculateDiskPoints(int n, float turnFraction)
    {
        Vector3[] pts = new Vector3[n];

        for(int i = 0; i<n; i++)
        {
            float distance = Mathf.Sqrt((float)i / n);
            float angle = Mathf.PI * 2 * turnFraction * i;

            float x = distance * Mathf.Sin(angle);
            float y = distance * Mathf.Cos(angle);
            

            pts[i] = new Vector3(x, 0, y);
        }
        return pts;
    }

    public static Vector3[] CalculateSpherePoints(int n, float turnFraction)
    {
        Vector3[] pts = new Vector3[n];

        for(int i = 0; i<n; i++)
        {
            float phi = Mathf.Acos(1 - 2 * (float) i / n);
            float theta = Mathf.PI * turnFraction * i;

            float x = Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = Mathf.Sin(theta) * Mathf.Sin(phi);
            float z = Mathf.Cos(phi);

            pts[i] = new Vector3(x, y, z);
        }


        return pts;
    }

    void OnDrawGizmos()
    {
        if(points == null)
        {
            return;
        }
        foreach(var point in points)
        {
            Gizmos.DrawSphere(point, pointsRadius);
        }
        
    }
}