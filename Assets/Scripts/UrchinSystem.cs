using System;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Random = UnityEngine.Random;

public class UrchinSystem : InstancedRenderer
{
    public InstancedRenderer kelp;

    public int numUrchins;
    public float scale;

    public float pow;

    public float startAtBottomBias = 0.2f;

    public float climbSpeed;
    
    public AnimationCurve climbCurve;

    public struct UrchinData
    {
        public Vector3 originPosition;
        public float height;
        public float t;
    }

    
    private readonly List<UrchinData> urchinData = new();
    

    public void RemoveInstances(Vector3 position, float radius, float probability)
    {
        for (int i = 0; i < numUrchins; i++)
        {
            float dist = Vector3.Distance(position, instances[i].position);

            if (dist < radius && Random.value < probability)
            {
                instances.RemoveAt(i);
                urchinData.RemoveAt(i);
            }
        }
    }

    

    void AddRandomUrchin()
    {
        var randomKelp = kelp.instances[Random.Range(0, kelp.instances.Count)];
        float randomHeight = Random.Range(0f, 1f);
        randomHeight = Mathf.Pow(randomHeight, pow) * startAtBottomBias;
        Vector3 randomOffset = Random.insideUnitSphere;
        Vector3 position = randomKelp.position + randomKelp.placeNormal * randomHeight * randomKelp.scale.y;
        Vector3 baseOffset = new Vector3(randomOffset.x, 0, randomOffset.z) * (1 - randomHeight);
        Vector3 normal = Random.onUnitSphere;
        Add(position + baseOffset, normal, Quaternion.LookRotation(normal), Vector3.one * scale, position);
        
        urchinData.Add(new UrchinData
        {
            originPosition = position,
            height = randomKelp.scale.y * Random.Range(0f, 1f),
            t = randomHeight
        });
    }

    protected override void Update()
    {
        for (int i = instances.Count; i < numUrchins; i++)
        {
            AddRandomUrchin();
        }
        
        if(instances.Count > numUrchins) {
            instances.RemoveRange(numUrchins, instances.Count - numUrchins);
            urchinData.RemoveRange(numUrchins, urchinData.Count - numUrchins);
        }

        for (int i = 0; i < instances.Count; i++)
        {
            Instance inst = instances[i];
            UrchinData urchin = urchinData[i];

            float evaluatedT = climbCurve.Evaluate(urchin.t);

            inst.position = Vector3.Lerp(inst.originPosition, inst.originPosition + Vector3.up * urchin.height, evaluatedT);
            urchin.t += Time.deltaTime * climbSpeed;
            urchin.t %= 1f;

            instances[i] = inst;
            urchinData[i] = urchin;
        }
        
        base.Update();
    }
}