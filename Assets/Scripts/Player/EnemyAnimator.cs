using EasyCharacterMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private EnemyController _controller;
    private Animator _animator;

    private int _isWalkingAnimation;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _controller = GetComponent<EnemyController>();
        _isWalkingAnimation = Animator.StringToHash("IsWalking");
    }

    private void FixedUpdate()
    {
        _animator.SetBool(_isWalkingAnimation, _controller.GetSpeed() != 0f);
    }

}
