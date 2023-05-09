using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPhysics : CharacterPhysics
{
    void OnMove(InputValue input)
    {
        float direction = input.Get<Vector2>().x;
        direction = Mathf.Round(direction);
        SetMovingDirection(direction);
    }
    void OnJump(InputValue input)
    {
        if(m_grounded)
        {
            Jump();
        }
    }
}