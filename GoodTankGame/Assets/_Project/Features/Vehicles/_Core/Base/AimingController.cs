using UnityEngine;
using UnityEngine.Rendering;

public class AimingController : MonoBehaviour
{

    [SerializeField] private Transform turret;
    [SerializeField] private Transform mantlet;

    [SerializeField] private float turretMaxRotationSpeed = 20f;
    [SerializeField] private float gunMaxElevationSpeed = 5f;

    [SerializeField] private bool verticalStabilizerEnabled = false;
    [SerializeField] private float verticalStabilizerStrength = 0f;

    [SerializeField] private float minElevation = -10f;
    [SerializeField] private float maxElevation = 25f;

    private Vector2 aimingVector;
    private float targetMantletRotation;
    private float currentLocalPitch;
    private float stabVelocity;

    [SerializeField] private float aimThreshold = 0.5f; 

    public float TurretYaw => turret.eulerAngles.y;
    public float TargetMantletPitch => targetMantletRotation;

    public bool IsAimed {  get; private set; }
    private TankStatus status;

    private void Awake()
    {
        status = GetComponent<TankStatus>();
        if (status == null) Debug.LogWarning($"Tank Status Component not found on: {gameObject.name}");
    }

    private void OnEnable()
    {
        status.OnModuleHpChanged += CheckDamage;
    }
    private void OnDisable()
    {
        status.OnModuleHpChanged -= CheckDamage;
    }

    private void Start()
    {
        float p = mantlet.eulerAngles.x;
        targetMantletRotation = p > 180f ? p - 360f : p;

        currentLocalPitch = mantlet.localEulerAngles.x;
        if (currentLocalPitch > 180f)
            currentLocalPitch -= 360f;
    }

    public void AimFromSight(
        Vector2 input,
        float sensitivity,
        float scopeFactor,
        bool canAim)
    {
        float gunElevationInput = 0f;

        if (canAim)
        {
            aimingVector.x += input.x * sensitivity * scopeFactor;
            aimingVector.y -= input.y * sensitivity * scopeFactor;

            aimingVector.x = Mathf.Clamp(
                aimingVector.x,
                -turretMaxRotationSpeed,
                turretMaxRotationSpeed);

            aimingVector.y = Mathf.Clamp(
                aimingVector.y,
                -gunMaxElevationSpeed,
                gunMaxElevationSpeed);

            float turretRotationSpeed = aimingVector.x * Time.deltaTime;
            gunElevationInput = aimingVector.y * Time.deltaTime;

            turret.rotation *= Quaternion.Euler(0f, turretRotationSpeed, 0f);
        }

        RotateCannon(gunElevationInput);
    }

    /// <summary>
    /// Naprowadza wieżę i lufę na punkt w worldSpace
    /// Wołane przez AI zamiast AimFromSight
    /// Ustawia IsAimed = true gdy cel jest w toleracji
    /// </summary>
    public void AimAtPoint(Vector3 worldPos)
    {
        Vector3 dirToTarget = worldPos - turret.position;
        Vector3 dirFlat = Vector3.ProjectOnPlane(dirToTarget, Vector3.up).normalized;

        float yawError = Vector3.SignedAngle(turret.forward, dirFlat, Vector3.up);
        float yawStep = Mathf.Min(Mathf.Abs(yawError), turretMaxRotationSpeed * Time.deltaTime);

        turret.rotation *= Quaternion.Euler(0f, Mathf.Sign(yawError) * yawStep, 0f);



        // LUFA
        float horizontalDist = dirFlat.magnitude > 0.01f
           ? Vector3.Distance(
               new Vector3(turret.position.x, 0f, turret.position.z),
               new Vector3(worldPos.x, 0f, worldPos.z))
           : 0.01f;

        float heightDiff = worldPos.y - mantlet.position.y;
        float requiredPitch = -Mathf.Atan2(heightDiff, horizontalDist) * Mathf.Rad2Deg;

        requiredPitch = Mathf.Clamp(requiredPitch, minElevation, maxElevation);

        float currentPitch = mantlet.eulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        float pitchError = Mathf.DeltaAngle(currentPitch, requiredPitch);

        float pitchStep = Mathf.Min(
            Mathf.Abs(pitchError),
            gunMaxElevationSpeed * Time.deltaTime);

        float gunElevationInput = Mathf.Sign(pitchError) * pitchStep;

        RotateCannon(gunElevationInput);

        IsAimed = Mathf.Abs(yawError) < aimThreshold
               && Mathf.Abs(pitchError) < aimThreshold;
    }

    public void ScanSector(float sectorAngle = 45f, float speed = 5f)
    {
        float t = Mathf.PingPong(Time.time * speed / sectorAngle, 1f);
        float yaw = Mathf.Lerp(-sectorAngle * 0.5f, sectorAngle * 0.5f, t);

        // Obrót lokalny
        turret.localRotation = Quaternion.Euler(0f, yaw, 0f);
        IsAimed = false;
    }

    public void ResetAimInput()
    {
        aimingVector = Vector2.zero;
    }

    private void RotateCannon(float gunElevationInput)
    {
        if (verticalStabilizerEnabled)
        {
            targetMantletRotation += gunElevationInput;

            float currentWorldPitch = mantlet.eulerAngles.x;
            if (currentWorldPitch > 180f)
                currentWorldPitch -= 360f;

            float error = Mathf.DeltaAngle(currentWorldPitch, targetMantletRotation);
            float maxCorrection = verticalStabilizerStrength * Time.deltaTime;

            float localPitch = mantlet.localEulerAngles.x;
            if (localPitch > 180f)
                localPitch -= 360f;

            float targetLocalPitch = localPitch + Mathf.Clamp(error, -maxCorrection, maxCorrection);

            currentLocalPitch = Mathf.SmoothDamp(
                currentLocalPitch,
                targetLocalPitch,
                ref stabVelocity,
                0.001f);

            mantlet.localRotation = Quaternion.Euler(currentLocalPitch, 0f, 0f);
        }
        else
        {
            mantlet.rotation *= Quaternion.Euler(gunElevationInput, 0f, 0f);

            float p = mantlet.eulerAngles.x;
            targetMantletRotation = p > 180f ? p - 360f : p;

            currentLocalPitch = mantlet.localEulerAngles.x;
            if (currentLocalPitch > 180f)
                currentLocalPitch -= 360f;
        }
    }

    private void CheckDamage(Module module)
    {

    }
}
