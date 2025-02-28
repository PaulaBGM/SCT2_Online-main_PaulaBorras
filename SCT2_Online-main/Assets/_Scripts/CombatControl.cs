using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatControl : MonoBehaviour
{
    [SerializeField] Collider weaponTrigger;

    Animator _cmpAnimator;
    CharacterController _cmpCc;

    // Start is called before the first frame update
    void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        _cmpAnimator = GetComponent<Animator>();
        _cmpCc = GetComponent<CharacterController>();

        weaponTrigger.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        _cmpAnimator.SetBool("moving", _cmpCc.velocity.sqrMagnitude > 1f);

        if(Input.GetMouseButtonDown(0))
        {
            _cmpAnimator.SetTrigger("attack");
        }
        else
        {
            _cmpAnimator.ResetTrigger("attack");
        }
        if (Input.GetMouseButtonDown(1))
        {
            _cmpAnimator.SetTrigger("strongAttack");
        }
        else
        {
            _cmpAnimator.ResetTrigger("strongAttack");
        }
    }

    void AttackAnimEvent()
    {
        weaponTrigger.enabled = true;
    }

    void EndAttackAnimEvent()
    {
        weaponTrigger.enabled = false;
    }
}
