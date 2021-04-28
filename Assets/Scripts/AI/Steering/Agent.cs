using System.Collections;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float maxSpeed = 1.0f;
    public float trueMaxSpeed;
    public float maxAccel = 30.0f;

    public float orientation;
    public float rotation;
    public Vector2 velocity;
    protected Steering steering;

    public float maxRotation = 45.0f;
    public float maxAngularAccel = 45.0f;

    private void Start()
    {
        velocity = Vector2.zero;
        steering = new Steering();
        trueMaxSpeed = maxSpeed;
    }

    public void SetSteering(Steering steering, float weight)
    {
        this.steering.linear += (weight * steering.linear);
        this.steering.angular += (weight * steering.angular);
    }

    protected virtual void Update()
    {
        Vector2 displacement = velocity * Time.deltaTime;

        orientation += rotation * Time.deltaTime;

        if (orientation < 0.0f)
        {
            orientation += 360.0f;
        }
        else if (orientation > 360.0f)
        {
            orientation -= 360.0f;
        }
        transform.Translate(displacement, Space.World);
        transform.rotation = new Quaternion();
        transform.Rotate(Vector3.forward, orientation);
    }

    protected virtual void LateUpdate()
    {
        velocity += steering.linear * Time.deltaTime;
        rotation += steering.angular * Time.deltaTime;
        if (velocity.magnitude > maxSpeed)
        {
            velocity.Normalize();
            velocity = velocity * maxSpeed;
        }

        if (steering.linear.magnitude == 0.0f)
        {
            velocity = Vector3.zero;
        }

        steering = new Steering();
    }

    //used for speed matching when traveling in groups
    public void ResetSpeed()
    {
        maxSpeed = trueMaxSpeed;
    }
}