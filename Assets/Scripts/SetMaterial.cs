using System;
using UnityEngine;
using KBCore.Refs;

[ExecuteAlways]
public class SetMaterial : MonoBehaviour
{
    public string name;
    public string name2;
    public Material material;
    public GameObject target;
    
    private void Update()
    {
        material.SetVector(name, target.transform.position);
        material.SetVector(name2, target.transform.forward);
    }
}