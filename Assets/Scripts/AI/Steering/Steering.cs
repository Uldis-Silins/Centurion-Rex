using System.Collections;
using UnityEngine;

public class Steering
{
    public float angular;
    public Vector2 linear;

    public Steering()
    {
        angular = 0.0f;
        linear = new Vector2();
    }
}