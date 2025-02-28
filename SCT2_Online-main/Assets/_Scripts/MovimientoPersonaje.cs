using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoPersonaje : MonoBehaviour
{
    private CharacterController ch_Controller;
    private float gravity = -9.8f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField] private float crouchSpeed = 1.0f;
    private float currentSpeed = 0f;
    private float stickToGroundSpeed = -3f;

    [Header("Gravity")]
    private Vector3 playerVelocity;
    private float verticalVelocity;

    [Header("Crouch")]
    private bool isCrouched = false;
    private float originalHeight;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private Transform headCheck;
    private float headCheckDistance = 2f;

    [Header("Dash")]
    private bool isDashing = false;
    private float dashTime;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashSpeed = 7f;
    private Vector3 dashDirection;

    void Start()
    {
        ch_Controller = GetComponent<CharacterController>();
        originalHeight = ch_Controller.height;
    }

    void Update()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            UpdatePlayerVelocity();
            ApplyGravity();
            ApplyVelocity();
            HandleCrouch();
            if (Input.GetKeyDown(KeyCode.Space)) StartDash();
        }
    }

    void ApplyVelocity()
    {
        Vector3 totalVelocity = playerVelocity + verticalVelocity * Vector3.up;
        ch_Controller.Move(totalVelocity * Time.deltaTime);
    }

    void UpdatePlayerVelocity()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        Vector3 vectorInput = new Vector3(xInput, 0, zInput);

        if (vectorInput.sqrMagnitude > 1)
        {
            vectorInput.Normalize();
        }

        float targetSpeed = isCrouched ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5);

        Vector3 localPlayerVelocity = vectorInput * currentSpeed;
        playerVelocity = transform.TransformVector(localPlayerVelocity);
    }

    void ApplyGravity()
    {
        if (ch_Controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = stickToGroundSpeed;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ch_Controller.height = crouchHeight;
            isCrouched = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (!Physics.Raycast(headCheck.position, Vector3.up, headCheckDistance))
            {
                ch_Controller.height = originalHeight;
                isCrouched = false;
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = 0;
        dashDirection = playerVelocity.sqrMagnitude > 0 ? playerVelocity.normalized : transform.forward;
    }

    void HandleDash()
    {
        dashTime += Time.deltaTime;
        ch_Controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        if (dashTime >= dashDuration)
        {
            isDashing = false;
        }
    }
}
