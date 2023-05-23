using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator m_animator;
    private SpriteRenderer m_sprite;
    private bool m_blinking = false;
    private float m_blinkingTime = 0.1f;
    private float m_blinkingTimer = 0f;
    private void Start()
    {
        m_animator = GetComponent<Animator>();
        m_sprite = GetComponent<SpriteRenderer>();

        var physics = GetComponent<CharacterPhysics>();
        if (physics != null)
        {
            physics.m_onCharacterChangeDirection += OnChangeDirection;
            physics.m_onCharacterFall += OnFall;
            physics.m_onCharacterIdle += OnIdle;
            physics.m_onCharacterJump += OnJump;
            physics.m_onCharacterRun += OnRun;
            physics.m_onCharacterLand += OnLand;
        }
        else
        {
            Debug.LogWarning("CharacterAnimation cannot find CharacterPhysics.");
        }

        var hurtable = GetComponent<Hurtable>();

        if (hurtable != null)
        {
            hurtable.EnterGraceEvent += OnEnterGrace;
            hurtable.ExitGraceEvent += OnExitGrace;
        }
    }
    private void Update()
    {
        if (m_blinking)
        {
            Blink();
        }
    }
    private void Blink()
    {
        m_blinkingTimer += Time.deltaTime;
        if (m_blinkingTimer > m_blinkingTime)
        {
            m_sprite.color = new Color(1, 1, 1, 1);

        }
        else
        {
            m_sprite.color = new Color(0, 0, 0, 0);
        }
        if (m_blinkingTimer > m_blinkingTime * 2)
            m_blinkingTimer = 0f;
    }
    public void OnEnterGrace(HurtEventData data)
    {
        StartBlinking();
    }
    public void OnExitGrace(HurtEventData data)
    {
        StopBlinking();
    }
    public void StartBlinking()
    {
        m_blinking = true;
        m_blinkingTimer = 0f;
    }
    public void StopBlinking()
    {
        m_blinking = false;
        m_sprite.color = new Color(1, 1, 1, 1);
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
