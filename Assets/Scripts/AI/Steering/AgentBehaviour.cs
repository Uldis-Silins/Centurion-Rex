using System.Collections;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{
    public float weight = 1.0f;

    [SerializeField] protected Vector3 m_moveTarget;
    protected Agent agent;
    public Vector3 dest;

    public float maxSpeed = 50.0f;
    public float maxAccel = 50.0f;
    public float maxRotation = 5.0f;
    public float maxAngularAccel = 5.0f;

    public Vector3 MoveTarget { get { return m_moveTarget; } }
    public float RemainingDistance { get { return Vector3.Distance(m_moveTarget, transform.position); } }

    public virtual void Awake()
    {
        agent = gameObject.GetComponent<Agent>();
    }

    public virtual void Update()
    {
        agent.SetSteering(GetSteering(), weight);

        //if (Vector3.Distance(transform.position, m_moveTarget) < arriveDistance)
        //{
        //    OnArrive();
        //}
    }

    public float MapToRange(float rotation)
    {
        rotation %= 360.0f;

        if (Mathf.Abs(rotation) > 180.0f)
        {
            if (rotation < 0.0f)
            {
                rotation += 360.0f;
            }
            else
            {
                rotation -= 360.0f;
            }
        }

        return rotation;
    }

    //public virtual void OnArrive()
    //{

    //}

    public virtual Steering GetSteering()
    {
        return new Steering();
    }

    public void SetDestination(Vector3 position, float checkRadius = 0.5f)
    {
        //Collider[] hits = Physics.OverlapSphere(position, checkRadius, 1 << LayerMask.NameToLayer("Unit"));
        
        //if(hits.Length > 0)
        //{
        //    position += new Vector3(Random.insideUnitCircle.x * hits.Length * 0.5f, 0f, Random.insideUnitCircle.y * hits.Length * 0.5f);
        //}

        position.y = transform.position.y;
        m_moveTarget = position;
    }

    public void Stop()
    {
        m_moveTarget = transform.position;
        agent.SetSteering(new Steering(), 1f);
    }
}