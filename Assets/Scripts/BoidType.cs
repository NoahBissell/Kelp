using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boid Type")]
public class BoidType : ScriptableObject
{
    public int priority;
    //public float inertia;
    public float force;
    public float cohesionWeight;
    public float avoidanceWeight;
    public float alignmentWeight;
    public float obstacleWeight;
    public float ceilingWeight;
    public float floorWeight;
    public float boundaryWeight;
    public float targetWeight;
    public float fleeWeight;
    public float autonomousWeight;

    public float ceilingHeight;
    // public float ceilingTurnDistance;
    public float floorHeight;
    //public float obstacleAversion;
    
    public float maxSpeed;
    public float minSpeed;
    public int numRays;
    public float detectionRadius;
    public float obstacleSlowdown;
    public float avoidanceRadius;
    public float rayDistance;
    public float raySphereRadius;
    public float rayRandomnessFactor;
    public int maxRandomRayIndex;
    public float maxTurnSpeed;
    public float boundaryRadius;
    public float fleeRadius;

    [SerializeField] [HideInInspector] Vector3[] rayDirections;

    private void OnValidate()
    {
        rayDirections = SpherePointsUtility.CalculateSpherePoints(numRays, .618f);
        maxRandomRayIndex = (int) (numRays * rayRandomnessFactor);
    }
    
    public Vector3 GetRayDirection(int index)
    {
        return rayDirections[index];
    }

    public int GetRandomRayIndex()
    {
        return Random.Range(0, maxRandomRayIndex);
    }
    
    public Vector3 SteerTowards(Vector3 vector, Vector3 velocity)
    {
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxTurnSpeed);
    }
}