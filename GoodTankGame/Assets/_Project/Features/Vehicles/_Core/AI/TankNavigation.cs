using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TankNavigation : MonoBehaviour
{
    private NavMeshPath path;
    private int currentWaypoint;
    private PowertrainSystem powertrain;
    private Rigidbody rb;

    [SerializeField] private float waypointTolerance = 1f;
    [SerializeField] private bool debugMode;

    private void Awake()
    {
        path = new NavMeshPath();
        powertrain = GetComponent<PowertrainSystem>();
        rb = GetComponent<Rigidbody>();
    }

    public void SetDestination(Vector3 target)
    {
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
        currentWaypoint = 0;
    }

    public bool UpdateMovement()
    {
        if (path.corners == null || currentWaypoint >= path.corners.Length) 
        {
            BrakeToStop();
            return true;
        } 

        Vector3 targetCorner = path.corners[currentWaypoint];

        float dist = Vector3.Distance(transform.position, targetCorner);
        if(dist < waypointTolerance)
        {
            currentWaypoint++;
            if (currentWaypoint >= path.corners.Length)
            {
                BrakeToStop();
                return true;
            }
        }

        Vector3 dirToTarget = (targetCorner - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

        float turnInput = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
        float forwardInput = Mathf.Clamp01(1f - Mathf.Abs(angleToTarget) / 45f);

        if (Mathf.Abs(angleToTarget) > 30f)
        {
            forwardInput = 0f;
        }

        Vector2 moveAiInput = new Vector2(turnInput, forwardInput);

        powertrain.Drive(moveAiInput, false, false);
        return false;
    }

    public void BrakeToStop()
    {
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        if (speed > 1f)
        {
            powertrain.Drive(new Vector2(0f, -1f), false, false);
        }
        else if (speed < -1f) 
        {
            powertrain.Drive(new Vector2(0f, 1f), false, false);
        }
        else
        {
            powertrain.Drive(Vector2.zero, false, false);
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugMode || path == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLineList(path.corners);
    }
}
