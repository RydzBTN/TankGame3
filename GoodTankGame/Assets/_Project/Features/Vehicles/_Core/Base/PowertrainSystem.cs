using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowertrainSystem : MonoBehaviour
{
    public enum SteeringType
    {
        NeutralSteering, // (Pivot Turn) Obie gąsienice kręcą się w przeciwnych kierunkach
        ClutchBraking    // (Styl 2WŚ) Jedna gąsienica jedzie, druga jest odłączana i hamowana
    }
    public enum Gearbox
    {
        Manual,
        Automatic
    }

    [System.Serializable]
    private class wheelAnimData
    {
        public Transform transform;
        public float diameter;
    }

    [Header("REFERENCJE")]
    [SerializeField] private Transform leftTrackPoint;
    [SerializeField] private Transform rightTrackPoint;
    [SerializeField] private TrackAnimator leftTrack;
    [SerializeField] private TrackAnimator rightTrack;

    [Space(15)]
    [Header("STEERING SYSTEM")]
    [Tooltip("Neutral: Obrót w miejscu. ClutchBraking: Skręt jak w T-34, Panzer IV itp.")]
    [SerializeField] private SteeringType steeringMode = SteeringType.NeutralSteering;

    [Space(15)]
    [Header("ENGINE")]
    [SerializeField] private AnimationCurve engineTorqueCurve;
    [SerializeField] private float currentRPM;
    [SerializeField] private float idleRPM = 800f;
    [SerializeField] private float maxRPM = 3000f;
    [Tooltip("speed at wich engine revs up")]
    [SerializeField] private float engineInertia = 5f; // bezwładność
    [SerializeField] private float engineBrakingPower = 100f;
    [SerializeField] private float brakePower = 50000f;
    [SerializeField] private float engineLoad;

    [Header("Fuel")]
    [SerializeField] private float maxFuelCapacity = 500f; //l
    [SerializeField] private float currentFuel;
    [Tooltip("Spalanie w litrach na sekundę przy max RPM i max wciśnięciu gazu")]
    [SerializeField] private float maxFuelConsumption;
    [Tooltip("Spalanie na biegu jałowym (gdy czołg stoi)")]
    [SerializeField] private float idleFuelConsumption = 0.05f;


    [Space(15)]
    [Header("GEARBOX")]
    [SerializeField] private Gearbox gearboxType;
    [Tooltip(" Minimalne przyspieszenie (m/s²) jakie musi zapewnić bieg startowy (ruszanie z 2 zamiast 1 gdy warunki pozwalają)")]
    [SerializeField] private float   minStartingAcceleration = 2f;
    [SerializeField] private float   gearChangeTime = 0f;
    [SerializeField] private float   gearChangeCooldown = 0.2f;
    [SerializeField] private float[] forwardGears = { 4.5f, 3.0f, 2.1f };
    [SerializeField] private float[] reverseGears = { -4.0f };
    [SerializeField] private float   finalDriveRatio = 3.5f;
    [SerializeField] private float   sprockerRadius = 0.4f;
    private float lastGearChangeTime = -999f;
    [Header("automatic transmission")]
    [Tooltip("Docelowe, optymalne obroty silnika podczas jazdy (np. maksymalny moment obrotowy)")]
    [SerializeField] private float targetRPM = 2200f;

    [Space(15)]
    [Header("Stats")]
    [SerializeField] private bool isShifting = false;
    [SerializeField] private float forwardSpeed;
    [SerializeField, Range(0f, 1f)] private float throttle;
    [SerializeField, Range(0f, 1f)] private float brake;
    [SerializeField] private int currentGearIndex = 0; // 0 = 1, -1 = -1
    [SerializeField] private float leftTrackTorqueNm;
    [SerializeField] private float rightTrackTorqueNm;

    private bool isMoving;
    private bool isMovingForward;
    private bool isMovingBackward;
    private bool isInputForward;
    private bool isInputBackward;
    public bool isEngineRunning = true;

    [Space(15)]
    [Header("WHEELS AND TRACK ANIMATION")]
    [SerializeField] private List<wheelAnimData> leftWheelsToAnim = new List<wheelAnimData>();
    [SerializeField] private List<wheelAnimData> rightWheelsToAnim = new List<wheelAnimData>();

    private float inputForward;
    private float inputTurn;
    public float engineDurability = 100f;
    private float gearboxDurability = 100f;
    private float leftTrackSpeed;
    private float rightTrackSpeed;

    // Niezależne wskaźniki hamowania dla gąsienic
    public float leftTrackBrakeValue;
    public float rightTrackBrakeValue;

    // Prywatne referencje do innych komponentów
    private Rigidbody rb;
    private PlayerInput playerInput;
    private TankStatus status;
    private VehicleAudioController vehicleAudio;


    private void Awake()
    {
        if (!TryGetComponent<Rigidbody>(out rb))
            Debug.LogError($"Component TankPowertrain on {gameObject.name} cant access Rigidbody");

        if (!TryGetComponent<TankStatus>(out status))
            Debug.LogError($"Component TankPowertrain on {gameObject.name} cant access TankStatus");

        if(!TryGetComponent<VehicleAudioController>(out vehicleAudio))
            Debug.LogError($"Component TankPowertrain on {gameObject.name} cant access AudioController");
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
        playerInput = InputManager.Instance.playerInput;
    }

    public void Drive(
        Vector2 playerMoveInput,
        bool upshift,
        bool downshift)
    {
        inputForward = playerMoveInput.y;
        inputTurn = playerMoveInput.x;

        forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        isMoving = Mathf.Abs(forwardSpeed) > 0.05f;
        isMovingForward = forwardSpeed > 0.05f;
        isMovingBackward = forwardSpeed < -0.05f;

        isInputForward = inputForward > 0.1f;
        isInputBackward = inputForward < -0.1f;

        vehicleAudio.ChangeEngineAudio((currentRPM - idleRPM) / (maxRPM - idleRPM), engineLoad);


        if (!isMoving && Mathf.Abs(inputForward) < 0.05f && Mathf.Abs(inputTurn) < 0.05f)
        {
            rb.linearDamping = 10f;
        }
        else
        {
            rb.linearDamping = 0f;
        }


        if (gearboxType == Gearbox.Manual)
        {
            HandleManualSteering(upshift, downshift);
        }
        else if (gearboxType == Gearbox.Automatic)
        {
            HandleSteering();
            HandleAutomaticTransmission();
        }
    }

    private void FixedUpdate()
    {
        // Obliczanie prędkości gąsienic
        leftTrackSpeed = Vector3.Dot(rb.GetPointVelocity(leftTrackPoint.position), leftTrackPoint.forward);
        rightTrackSpeed = Vector3.Dot(rb.GetPointVelocity(rightTrackPoint.position), rightTrackPoint.forward);

        UpdateEngine(Time.fixedDeltaTime, throttle);
        engineLoad = CalculateEngineLoad();
        float rawTorque = GetCurrentTransmissonOutput();
        CalculateTrackTorque(rawTorque, throttle);

        ApplyForcesToTracks();

        AnimateWheels();
    }

    private void CheckDamage(Module module)
    {
        if (module.type == Module.ModuleType.Engine) engineDurability = module.hp;
        if (module.type == Module.ModuleType.Transmission) gearboxDurability = module.hp;
    }

    #region Engine and Gearbox Simulation

    private float GetEngineDamageValue() => Mathf.Clamp((engineDurability / 100f), 0.1f, 1f);
    private bool DrawGearChange() => UnityEngine.Random.value <= (gearboxDurability / 100);
    /// <summary>
    /// Zarządza sterowaniem silnika na podstawie inputow oraz stanu pojazdu
    /// </summary>
    private void HandleSteering()
    {
        forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        isMoving = Mathf.Abs(forwardSpeed) > 0.05f;
        isMovingForward = forwardSpeed > 0.05f;
        isMovingBackward = forwardSpeed < -0.05f;

        isInputForward = inputForward > 0.1f;
        isInputBackward = inputForward < -0.1f;

        rb.angularDamping = 0f;
        rb.linearDamping = 0f;

        // Brak inputu gazu
        if (!isInputForward && !isInputBackward)
        {
            // Skręcamy w miejscu (Pivot turn) – dajemy gaz silnikowi
            if (Mathf.Abs(inputTurn) > 0.1f)
            {
                throttle = 1f;
                brake = 0f;
            }
            else
            {
                throttle = 0f;
                brake = 0f;

                if(Mathf.Abs(forwardSpeed) < 0.5f)
                {
                    rb.angularDamping = 10f;
                    rb.linearDamping = 10f;
                }
            }
            return;
        }

        if (isMovingForward && isInputBackward)
        {
            throttle = 0f;
            brake = Mathf.Abs(inputForward);
            return;
        }

        if (isMovingBackward && isInputForward)
        {
            throttle = 0f;
            brake = Mathf.Abs(inputForward);
            return;
        }

        throttle = 1f;
        brake = 0f;
    }
    public float CalculateEngineLoad()
    {
        if (!isEngineRunning) return 0f;
        if (throttle < 0.01f) return 0.05f;

        float rpmPercentage = (currentRPM - idleRPM) / (maxRPM - idleRPM);
        rpmPercentage = Mathf.Clamp01(rpmPercentage);

        float loadFromRPM = Mathf.Lerp(1.0f, 0.4f, rpmPercentage);
        float finalLoad = throttle * loadFromRPM;

        if (Mathf.Abs(inputTurn) > 0.1f)
        {
            finalLoad = Mathf.Max(finalLoad, throttle * 0.85f);
        }
        
        return Mathf.Clamp01(finalLoad);
    }
    private void HandleAutomaticTransmission()
    {
        if (isShifting) return;
        if (Time.time - lastGearChangeTime < gearChangeCooldown) return;

        // RUSZANIE Z MIEJSCA
        if (isInputForward && currentGearIndex < 0 && forwardSpeed > -1.0f)
        {
            StartCoroutine(ChangeGear(GetDynamicStartingGear()));
            return;
        }
        else if (isInputBackward && currentGearIndex >= 0 && forwardSpeed < 1.0f)
        {
            StartCoroutine(ChangeGear(-1));
            return;
        }

        if (!isMoving && !isInputBackward && !isInputForward)
        {
            int startGear = GetDynamicStartingGear();
            if (currentGearIndex != startGear)
            {
                StartCoroutine(ChangeGear(startGear));
            }
            return;
        }


        // JAZDA DO PRZODU
        if (isMovingForward && currentGearIndex >= 0)
        {
            float slopeSin = transform.forward.y;

            float upshiftRpmThreshold = Mathf.Lerp(targetRPM, maxRPM, engineLoad);
            float downshiftRpmThreshold = Mathf.Lerp(idleRPM * 1.5f, targetRPM * 0.9f, engineLoad);

            if (slopeSin > 0.05f)
            {
                float uphillFactor = Mathf.Clamp01(slopeSin * 3f);

                downshiftRpmThreshold += (targetRPM - downshiftRpmThreshold) * uphillFactor;
                upshiftRpmThreshold += (maxRPM - upshiftRpmThreshold) * uphillFactor;
            } 
            else if (slopeSin < -0.05f)
            {
                upshiftRpmThreshold += (slopeSin * targetRPM * 0.5f);
            }

            upshiftRpmThreshold = Mathf.Clamp(upshiftRpmThreshold, targetRPM * 0.8f, maxRPM);
            downshiftRpmThreshold = Mathf.Clamp(downshiftRpmThreshold, idleRPM * 1.2f, maxRPM * 0.8f);
            
            if (currentRPM > upshiftRpmThreshold)
            {
                int nextGear = currentGearIndex + 1;
                if (nextGear < forwardGears.Length)
                {
                    StartCoroutine(ChangeGear(nextGear));
                    return;
                }
            }

            if (currentRPM < downshiftRpmThreshold)
            {
                int minAllowedGear = (forwardSpeed > 2f) ? 1 : GetDynamicStartingGear();
                int nextGear = currentGearIndex - 1;

                
                if (nextGear >= minAllowedGear)
                {
                    StartCoroutine(ChangeGear(nextGear));
                    return;
                }
            }
        }

    }
    private void HandleManualSteering(bool upshift, bool downshift)
    {
        brake = 0f;
        throttle = 0f;

        if (inputForward > 0) throttle = Math.Abs(inputForward);
        if (inputForward < 0) brake = Math.Abs(inputForward);

        if (Mathf.Abs(inputTurn) > 0.1f)
        {
            throttle = 1f;
            brake = 0f;
        }


        if (downshift)
        {
            int nextGear = currentGearIndex - 1;
            if(nextGear < 0)
            {
                if( Mathf.Abs(nextGear) <= reverseGears.Length) StartCoroutine(ChangeGear(nextGear));
            }
            else StartCoroutine(ChangeGear(nextGear));

        } 
        else if (upshift)
        {
            int nextGear = currentGearIndex + 1;
            if(nextGear < forwardGears.Length) StartCoroutine(ChangeGear(nextGear));

        }
    }

    /// <summary>
    /// Oblicza optymalny bieg startowy. Skanuje biegi od najwyższego do najniższego
    /// i wybiera najwyższy, który zapewnia wystarczającą siłę napędową do ruszenia z miejsca.
    /// Efekt: "ruszanie z dwójki" – gdy czołg ma odpowiedni moment przy targetRPM na wyższym biegu,
    /// nie ma sensu zaczynać od jedynki (większe zużycie skrzyni, szybsze wyjście z pasma mocy).
    /// </summary>
    private int GetDynamicStartingGear()
    {
        float slopeSin = transform.forward.y;
        float gravityOpposingForce = rb.mass * Mathf.Abs(Physics.gravity.y) * slopeSin;

        float rollingResistance = rb.mass * 0.5f;
        float totalResistance = gravityOpposingForce + rollingResistance;
        float requiredNetForce = rb.mass * 1.2f;

        int maxStartingGearIndex = Mathf.Min(1, forwardGears.Length - 1);
        for (int i = maxStartingGearIndex; i >= 0; i--)
        {
            float maxOutputTorque = GetTransmissonOutput(i);
            float trackForce = (maxOutputTorque / sprockerRadius);
            float netForce = trackForce - totalResistance;

            if (netForce >= requiredNetForce)
            {
                if (forwardSpeed < 2f && Mathf.Abs(inputTurn) > 0.05f) return 0;
                return i;
            }
        }
        
        return 0;
    }
    private IEnumerator ChangeGear(int gear)
    {
        isShifting = true;
        status.NotifyGearChange(99);
        leftTrackTorqueNm = 0f;
        rightTrackTorqueNm = 0f;

        yield return new WaitForSeconds(gearChangeTime);

        // szansa na zmiane biegu maleje wraz z uszkodzeniem transmisji
        if(DrawGearChange()) currentGearIndex = gear;

        status.NotifyGearChange(currentGearIndex >= 0 ? currentGearIndex + 1 : currentGearIndex);
        isShifting = false;
        lastGearChangeTime = Time.time;
    }


    /// <summary>
    /// Oblicza curent RPM na podstawie prędkości kół i zużywa paliwo
    /// </summary>
    private void UpdateEngine(float deltaTime, float throttle)
    {
        if(currentFuel <= 0f)
        { 
            isEngineRunning = false;
            currentFuel = 0f;
        }
        if (!isEngineRunning)
        {
            currentRPM = Mathf.Lerp(currentRPM, 0f, deltaTime * engineInertia);
            leftTrackTorqueNm = 0f;
            rightTrackTorqueNm = 0f;
            return;
        }

        float currentFuelConsumprion = idleFuelConsumption + (maxFuelConsumption * engineLoad);
        currentFuel -= currentFuelConsumprion * deltaTime;
        status.NotifyFuelChange(currentFuel, (currentFuel/maxFuelCapacity) * 100f);

        // Symulacja sprzęgła/zmiennika momentu.
        // Silnik wchodzi na obroty od wciśnięcia gazu, ale jest też ciągnięty przez prędkość pojazdu.
        float wheelRPM = CalculateAbsAverageTracksRPM() * Mathf.Abs(GetCurrentGearRatio()) * finalDriveRatio;

        currentRPM = Mathf.Lerp(currentRPM, idleRPM + wheelRPM, 1f / engineInertia);
        currentRPM = Mathf.Clamp(currentRPM, idleRPM, maxRPM);

        status.NotifyRPMChange(currentRPM, (currentRPM/maxRPM) * 100f);
    }
    private float CalculateEngineBrakingForce()
    {
        // Jeśli zmieniamy bieg (sprzęgło wciśnięte) albo trzymamy gaz - silnik nie hamuje
        if (isShifting || throttle > 0.05f || !isEngineRunning) return 0f;

        // Hamowanie silnikiem jest silniejsze na wysokich obrotach
        float rpmFactor = currentRPM / maxRPM;

        // Obliczamy surowy moment oporu z silnika (Nm)
        float rawResistanceTorque = engineBrakingPower * rpmFactor;

        // Przemnażamy opór przez skrzynię biegów (na 1. biegu jest ogromny mnożnik!)
        float torqueAtSprocket = rawResistanceTorque * Mathf.Abs(GetCurrentGearRatio()) * finalDriveRatio;

        // Zmieniamy moment obrotowy (Nm) na fizyczną siłę pchającą (N), dzieląc przez promień koła
        float brakingForceInNewtons = torqueAtSprocket / sprockerRadius;

        return brakingForceInNewtons;
    }


    /// <summary>
    /// Takes engine rpm and calculates torque output out of transmission
    /// </summary>
    /// <returns>Powen in Nm</returns>
    private float GetCurrentTransmissonOutput()
    {
        if (isShifting) return 0f;

        float rawEngineTorque = engineTorqueCurve.Evaluate(currentRPM);
        return rawEngineTorque * GetCurrentGearRatio() * finalDriveRatio * GetEngineDamageValue();
    }
    private float GetTransmissonOutput(int gear)
    {
        if (isShifting) return 0f;

        float rawEngineTorque = engineTorqueCurve.Evaluate(currentRPM);
        return rawEngineTorque * GetGearRatio(gear) * finalDriveRatio * GetEngineDamageValue();
    }

     
    /// <summary>
    /// Takes power and applies on left and right track, 
    /// </summary>
    /// <param name="torque">Torque from transmission in Nm</param>
    private void CalculateTrackTorque(float transmissionRawTorque, float throttle)
    {
        // Sztucznie zmieniona wartość mocy na podstawie wciśnięcia gazu
        float activeTorque = (transmissionRawTorque * throttle);

        if (currentRPM > maxRPM || (isMovingForward && !isInputForward) && Mathf.Abs(inputTurn) < 0.05f) activeTorque = -CalculateEngineBrakingForce();

        leftTrackTorqueNm = activeTorque * 0.5f;
        rightTrackTorqueNm = activeTorque * 0.5f;

        leftTrackBrakeValue = brake;
        rightTrackBrakeValue = brake;

        // Jeśli wciskamy A lub D
        if (Mathf.Abs(inputTurn) > 0.05f)
        {
            switch (steeringMode)
            {
                case SteeringType.NeutralSteering:
                    // Brak wciśniętego gazu = obrot w miejscu,
                    // bierzemy moc nie pomnożoną przez wcisnięcie gazu
                    if (Mathf.Abs(inputForward) < 0.05f)
                    {
                        // [ZMIANA] Skręt w miejscu (Pivot). transmissionRawTorque teraz rośnie, bo w HandleSteering dodajemy 'throttle' przy skręcie!
                        // Zapewniamy, że skręt w miejscu wymusza najniższe przełożenie (jedynka)
                        float pivotTorque = engineTorqueCurve.Evaluate(currentRPM) * forwardGears[0] * finalDriveRatio * inputTurn;
                        leftTrackTorqueNm = pivotTorque;
                        rightTrackTorqueNm = -pivotTorque;
                    }
                    else // Skręt w ruchu
                    {
                        if (inputTurn > 0) // w prawo
                        {
                            leftTrackTorqueNm += activeTorque * inputTurn;
                            rightTrackTorqueNm -= activeTorque * inputTurn;
                        }
                        else // w lewo
                        {
                            leftTrackTorqueNm -= activeTorque * Mathf.Abs(inputTurn);
                            rightTrackTorqueNm += activeTorque * Mathf.Abs(inputTurn);
                        }
                    }
                break;

                case SteeringType.ClutchBraking:

                    // Skręt w miejscu
                    if (Mathf.Abs(inputForward) < 0.05f)
                    {
                        if (inputTurn > 0) // W prawo
                        {
                            rightTrackTorqueNm = 0f;
                            rightTrackBrakeValue = 1f;
                        }
                        else // Skręt w Lewo
                        {
                            leftTrackTorqueNm = 0; 
                            leftTrackBrakeValue = 1f;
                        }
                        return;
                    }

                    // Logika odłączania gąsienicy oraz hamowania
                    if (inputTurn > 0) // Skręt w Prawo
                    {
                        rightTrackTorqueNm *= 0.1f; // Odcinamy moment
                        rightTrackBrakeValue = 0.5f; //Mathf.Max(rightTrackBrakeValue, Mathf.Abs(inputTurn)); // Dociskamy jej hamulec
                    }
                    else // Skręt w Lewo
                    {
                        leftTrackTorqueNm *= 0.1f; //(1f - Mathf.Abs(inputTurn));
                        leftTrackBrakeValue = 0.5f; //Mathf.Max(leftTrackBrakeValue, Mathf.Abs(inputTurn));
                    }
                    break;
            }

           
        }
    }
    private void ApplyForcesToTracks()
    {
        Vector3 leftTrackForceN = leftTrackPoint.forward.normalized * (leftTrackTorqueNm / sprockerRadius);
        Vector3 rightTrackForceN = rightTrackPoint.forward.normalized * (rightTrackTorqueNm / sprockerRadius);

        rb.AddForceAtPosition(leftTrackForceN, leftTrackPoint.position);
        rb.AddForceAtPosition(rightTrackForceN, rightTrackPoint.position);

        ApplyBrakeForceToPoint(leftTrackPoint, leftTrackSpeed, leftTrackBrakeValue);
        ApplyBrakeForceToPoint(rightTrackPoint, rightTrackSpeed, rightTrackBrakeValue);
    }
    private void ApplyBrakeForceToPoint(Transform trackPoint, float trackSpeed, float trackBrakeInput)
    {
        if (trackBrakeInput < 0.01f) return;

        if (Mathf.Abs(trackSpeed) > 0.05f)
        {
            Vector3 brakeForce = -trackPoint.forward * Mathf.Sign(trackSpeed) * brakePower * trackBrakeInput;
            rb.AddForceAtPosition(brakeForce, trackPoint.position);
        }
        
    }
    private float GetCurrentGearRatio()
    {
        if (currentGearIndex == -1) return reverseGears[0];
        return forwardGears[currentGearIndex];
    }
    private float GetGearRatio(int gear)
    {
        if (gear == -1) return reverseGears[0];
        return forwardGears[gear];
    }
    private float CalculateAbsAverageTracksRPM()
    {
        // Wzór na RPM: (Prędkość [m/s] / Obwód koła) * 60

        float totalRPM = 0;
        float wheelCircumference = 2f * Mathf.PI * sprockerRadius;

        totalRPM += (leftTrackSpeed / wheelCircumference) * 60;
        totalRPM += (rightTrackSpeed / wheelCircumference) * 60;
        return  Mathf.Abs(totalRPM / 2f);
    }
    
    #endregion

    #region Wheels Animation
    private void AnimateWheels()
    {
        leftTrack.UpdateSpeed(leftTrackSpeed);
        rightTrack.UpdateSpeed(rightTrackSpeed);

        foreach (wheelAnimData wheel in leftWheelsToAnim)
        {
            float rpm = CalculateWheelRPM(wheel.diameter, leftTrackSpeed);
            float rotation = (rpm / 60f) * Time.fixedDeltaTime * 360f;
            wheel.transform.Rotate(rotation, 0, 0);
        }
        foreach (wheelAnimData wheel in rightWheelsToAnim)
        {
            float rpm = CalculateWheelRPM(wheel.diameter, rightTrackSpeed);
            float rotation = (rpm / 60f) * Time.fixedDeltaTime * 360f;
            wheel.transform.Rotate(rotation, 0, 0);
        }
    }

    public float CalculateWheelRPM(float wheelDiameterInMeters, float speedInMeterPerSecond)
    {
        // Obwód koła = π × średnica
        float circumferenceInMeters = Mathf.PI * wheelDiameterInMeters;

        // Liczba obrotów na sekundę = prędkość / obwód
        float revolutionsPerSecond = speedInMeterPerSecond / circumferenceInMeters;

        // RPM = obroty na sekundę × 60
        float rpm = revolutionsPerSecond * 60f;

        return rpm;
    }

    public void SetDurability(int durability)
    {
        
    }
    #endregion

}
