using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerBoidGO : BoidGO
{
    public static event System.Action<Vector3, BoidGO> OnBoidSetTarget;
    public static System.Action OnBoidStopTarget;

    bool setting;

    public override void Move(Vector3 targetHeading, float speed)
    {

       
        if (!float.IsNaN(targetHeading.x))
        {
            Vector3 newHeading = math.normalize(transform.forward + boidType.maxTurnSpeed * Time.deltaTime * (targetHeading - transform.forward));
            OnBoidSetTarget?.Invoke(newHeading, this);
            setting = true;
        }
        else
        {
            if(setting)
            {
                OnBoidStopTarget?.Invoke();
            }
            setting = false;
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
