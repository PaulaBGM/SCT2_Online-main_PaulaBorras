using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerControl : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.8f;
    [Header("Movement")]    
    [SerializeField] private float forwardSpeed = 5;
    [SerializeField] private float sideSpeed = 2;
    [SerializeField] private float stickToGroundSpeed = -3;
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float rotationSpeed = 180;
    [SerializeField] private bool useJumpAnimEvent = true;
    [SerializeField] private float endJumpRaycastDistance = 3;
    [Header("Sliding")]
    [SerializeField] private float slideSlope = 45;
    [SerializeField] private float slideSpeed = 6;
    [SerializeField] private float slideSlowdownTime = 2;
    [SerializeField] private AnimationCurve slideSlowDownCurve = AnimationCurve.EaseInOut(0,1,1,0);

    private Animator _cmpAnimator;
    private CharacterController _cmpCc;

    private Vector3 _playerVelocity;
    private float _verticalVelocity;
    private Vector3 _slideVelocity;

    private bool _jumping;
    private bool _jumpEnded = true;
    private bool _waitingForJumpAnimEvent;

    private bool _sliding;
    private float _slidingTime;
    private float _slidePlayerVelocityFactor = 1;

    public bool useRootMotion = true;

    private static readonly int Jump = Animator.StringToHash("jump");
    private static readonly int ZSpeed = Animator.StringToHash("zSpeed");
    private static readonly int XSpeed = Animator.StringToHash("xSpeed");
    private static readonly int Crouched = Animator.StringToHash("crouched");
    
    private void Start()
    {
        _cmpCc = GetComponent<CharacterController>();
        _cmpAnimator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        UpdatePlayerVelocity();
        UpdateVerticalVelocity();
        UpdateSlideVelocity();

        ApplyTotalVelocity();
        
        Crouch();
        
        UpdateRotation();
    }

    private void ApplyTotalVelocity()
    {
        // Muevo al personaje combinando los ejes de movimiento (XZ e Y)
        // en una única llamada para que detecte el suelo y colisiones correctamente
        Vector3 totalVelocity = _playerVelocity * _slidePlayerVelocityFactor + _slideVelocity + Vector3.up * _verticalVelocity;
        Debug.DrawRay(this.transform.position, Vector3.ProjectOnPlane(totalVelocity, Vector3.up), Color.magenta, 3);
        if (_jumping || !useRootMotion)
        {
            _cmpCc.Move(totalVelocity * Time.deltaTime);
        }
    }

    private void UpdateRotation()
    {
        float mouseXInput = Input.GetAxis("Mouse X");
        transform.Rotate(0, mouseXInput * rotationSpeed * Time.deltaTime, 0);
    }

    private void UpdatePlayerVelocity()
    {
        // Accedo al input de las flechas/WASD
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // Combino el input y lo normalizo para que no se mueva más rápido en diagonal
        Vector3 input = new Vector3(xInput, 0, zInput);
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
        
        // Calculo la velocidad y la convierto a local al personaje
        Vector3 localPlayerVelocity = new Vector3(input.x * sideSpeed, 0, input.z * forwardSpeed);
        _playerVelocity = transform.TransformVector(localPlayerVelocity);

        // Le paso al animator la velocidad de movimiento del personaje en cada eje
        _cmpAnimator.SetFloat(ZSpeed, localPlayerVelocity.z);
        _cmpAnimator.SetFloat(XSpeed, localPlayerVelocity.x);
    }

    private void UpdateVerticalVelocity()
    {
        // Si pulso espacio, activo la animación de salto. Esta debe tener un evento Jump que producirá el salto.
        if (Input.GetAxisRaw("Jump") > 0.5f && _cmpCc.isGrounded && !_sliding && !_jumping && !_waitingForJumpAnimEvent)
        {
            _jumping = true;            
            _jumpEnded = false;
            
            _cmpAnimator.SetTrigger(Jump);
            _waitingForJumpAnimEvent = useJumpAnimEvent;
            if(!_waitingForJumpAnimEvent) { 
                _verticalVelocity = jumpForce;
                _slidingTime = slideSlowdownTime;
            }            
        }
        // Si no estoy saltando, y estoy en el suelo, reseteo la velocidad
        // a un valor negativo pequeño, para mantenerme pegado al suelo
        if (!_waitingForJumpAnimEvent && _jumpEnded && _verticalVelocity < 0 && _cmpCc.isGrounded)
        {
            _jumping = false;            
            _verticalVelocity = stickToGroundSpeed;
        }

        // Calculo si debería empezar a reproducir la animación de final de salto
        if (_jumping && !_jumpEnded && !_waitingForJumpAnimEvent && _verticalVelocity < 0)
        {
            if(Physics.Raycast(transform.position, Vector3.down, endJumpRaycastDistance))
            {
                _jumpEnded = true;
                _cmpAnimator.SetTrigger("endJump");                
            }            
        }

        // En todo momento, aplico la gravedad reduciendo la velocidad en Y
        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void JumpAnimEvent()
    {        
        if(_waitingForJumpAnimEvent) { 
            _verticalVelocity = jumpForce;
            _slidingTime = slideSlowdownTime;
            _waitingForJumpAnimEvent = false; 
        }
    }

    private void UpdateSlideVelocity()
    {
        Vector3 maxSlideVelocity = Vector3.zero;
        RaycastHit hitInfo;
        if (_cmpCc.isGrounded && Physics.SphereCast(transform.position + _cmpCc.center, _cmpCc.radius, Vector3.down, out hitInfo))
        {
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
            //print(angle);
            if (angle > slideSlope)
            {
                _sliding = true;
                
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal).normalized;
                maxSlideVelocity = slideDirection * slideSpeed;

                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.blue, 3);
                Debug.DrawRay(hitInfo.point, slideDirection, Color.red, 3);
            }
            else
            {
                _sliding = false;
                _slidingTime = 0;
            }
        }

        if(_sliding) { _slidingTime += Time.deltaTime; }
        
        _slideVelocity = _sliding
            ? Vector3.Lerp(_slideVelocity, maxSlideVelocity, Time.deltaTime * 3) 
            : Vector3.Lerp(_slideVelocity, Vector3.zero, Time.deltaTime * 5);

        _slidePlayerVelocityFactor = _sliding
            ? slideSlowDownCurve.Evaluate(Mathf.Clamp01(_slidingTime / slideSlowdownTime))
            : Mathf.Lerp(_slidePlayerVelocityFactor, 1, 10 * Time.deltaTime);        

    }

    private void Crouch()
    {
        if (Input.GetAxisRaw("Crouch") > 0.5f)
        {
            _cmpAnimator.SetBool(Crouched, true);
        }
        else
        {
            _cmpAnimator.SetBool(Crouched, false);
        }
    }
    
    private void OnAnimatorMove()
    {
        if(!_jumping && useRootMotion)
        {
            Vector3 rootMotionMove = _cmpAnimator.rootPosition - transform.position;
            Vector3 totalMovement = rootMotionMove * _slidePlayerVelocityFactor +
                                    _slideVelocity * Time.deltaTime +
                                    Vector3.up * _verticalVelocity * Time.deltaTime;
            _cmpCc.Move(totalMovement);

            transform.rotation = _cmpAnimator.rootRotation;
        }        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.attachedRigidbody == null) return;
        
        var hitRb = hit.collider.attachedRigidbody;
        var pushForce = Random.Range(1f, 4f);
        
        hitRb.AddForce(hit.moveDirection * pushForce, ForceMode.Impulse);
    }
}
