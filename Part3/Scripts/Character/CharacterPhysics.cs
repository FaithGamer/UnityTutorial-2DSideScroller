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
    public Vector2 velocity
    {
        get
        {
            return m_velocity + addedVelocity;
        }
        private set { }
    }
    public Vector2 addedVelocity
    {
        get
        {
            Vector2 velocity = new Vector2(0, 0);
            foreach (Vector2 vel in m_addedVelocity)
            {
                velocity += vel;
            }
            return velocity;
        }
        private set
        {

        }
    }
    public State state
    {
        get
        {
            return m_state;
        }
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
    }
    public void SetMovingDirection(float direction)
    {
        if (m_movingDirection != direction)
        {
            m_movingDirection = direction;
            m_onCharacterChangeDirection?.Invoke(this, direction);
            if (m_movingDirection == 0 && m_grounded)
            {
                state = State.Idle;
            }
            else if (m_movingDirection != 0 && m_grounded)
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
    protected List<Vector2> m_addedVelocity = new List<Vector2>();
    protected bool m_grounded = false;
    protected float m_collisionMargin = 0.01f;
    protected float m_movingDirection;
    protected float m_moveDisabledTime = 0f;
    protected float m_moveDisabledTimer = 999f;

    //To avoid memory allocation each frame (Don't know how much useful it is):
    protected ContactFilter2D m_collisionFilter;
    protected List<RaycastHit2D> m_collisionsOccured = new List<RaycastHit2D>();

    private void Start()
    {
        var hurtable = GetComponent<Hurtable>();
        if (hurtable != null)
        {
            hurtable.HurtEvent += OnHurt;
        }
    }
    //Add bonus velocity that can overpass the max speed limit
    //It will be deccelerated by m_params.decceleration and removed when it reaches 0
    public void AddVelocity(Vector2 velocity)
    {
        m_addedVelocity.Add(velocity);
    }
    public void OnHurt(HurtEventData data)
    {
        //m_velocity = -m_velocity*2;

        Vector3 offset = transform.position - data.whosHurting.transform.position;
        offset = offset.normalized * data.parameters.pushPower;
        m_velocity = Vector2.zero;
        AddVelocity(new Vector2(offset.x, offset.y));
        DisableDirectionFor(0.3f);
    }
    public void DisableDirectionFor(float seconds)
    {
        m_moveDisabledTime = seconds;
        m_moveDisabledTimer = 0f;
    }
    public void EnableDirection()
    {
        m_moveDisabledTime = 0f;
        m_moveDisabledTimer = 999f;
    }
    public bool IsDirectionEnabled()
    {
        return m_moveDisabledTimer > m_moveDisabledTime;
    }
    public virtual void Jump()
    {
        m_velocity.y += m_params.jumpForce;
        state = State.Jump;
    }
    private void FixedUpdate()
    {

        Vector2 lastVelocity = m_velocity;

        //Processing velocity
        ComputeVelocity();
        ComputeAddedVelocity();
        //Move and collide, separating X movement from Y movement
        Vector2 offset = (m_velocity + addedVelocity) * Time.fixedDeltaTime;
        MoveAndCollide(offset * Vector2.up, true);
        MoveAndCollide(offset * Vector2.right, false);

        //Check if we started falling
        if (lastVelocity.y >= 0 && m_velocity.y < 0)
        {
            state = State.Fall;
        }

        //Disability to directional movement
        if (!IsDirectionEnabled())
        {
            m_moveDisabledTimer += Time.fixedDeltaTime;
        }
    }


    protected virtual void ComputeVelocity()
    {
        //Gravity pulling down
        m_velocity.y += m_params.gravityMod * Physics2D.gravity.y * Time.fixedDeltaTime;
        m_velocity.y = Mathf.Clamp(m_velocity.y, -m_params.maxVerticalSpeed, m_params.maxVerticalSpeed);

        float accel = 0f;

        //Directional acceleration
        if (IsDirectionEnabled())
        {
            if (m_grounded)
            {
                accel = m_movingDirection * m_params.acceleration * Time.fixedDeltaTime;
            }
            else
            {
                //We don't accelerate the same speed on the ground than on the air
                accel = m_movingDirection * m_params.airAcceleration * Time.fixedDeltaTime;
            }
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
    protected virtual void ComputeAddedVelocity()
    {
        //Deccelerate the Addeds velocities
        for (int i = m_addedVelocity.Count - 1; i >= 0; i--)
        {
            float xVel = 0;
            float yVel = 0;

            //Move towards zero
            if (m_addedVelocity[i].x > 0)
            {
                xVel = m_addedVelocity[i].x - m_params.decceleration * Time.fixedDeltaTime;
                xVel = Mathf.Max(0, xVel);
            }
            else if (m_addedVelocity[i].x < 0)
            {
                xVel = m_addedVelocity[i].x + m_params.decceleration * Time.fixedDeltaTime;
                xVel = Mathf.Min(0, xVel);
            }
            if (m_addedVelocity[i].y > 0)
            {
                yVel = m_addedVelocity[i].y - m_params.decceleration * Time.fixedDeltaTime;
                yVel = Mathf.Max(0, yVel);
            }
            else if (m_addedVelocity[i].y < 0)
            {
                yVel = m_addedVelocity[i].y + m_params.decceleration * Time.fixedDeltaTime;
                yVel = Mathf.Min(0, yVel);
            }

            //Remove the Added velocity when it reaches zero
            if (xVel < 0.001f && xVel > -0.001f && yVel < 0.001f && yVel > -0.001f)
            {
                m_addedVelocity.RemoveAt(i);
            }
            else
            {
                m_addedVelocity[i] = new Vector2(xVel, yVel);
            }
        }
    }
    protected void RemoveYFromAddedVelocity()
    {
        for (int i = 0; i < m_addedVelocity.Count; i++)
        {
            m_addedVelocity[i] = new Vector2(m_addedVelocity[i].x, 0f);
        }
    }
    protected void RemoveXFromAddedVelocity()
    {
        for (int i = 0; i < m_addedVelocity.Count; i++)
        {
            m_addedVelocity[i] = new Vector2(0f, m_addedVelocity[i].y);
        }
    }
    protected virtual void MoveAndCollide(Vector2 offset, bool up)
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
                RemoveYFromAddedVelocity();
             
                if(m_velocity.y < 0f)
                {  
                    m_velocity.y = 0f;
                }
                if (!m_grounded)
                {
                    m_onCharacterLand?.Invoke(this);
                    m_grounded = true;

                    if (m_movingDirection == 0)
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
                RemoveYFromAddedVelocity();
                //hitting a roof, slide along the roof
                float projection = Vector2.Dot(m_velocity, collision.normal);
                if (projection < 0)
                {
                    //   m_velocity -= projection * collision.normal;
                }
            }
            else
            {
                //wall collision
                RemoveXFromAddedVelocity();
            }

            float distanceLeft = collision.distance - m_collisionMargin;
            distance = distanceLeft < distance ? distanceLeft : distance;
        }

        //Apply the movement
        offset = offset.normalized * distance;

        m_body.position = m_body.position + offset;
    }

}
