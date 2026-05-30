using System;
using KBCore.Refs;
using UnityEngine;

[ExecuteAlways]
public class MaterialPropSetter : MonoBehaviour
{
    [SerializeField, Self] public MeshRenderer meshRenderer;

    
    [System.Serializable]
    public class PropertyLink
    {
        public string name;
        public LinkType type;
        public bool onlyInEditor;
        
        public enum LinkType
        {
            Time,
            Position
        }
    }

    public PropertyLink[] links;


    private void OnValidate()
    {
        this.ValidateRefs();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            foreach (var link in links)
            {
                if (link.onlyInEditor)
                {
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    switch (link.type)
                    {
                        case PropertyLink.LinkType.Position:
                            mpb.SetVector(link.name, transform.position);
                            break;
                        case PropertyLink.LinkType.Time:
                            mpb.SetFloat(link.name, Time.time);
                            break;
                    }
                    
                    meshRenderer.SetPropertyBlock(mpb);
                    
                    
                }
            }
        }
    }
    #endif

    
}