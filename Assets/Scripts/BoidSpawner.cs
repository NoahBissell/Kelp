using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidSpawner : MonoBehaviour
{
    public BoidGO boidPrefab;
    public int numBoids;
    public float radius;
    public int index;

    public bool spawnAtRuntime;
    
    

    // Start is called before the first frame update
    public BoidGO[] Spawn()
    {
        BoidGO[] boids = new BoidGO[numBoids];
        for (int i = 0; i < numBoids; i++)
        {
            Vector3 position = transform.position + Random.insideUnitSphere * radius;
            Quaternion rotation = Random.rotation;

            BoidGO b;
            #if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                b = PrefabUtility.InstantiatePrefab(boidPrefab) as BoidGO;
                if (b == null)
                {
                    throw new System.Exception("Could not instantiate prefab");
                }
                b.transform.position = position;
                b.transform.rotation = rotation;
            }
            #endif
            if (Application.isPlaying)
            {
                b = Instantiate(boidPrefab, position, rotation);
            }
            else
            {
                return new BoidGO[0];
            }

            b.SetSpawnIndex(index);
            b.SetSpawnPosition(transform.position);
            b.transform.SetParent(transform);
            b.spawned = true;
            boids[i] = b;
        }

        return boids;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, radius * 2 * Vector3.one);
    }
}
