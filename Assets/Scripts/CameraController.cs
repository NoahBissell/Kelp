using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Extended by Mahelita 08-01-18.
        Added up and down movement using e and q, respectively.
        Adjusted parameters to fit my needs.
    Updated to the new Unity input sytem by @bison_42 / bison-- 2022-01-02
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    qe : Move camera down or up, respectively
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height
    */


    public float mainSpeed = 5.0f; //regular speed
    public float shiftAdd = 25f; //multiplied by how long shift is held.  Basically running
    public float maxShift = 100.0f; //Maximum speed when holdin gshift
    public float camSens = 0.25f; //How sensitive it with mouse
    public bool rotateOnlyIfMousedown = true;
    public bool movementStaysFlat = false;

    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    void Update()
    {

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            lastMouse = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0); // $CTK reset when we begin
        }

        if (!rotateOnlyIfMousedown ||
            (rotateOnlyIfMousedown && Mouse.current.rightButton.IsPressed()))
        {
            lastMouse = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0) - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Mouse.current.position.ReadValue();
            //Mouse  camera angle done.  
        }

        //Keyboard commands
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (Keyboard.current.leftShiftKey.IsPressed())
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Keyboard.current.spaceKey.IsPressed()
            || (movementStaysFlat && !(rotateOnlyIfMousedown && Mouse.current.rightButton.IsPressed())))
        { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }

    }

    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Keyboard.current.wKey.IsPressed())
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Keyboard.current.sKey.IsPressed())
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Keyboard.current.aKey.IsPressed())
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Keyboard.current.dKey.IsPressed())
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Keyboard.current.qKey.IsPressed())
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Keyboard.current.eKey.IsPressed())
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        return p_Velocity;
    }
}