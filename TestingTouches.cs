using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.InputSystem;

public class TestingTouches : MonoBehaviour
{
    //public InputMaster inputControl;
    public Text delta;
    public Text pressed;

    private void Awake()
    {
        //inputControl = new InputMaster();
        //inputControl.Menu.Swipe.performed += ctx => Testing(ctx.ReadValue<Vector2>());
        //inputControl.Menu.Buttons.performed += ctx => TestingPress();
    }

    public void Testing(Vector2 value)
    {
        delta.text = value.ToString();
    }

    public void TestingPress()
    {
        pressed.text = "Is Pressed";
    }

    //public void OnEnable()
    //{
    //    inputControl.Enable();
    //}

    //public void OnDisable()
    //{
    //    inputControl.Disable();
    //}
}
