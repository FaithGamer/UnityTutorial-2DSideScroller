using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPhysics", menuName = "MyAssets/CharacterPhysics")]
public class CharacterPhysicsParameters : ScriptableObject
{
    public float maxVerticalSpeed;
    public float maxHorizontalSpeed;
    public float acceleration;
    public float decceleration;
    public float airAcceleration;
    public float jumpForce;
    public float gravityMod;
}