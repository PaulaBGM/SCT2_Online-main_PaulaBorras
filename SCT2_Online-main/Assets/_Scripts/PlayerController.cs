using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator _anim;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetFloat("Yvel", Input.GetAxis("Vertical"));
        _anim.SetFloat("Xvel", Input.GetAxis("Horizontal"));
    }
}
