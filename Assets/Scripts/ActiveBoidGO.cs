using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using Unity.Mathematics;
using UnityEngine;


public class ActiveBoidGO : BoidGO
{
    public bool useRigidbody;
    public float maxDeltaV;
    public float bankFactor;
    public float directionChangeMultiplier;
    // bool underwater = true;
    [SerializeField, Child] MeshRenderer meshRenderer;

    public float animationTime;
    public float upDownThreshold;
    public AdaptiveNormalizer animSpeedNormalizer = new();

    public float baseSpeed;
    public float maxFlipRate = 1f;

    private MaterialPropertyBlock mpb;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        mpb = new MaterialPropertyBlock();
        
        
    }

    void OnValidate()
    {
        this.ValidateRefs();
    }
    
    
    public override void Move(Vector3 targetHeading, float speed)
    {
        Vector3 newHeading = targetHeading;
        
        // if(useRigidbody)
        // {
        //     if (!underwater) return;
        //     rb.MoveRotation(Quaternion.LookRotation(newHeading));
        //     Vector3 velocity = speed * newHeading;
        //     rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, velocity, maxDeltaV * Time.deltaTime);
        // }
        // else
        // {
        Vector3 diff = targetHeading * speed - transform.forward * currentSpeed;
        float speedDiff = Mathf.Max(0, speed - currentSpeed) + Vector3.Angle(targetHeading, transform.forward) / 180f * directionChangeMultiplier;
        mpb.Clear();
        float acceleration = (1 - baseSpeed) * animSpeedNormalizer.Update(speedDiff, Time.fixedDeltaTime) + baseSpeed;
        animationTime += Time.fixedDeltaTime * acceleration;
        mpb.SetFloat("_PlayTime", animationTime);
        mpb.SetFloat("_Speed", acceleration);
        meshRenderer.SetPropertyBlock(mpb);

        if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up)) < upDownThreshold)
        {
            Quaternion newRot = Quaternion.LookRotation(newHeading, Vector3.up);
            Quaternion oldRot = Quaternion.LookRotation(newHeading, transform.up);
            
            transform.rotation = Quaternion.Lerp(oldRot, newRot, maxFlipRate * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(newHeading, transform.up);
        }

        transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);
        // }
        
        currentSpeed = speed;

    }


}
