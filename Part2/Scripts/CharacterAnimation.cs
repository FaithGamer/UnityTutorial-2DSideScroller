using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private CharacterPhysics m_physics;
    private Animator m_animator;

    void Start()
    {

        m_physics = GetComponent<CharacterPhysics>();
        m_animator = GetComponent<Animator>();

        m_physics.m_onCharacterChangeDirection += OnChangeDirection;
        m_physics.m_onCharacterFall += OnFall;
        m_physics.m_onCharacterIdle += OnIdle;
        m_physics.m_onCharacterJump += OnJump;
        m_physics.m_onCharacterRun += OnRun;
        m_physics.m_onCharacterLand += OnLand;
    }

    private void OnChangeDirection(object sender, float direction)
    {
        if (direction < 0f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction > 0f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private void OnFall(object sender)
    {
        m_animator.SetInteger("State", (int)CharacterPhysics.State.Fall);
    }

    private void OnIdle(object sender)
    {
        m_animator.SetInteger("State", (int)CharacterPhysics.State.Idle);
    }
    private void OnJump(object sender)
    {
        m_animator.SetInteger("State", (int)CharacterPhysics.State.Jump);
    }
    private void OnRun(object sender)
    {
        m_animator.SetInteger("State", (int)CharacterPhysics.State.Run);
    }
    private void OnLand(object sender)
    {
        m_animator.SetTrigger("Land");
    }
}
