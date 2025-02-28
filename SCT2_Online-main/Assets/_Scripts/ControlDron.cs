using UnityEngine;

/// <summary>
/// Controla el movimiento de un dron utilizando el CharacterController en Unity.
/// Implementa aceleración, frenado, colisión con techo y empuje de objetos dinámicos.
/// Ahora el movimiento está invertido.
/// </summary>
public class ControlDron : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movimiento")]
    public float maxSpeed = 5f;         // Velocidad máxima del dron
    public float acceleration = 10f;    // Aceleración del dron
    public float rotationSpeed = 100f;  // Velocidad de rotación
    public float liftSpeed = 3f;        // Velocidad de elevación

    private Vector3 velocity = Vector3.zero; // Velocidad actual del dron
    private bool isFalling = false;          // Estado de caída
    private float fallTimer = 0f;            // Tiempo en caída

    [Header("Gravedad")]
    public float gravity = 9.8f;             // Intensidad de la gravedad
    public float fallDuration = 1f;          // Tiempo máximo de caída antes de recuperar control

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
        float horizontal = Input.GetAxis("Horizontal"); // Rotación
        float vertical = Input.GetAxis("Vertical");     // Adelante/Atrás
        float lift = 0f;                                // Subir/Bajar

        if (Input.GetKey(KeyCode.Space)) lift = -1f; // Invertido
        if (Input.GetKey(KeyCode.LeftControl)) lift = 1f; // Invertido

        // Calcula la velocidad deseada en cada eje (invertida)
        Vector3 desiredVelocity = (-transform.forward * vertical * maxSpeed) + // Invertido
                                  (-transform.up * lift * liftSpeed);          // Invertido

        // Interpola la velocidad actual hacia la deseada suavemente
        velocity = Vector3.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime);

        // Aplica rotación invertida
        transform.Rotate(Vector3.up * -horizontal * rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Aplica gravedad cuando el dron ha chocado contra el techo.
    /// Se activa por colisión superior y dura hasta tocar suelo o un tiempo límite.
    /// </summary>
    void ApplyGravity()
    {
        fallTimer += Time.deltaTime;
        velocity.y -= gravity * Time.deltaTime;  // Aplica caída

        if (fallTimer >= fallDuration || controller.isGrounded)
        {
            isFalling = false;
            fallTimer = 0f;
        }
    }

    /// <summary>
    /// Maneja colisiones del dron con objetos en la escena.
    /// </summary>
    /// <param name="hit">Información del objeto con el que se colisionó.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Si el dron choca con un techo, activa la caída
        if ((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            isFalling = true;
        }

        // Si choca con un objeto dinámico, lo empuja horizontalmente
        if (hit.collider.attachedRigidbody != null)
        {
            Vector3 pushDirection = new Vector3(-velocity.x, 0, -velocity.z); // Invertido
            hit.collider.attachedRigidbody.velocity = pushDirection;
        }
    }
}
