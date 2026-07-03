using Unity.VisualScripting;
using UnityEngine;

public class TankAIPerception : MonoBehaviour
{
    [SerializeField] private float sightRange = 600f;
    [SerializeField] private float sightAngle = 90f;
    [SerializeField] private float hearingRange = 50f;
    [SerializeField] private LayerMask targetableLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private float lastPerceptionUpdate;
    private const float PERCEPTION_INTERVAL = 0.5f;
    private TankController myTank;

    public TankController CurrentTarget {  get; private set; }
    public bool CanSeeTarget { get; private set; }
    public float DistanceToTarget => CurrentTarget != null 
        ? Vector3.Distance(transform.position, CurrentTarget.transform.position) 
        : float.MaxValue;

    private void Awake()
    {
        myTank = GetComponent<TankController>();
    }

    private void Update()
    {
        if (Time.time - lastPerceptionUpdate < PERCEPTION_INTERVAL) return;
        lastPerceptionUpdate = Time.time;
        UpdatePerception();
    }

    private void UpdatePerception()
    {
        CanSeeTarget = false;

        Collider[] inRange = Physics.OverlapSphere(transform.position, sightRange, targetableLayer);
        
        TankController bestTarget = null;
        float closestAngle = sightAngle;

        foreach(Collider col in inRange)
        {
            TankController tank = col.GetComponentInParent<TankController>();
            if (tank == null || tank == myTank || tank.team == myTank.team) continue;

            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle > sightAngle) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (Physics.Raycast(transform.position + Vector3.up, dirToTarget, dist, obstacleLayer)) continue;

            if(angle < closestAngle)
            {
                closestAngle = angle;
                bestTarget = tank;
            }
        }

        CurrentTarget = bestTarget;
        CanSeeTarget = bestTarget != null;
    }

    public void ForceTarget(TankController tank) => CurrentTarget = tank;
}
