using UnityEngine;

[CreateAssetMenu(menuName = "Ocean/Species")]
public class Species : ScriptableObject
{
    public string displayName;
    public string scientificName;
    [TextArea] public string description;
    public string depthLayer;
    public string behavior;
}