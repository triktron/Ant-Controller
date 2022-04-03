using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AntMover : MonoBehaviour
{
    public float HorizontalSpeed = 5;
    public float VecticalSpeed = 2;
    public float RotateSpeed = 2;
    private AntController controller = null;

    public InputAction Horizontal;
    public InputAction Vertical;
    public InputAction Rotate;

    private void Start()
    {
        Horizontal.Enable();
        Vertical.Enable();
        Rotate.Enable();
        controller = GetComponent<AntController>();
    }

    private void OnDisable()
    {
        Horizontal.Disable();
        Vertical.Disable();
        Rotate.Disable();
    }

    void Update()
    {
        float yInput = Horizontal.ReadValue<float>();
        float xInput = Vertical.ReadValue<float>();
        float zInput = Rotate.ReadValue<float>();

        controller?.SetSpeed(Vector3.forward * HorizontalSpeed * yInput + Vector3.right * VecticalSpeed * xInput);
        controller?.SetRotationSpeed(Vector3.up * RotateSpeed * zInput);
    }
}
