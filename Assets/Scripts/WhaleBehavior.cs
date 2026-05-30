using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class WhaleBehavior : MonoBehaviour
{
    public static readonly int hashSwim = Animator.StringToHash("Swim");
    public static readonly int hashSide = Animator.StringToHash("Side");
    public static readonly int hashBreathe = Animator.StringToHash("Breathe");
    
    [SerializeField, Self] Animator animator;
    [SerializeField, Self] private SplineAnimate spline;

    public float speedThreshold;
    public float turnThreshold;
    public float turnLookaheadSeconds;
    private Vector3 lastPos;

    public float breathProbabilityPerSecond;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnValidate()
    {
        this.ValidateRefs();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = transform.position - lastPos;
        
        animator.SetInteger(hashSwim, offset.magnitude > speedThreshold ? 1 : 0);

        Vector3 accel = spline.Container.EvaluateAcceleration((spline.NormalizedTime + turnLookaheadSeconds / spline.Duration) % 1f);
        float right = Vector3.Dot(accel, transform.right);

        if (right > turnThreshold)
        {
            animator.SetInteger(hashSide, 1);
        }
        else if (right < -turnThreshold)
        {
            animator.SetInteger(hashSide, -1);
        }
        else
        {
            animator.SetInteger(hashSide, 0);
        }


        if (Random.Range(0f, 1f) < breathProbabilityPerSecond * Time.deltaTime)
        {
            animator.SetTrigger(hashBreathe);
        }
        
        
        lastPos = transform.position;
    }
}
