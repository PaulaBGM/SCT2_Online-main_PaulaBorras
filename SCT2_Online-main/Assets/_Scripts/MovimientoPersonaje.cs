using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoPersonaje : MonoBehaviour
{
    private CharacterController ch_Controller;
    private float gravity = -9.8f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.5f;  // Velocidad al caminar
    [SerializeField] private float runSpeed = 3.5f;   // Velocidad al correr
    [SerializeField] private float crouchSpeed = 1.0f; // Velocidad al agacharse
    private float currentSpeed = 0f;                   // Velocidad actual del jugador
    private float stickToGroundSpeed = -3f;            // Velocidad de adherencia al suelo

    [Header("Gravity")]
    private Vector3 playerVelocity;  // Velocidad del jugador
    private float verticalVelocity;  // Velocidad en el eje vertical (gravedad)

    [Header("Crouch")]
    private bool isCrouched = false;          // Indica si el personaje está agachado
    private float originalHeight;              // Altura original del personaje
    [SerializeField] private float crouchHeight = 1f; // Altura cuando está agachado
    [SerializeField] private Transform headCheck;    // Transformación para verificar la cabeza
    private float headCheckDistance = 2f;     // Distancia de verificación de la cabeza

    [Header("Dash")]
    private bool isDashing = false;             // Indica si el personaje está corriendo
    private float dashTime;                     // Tiempo de dash
    [SerializeField] private float dashDuration = 0.2f; // Duración del dash
    [SerializeField] private float dashSpeed = 7f;  // Velocidad del dash
    private Vector3 dashDirection;              // Dirección del dash

    void Start()
    {
        // Inicialización del CharacterController y altura original
        ch_Controller = GetComponent<CharacterController>();
        originalHeight = ch_Controller.height;
    }

    void Update()
    {
        // Si el personaje está en "dash", manejarlo
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            // Si no está en dash, actualizar la velocidad del jugador y aplicar gravedad
            UpdatePlayerVelocity();
            ApplyGravity();
            ApplyVelocity();
            HandleCrouch();

            // Iniciar el dash al presionar la barra espaciadora
            if (Input.GetKeyDown(KeyCode.Space)) StartDash();
        }
    }

    // Aplica la velocidad del jugador al CharacterController
    void ApplyVelocity()
    {
        // Calcula la velocidad total sumando la velocidad vertical (gravedad)
        Vector3 totalVelocity = playerVelocity + verticalVelocity * Vector3.up;
        // Mueve al personaje usando el CharacterController
        ch_Controller.Move(totalVelocity * Time.deltaTime);
    }

    // Actualiza la velocidad del jugador según las entradas del usuario
    void UpdatePlayerVelocity()
    {
        // Obtiene las entradas horizontales y verticales del jugador
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        Vector3 vectorInput = new Vector3(xInput, 0, zInput);

        // Normaliza el vector de movimiento si es necesario para que no se mueva más rápido en diagonal
        if (vectorInput.sqrMagnitude > 1)
        {
            vectorInput.Normalize();
        }

        // Determina la velocidad actual según si está agachado, caminando o corriendo
        float targetSpeed = isCrouched ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5);

        // Convierte la velocidad en un vector global para el movimiento del jugador
        Vector3 localPlayerVelocity = vectorInput * currentSpeed;
        playerVelocity = transform.TransformVector(localPlayerVelocity);
    }

    // Aplica la gravedad, haciendo que el personaje caiga si no está en el suelo
    void ApplyGravity()
    {
        if (ch_Controller.isGrounded && verticalVelocity < 0)
        {
            // Si está en el suelo, establece la velocidad vertical a un valor para adherir al suelo
            verticalVelocity = stickToGroundSpeed;
        }
        else
        {
            // Si no está en el suelo, aplica la gravedad
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    // Maneja la acción de agacharse
    void HandleCrouch()
    {
        // Si el jugador presiona la tecla Left Control, el personaje se agacha
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ch_Controller.height = crouchHeight;
            isCrouched = true;
        }
        // Si se suelta Left Control, el personaje se levanta (si no hay obstáculos)
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (!Physics.Raycast(headCheck.position, Vector3.up, headCheckDistance))
            {
                ch_Controller.height = originalHeight;
                isCrouched = false;
            }
        }
    }

    // Inicia el dash cuando se presiona la barra espaciadora
    void StartDash()
    {
        isDashing = true;
        dashTime = 0;
        // Si el jugador tiene velocidad, el dash va en la dirección de su movimiento
        dashDirection = playerVelocity.sqrMagnitude > 0 ? playerVelocity.normalized : transform.forward;
    }

    // Maneja el dash durante su duración
    void HandleDash()
    {
        dashTime += Time.deltaTime;
        // Mueve al personaje en la dirección del dash
        ch_Controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        // Termina el dash si se ha alcanzado la duración
        if (dashTime >= dashDuration)
        {
            isDashing = false;
        }
    }
}
