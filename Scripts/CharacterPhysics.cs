using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : MonoBehaviour
{
    //Changing state events
    public event OnCharacterFall m_onCharacterFall;
    public event OnCharacterJump m_onCharacterJump;
    public event OnCharacterIdle m_onCharacterIdle;
    public event OnCharacterRun m_onCharacterRun;

    public delegate void OnCharacterFall(object sender);
    public delegate void OnCharacterJump(object sender);
    public delegate void OnCharacterIdle(object sender);
    public delegate void OnCharacterRun(object sender);

    //Other physics events
    public event OnCharacterChangeDirection m_onCharacterChangeDirection;
    public event OnCharacterLand m_onCharacterLand;

    public delegate void OnCharacterChangeDirection(object sender, float direction);
    public delegate void OnCharacterLand(object sender);
    public Vector2 velocity => m_velocity;

    public State state
    {
        private set
        {
            if (value == State.Idle && m_state != State.Idle)
            {
                m_state = value;
                m_onCharacterIdle?.Invoke(this);
            }
            else if (value == State.Run && m_state != State.Run)
            {
                m_state = value;
                m_onCharacterRun?.Invoke(this);
            }
            else if (value == State.Jump && m_state != State.Jump)
            {
                m_state = value;
                m_grounded = false;
                m_onCharacterJump?.Invoke(this);
            }
            else if (value == State.Fall && m_state != State.Fall)
            {
                m_state = value;
                m_grounded = false;
                m_onCharacterFall?.Invoke(this);
            }
        }
        get
        {
            return m_state;
        }
    }
    public void SetMovingDirection(float direction)
    {
        if(m_movingDirection != direction)
        {
            m_movingDirection = direction;
            m_onCharacterChangeDirection?.Invoke(this, direction);
            if(m_movingDirection == 0 && m_grounded)
            {
                state = State.Idle;
            }
            else if(m_movingDirection != 0 && m_grounded)
            {
                state = State.Run;
            }
        }
    }
    [SerializeField] protected CharacterPhysicsParameters m_params;
    [SerializeField] protected Rigidbody2D m_body;

    public enum State
    {
        Idle,
        Run,
        Jump,
        Fall
    }
    protected State m_state = State.Idle; //Use state for setter only

    protected Vector2 m_velocity;
    protected bool m_grounded = false;
    protected float m_collisionMargin = 0.01f;
    protected float m_movingDirection;

    //To avoid memory allocation each frame (Don't know how much useful it is):
    protected ContactFilter2D m_collisionFilter;
    protected List<RaycastHit2D> m_collisionsOccured = new List<RaycastHit2D>();

    private void FixedUpdate()
    {
    
        Vector2 lastVelocity = m_velocity;

        //Processing velocity
        ComputeVelocity();
        Vector2 offset = m_velocity * Time.fixedDeltaTime;
        MoveAndCollide(offset * Vector2.up);
        MoveAndCollide(offset * Vector2.right);

        //Check if we started falling
        if(lastVelocity.y >= 0 && m_velocity.y < 0)
        {
            state = State.Fall;
        }
    }
    protected virtual void Jump()
    {   
        m_velocity.y += m_params.jumpForce;
        state = State.Jump;
    }
    protected virtual void ComputeVelocity()
    {
        //Gravity pulling down
        m_velocity.y += m_params.gravityMod * Physics2D.gravity.y * Time.fixedDeltaTime;
        m_velocity.y = Mathf.Clamp(m_velocity.y, -m_params.maxVerticalSpeed, m_params.maxVerticalSpeed);

        float accel = 0f;

        //Directional acceleration
        if (m_grounded)
        {
            accel = m_movingDirection * m_params.acceleration * Time.fixedDeltaTime;
        }
        else
        {
            //We don't accelerate the same speed on the ground than on the air
            accel = m_movingDirection * m_params.airAcceleration * Time.fixedDeltaTime;
        }

        m_velocity.x += accel;

        //Max speed clamping
        m_velocity.x = Mathf.Clamp(m_velocity.x, -m_params.maxHorizontalSpeed, m_params.maxHorizontalSpeed);

        //Deceleration if we are not accelerating 
        if (m_movingDirection == 0f)
        {
            if (m_velocity.x > 0)
            {
                m_velocity.x -= m_params.decceleration * Time.fixedDeltaTime;
                m_velocity.x = Mathf.Max(0, m_velocity.x);
            }
            else if (m_velocity.x < 0)
            {
                m_velocity.x += m_params.decceleration * Time.fixedDeltaTime;
                m_velocity.x = Mathf.Min(0, m_velocity.x);
            }
        }
    }
    protected virtual void MoveAndCollide(Vector2 offset)
    {
        float distance = offset.magnitude;

        //Raycasting to get collisions
        m_body.Cast(offset.normalized, m_collisionFilter, m_collisionsOccured, distance + m_collisionMargin);

        //Iterate each collision
        foreach (RaycastHit2D collision in m_collisionsOccured)
        {
            Vector2 position = collision.point;

            //Normal of the collision is a perpendicular, normalized vector, of the collision surface.
            //By checking the Y component of the normal vector,
            //We can evaluate the slope of the surface we hit.
            //0 = wall; <0 = roof; >0 = floor; 0.1 = floor with steep slope; 0.9 = floor light slope.
            if (collision.normal.y > 0)
            {
                m_velocity.y = 0f;
                if (!m_grounded)
                {
                    m_onCharacterLand?.Invoke(this);
                    m_grounded = true;

                    if(m_movingDirection == 0)
                    {
                        state = State.Idle;
                    }
                    else
                    {
                        state = State.Run;
                    }
                }
            }
            else if (collision.normal.y < 0)
            {
                //hitting a roof, slide along the roof
                float projection = Vector2.Dot(m_velocity, collision.normal);
                if (projection < 0)
                {
                 //   m_velocity -= projection * collision.normal;
                }
            }

            float distanceLeft = collision.distance - m_collisionMargin;
            distance = distanceLeft < distance ? distanceLeft : distance;
        }

        //Apply the movement
        offset = offset.normalized * distance;

        m_body.position = m_body.position + offset;
    }

}
