using System;
using EasyButtons;
using KBCore.Refs;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.UI;
public class SpeciesCameraManager : MonoBehaviour
{
    public Animator uiAnimator;
    public TextMeshProUGUI speciesNameText;
    public TextMeshProUGUI speciesDescriptionText;
    public TextMeshProUGUI speciesSciNameText;
    public TextMeshProUGUI speciesBehaviorText;

    // public AnimationCurve uiFadeCurve;

    public Species[] cycleSpecies;

    public CinemachineCamera camera;

    private int index;
    public Species currentSpecies;

    [SerializeField, Scene] private SpeciesManager registry;
    public bool keyboardSwitch;

    private void OnValidate()
    {
        this.ValidateRefs();
    }

    private void Update()
    {
        if (keyboardSwitch && Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            index = (index + 1) % cycleSpecies.Length;
            SetNewSpecies(cycleSpecies[index]);
        }
    }

    [Button]
    public void SetNewSpecies(Species species)
    {
        this.currentSpecies = species;
        
        uiAnimator.SetTrigger("Change");

        var fishes = registry.GetSpeciesInScene(species);
        var fish = fishes[Random.Range(0, fishes.Count)];

        
        camera.Target = new CameraTarget()
        {
            TrackingTarget = fish.transform,
        };
    }

    [Button]
    public void ChangeUI()
    {
        speciesNameText.text = currentSpecies.displayName;
        speciesDescriptionText.text = currentSpecies.description;
        speciesSciNameText.text = currentSpecies.scientificName;
        speciesBehaviorText.text = currentSpecies.behavior;
    }

    
}