using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BoidBehavior : MonoBehaviour
{
    public static readonly int hashRandom = Animator.StringToHash("Random");
    public static readonly int hashTimer = Animator.StringToHash("Timer");
    public static readonly int hashUnderwater = Animator.StringToHash("Underwater");

    Animator animator;
    [HideInInspector] public BoidGO boidGO;
    // [HideInInspector] public Bouyant bouyant;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boidGO = GetComponent<BoidGO>();
        // bouyant = GetComponent<Bouyant>();
        // bouyant.OnSwitchFluid += SwitchFluid;
        // SceneLinkedSMB<BoidBehavior>.Initialise(animator, this);
        
    }

    // private void SwitchFluid(FluidSettings.FluidType type)
    // {
    //     if(type == FluidSettings.FluidType.Air)
    //     {
    //         animator.SetBool(hashUnderwater, false);
    //     }
    //     else
    //     {
    //         animator.SetBool(hashUnderwater, true);
    //     }
    // }

    public void TimerElapsed()
    {
        animator.SetTrigger(hashTimer);
    }
    
    

}