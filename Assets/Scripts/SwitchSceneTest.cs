using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchSceneTest : MonoBehaviour
{
    public string sceneToLoad;
    
    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneLoader.instance.LoadScene(sceneToLoad);
        }
    }
}