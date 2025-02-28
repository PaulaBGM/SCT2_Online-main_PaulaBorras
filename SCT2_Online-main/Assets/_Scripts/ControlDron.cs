using UnityEngine;

/// <summary>
/// Controla el movimiento de un dron utilizando el CharacterController en Unity.
/// Implementa aceleraci�n, frenado, colisi�n con techo y empuje de objetos din�micos.
/// Ahora el movimiento est� invertido.
/// </summary>
public class ControlDron : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movimiento")]
    public float maxSpeed = 5f;         // Velocidad m�xima del dron
    public float acceleration = 10f;    // Aceleraci�n del dron
    public float rotationSpeed = 100f;  // Velocidad de rotaci�n
    public float liftSpeed = 3f;        // Velocidad de elevaci�n

    private Vector3 velocity = Vector3.zero; // Velocidad actual del dron
    private bool isFalling = false;          // Estado de ca�da
    private float fallTimer = 0f;            // Tiempo en ca�da

    [Header("Gravedad")]
    public float gravity = 9.8f;             // Intensidad de la gravedad
    public float fallDuration = 1f;          // Tiempo m�ximo de ca�da antes de recuperar control

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Debug.Log(controller.height);
        if (isFalling)
        {
            ApplyGravity();
        }
        else
        {
            HandleMovement();
        }

        // Aplica el movimiento calculado al CharacterController
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Maneja el movimiento del dron basado en la entrada del usuario.
    /// Movimiento invertido en todas las direcciones.
    /// </summary>
    void HandleMovement()
    {
        // Captura la entrada del jugador
        float horizontal = Input.GetAxis("Horizontal"); // Rotaci�n
        float vertical = Input.GetAxis("Vertical");     // Adelante/Atr�s
        float lift = 0f;                                // Subir/Bajar

        if (Input.GetKey(KeyCode.Space)) lift = -1f; // Invertido
        if (Input.GetKey(KeyCode.LeftControl)) lift = 1f; // Invertido

        // Calcula la velocidad deseada en cada eje (invertida)
        Vector3 desiredVelocity = (-transform.forward * vertical * maxSpeed) + // Invertido
                                  (-transform.up * lift * liftSpeed);          // Invertido

        // Interpola la velocidad actual hacia la deseada suavemente
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime);

        // Aplica rotaci�n invertida
        transform.Rotate(Vector3.up * -horizontal * rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Aplica gravedad cuando el dron ha chocado contra el techo.
    /// Se activa por colisi�n superior y dura hasta tocar suelo o un tiempo l�mite.
    /// </summary>
    void ApplyGravity()
    {
        fallTimer += Time.deltaTime;
        velocity.y -= gravity * Time.deltaTime;  // Aplica ca�da

        if (fallTimer >= fallDuration || controller.isGrounded)
        {
            isFalling = false;
            fallTimer = 0f;
        }
    }

    /// <summary>
    /// Maneja colisiones del dron con objetos en la escena.
    /// </summary>
    /// <param name="hit">Informaci�n del objeto con el que se colision�.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Si el dron choca con un techo, activa la ca�da
        if ((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            isFalling = true;
        }

        // Si choca con un objeto din�mico, lo empuja horizontalmente
        if (hit.collider.attachedRigidbody != null)
        {
            Vector3 pushDirection = new Vector3(-velocity.x, 0, -velocity.z); // Invertido
            hit.collider.attachedRigidbody.velocity = pushDirection;
        }
    }
}
