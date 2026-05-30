using System;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
[DefaultExecutionOrder(100)]
public class SpeciesManager : MonoBehaviour
{
    Dictionary<Species, List<FishSpeciesIdentity>> speciesLookup;
    [SerializeField, Scene] private FishSpeciesIdentity[] speciesInScene;

    public bool genOnStart = true;


    private void OnValidate()
    {
        this.ValidateRefs();
        
        GenerateRegistry();
    }

    private void Start()
    {
        if (genOnStart) GenerateRegistry();
    }

    void GenerateRegistry()
    {
        speciesLookup = new Dictionary<Species, List<FishSpeciesIdentity>>();
        foreach (FishSpeciesIdentity s in speciesInScene)
        {
            if (!speciesLookup.ContainsKey(s.species))
            {
                speciesLookup.Add(s.species, new List<FishSpeciesIdentity>());
                speciesLookup[s.species].Add(s);
            }
            else
            {
                speciesLookup[s.species].Add(s);
            }
        }
    }

    public IReadOnlyList<FishSpeciesIdentity> GetSpeciesInScene(Species species)
    {
        return speciesLookup[species];
    }
}