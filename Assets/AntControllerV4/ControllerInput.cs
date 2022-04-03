using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : MonoBehaviour
{
    private AntController _Controller = null;

    public InputAction Horizontal;
    public InputAction Vertical;
    public InputAction Rotate;
    public InputAction Run;

    private void Start()
    {
        Horizontal.Enable();
        Vertical.Enable();
        Rotate.Enable();
        Run.Enable();
        _Controller = GetComponent<AntController>();
    }

    private void OnDisable()
    {
        Horizontal.Disable();
        Vertical.Disable();
        Rotate.Disable();
        Run.Disable();
    }

    void Update()
    {
        var input = new ControllerInputStruct(Horizontal.ReadValue<float>(), Vertical.ReadValue<float>(), Rotate.ReadValue<float>(), Run.ReadValue<float>());

        _Controller?.SetInput(input);
    }
}
