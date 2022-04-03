using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AntControllerInput : MonoBehaviour
{
    public float HorizontalSpeed = 5;
    public float VecticalSpeed = 2;
    public float RotateSpeed = 2;
    public float RunSpeed = 2;
    private AntController controller = null;

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
        controller = GetComponent<AntController>();
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
        float yInput = Horizontal.ReadValue<float>();
        float xInput = Vertical.ReadValue<float>();
        float zInput = Rotate.ReadValue<float>();
        float run = Run.ReadValue<float>();

        var pnr = new AntController.PositionAndRotation()
        {
            Position = Vector3.forward * Mathf.Lerp(HorizontalSpeed, RunSpeed, run) * yInput + Vector3.right * VecticalSpeed * xInput,
            Rotation = Vector3.up * RotateSpeed * zInput
        };

        controller?.SetPNR(pnr);
    }
}
