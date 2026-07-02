using Dreamteck.Splines.Primitives;
using System.Collections.Generic;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;

public class SuspensionSystem : MonoBehaviour
{
    //kola w modelu są w pozycji ściśniętej
    public Transform[] wheels;

    public float springStrength = 20000f;
    public float springDamping = 1500f;
    public float maxSuspensionTravel = 0.5f;
    public float wheelDiameter = 0.5f;
    public bool isWheeled = false;
    public float aeroFrontalCoeff = 0.1f;
    public float aeroLateralCoeff = 0.2f;

    private List<Vector3> suspensionPoints;
    private Rigidbody rb;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupPoints();
    }

    private void FixedUpdate()
    {
        CalculateWheelsForces();
        ApplyAerodynamicDrag();
    }

    private void SetupPoints()
    {
        suspensionPoints = new List<Vector3>();
        foreach (Transform point in wheels) 
        {
            Vector3 localPos = transform.InverseTransformPoint(point.position);
            suspensionPoints.Add(localPos - new Vector3(0, wheelDiameter, 0));
        }
    }

    private void CalculateWheelsForces()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Vector3 worldPoint = transform.TransformPoint(suspensionPoints[i]);

            if (Physics.Raycast(worldPoint, -transform.up, out RaycastHit hit, maxSuspensionTravel))
            {
                float travelDistance = maxSuspensionTravel - hit.distance;
                float springCompressionRatio = travelDistance / maxSuspensionTravel;
                float springForce = springCompressionRatio * springStrength;

                Vector3 pointVelocity = rb.GetPointVelocity(worldPoint);
                //pointVelocity = Vector3.ProjectOnPlane(pointVelocity, hit.normal); // odfiltrowanie ruchów pionowych - pojazd skacze

                float verticalVelocity = Vector3.Dot(pointVelocity, transform.up);
                float dampingForce = -verticalVelocity * springDamping;

                float totalForce = springForce + dampingForce;
                totalForce = Mathf.Max(0, totalForce);
                rb.AddForceAtPosition(transform.up * totalForce, worldPoint);

                //ruch w osi y
                wheels[i].position = hit.point + (transform.up * wheelDiameter);



                float surfaceMu = 1.0f;
                float surfaceRolling = isWheeled ? 0.01f : 0.04f;

                //if (hit.collider.TryGetComponent<SurfaceProperties>(out var surface))
                //{
                //    surfaceMu = surface.frictionCoefficient;
                //    surfaceRolling = surface.rollingResistance;
                //}

                Vector3 longDir = Vector3.ProjectOnPlane(wheels[i].transform.forward, hit.normal).normalized;
                Vector3 rightDir = Vector3.ProjectOnPlane(wheels[i].transform.right, hit.normal).normalized;




                CalculateAndApplyWheelFriction(
                    hit.point, pointVelocity, totalForce,
                    longDir, rightDir,
                    surfaceMu, surfaceRolling);

                Debug.DrawLine(worldPoint, hit.point, Color.green);
            }
            else
            {
                wheels[i].position = worldPoint - (transform.up * maxSuspensionTravel) + (transform.up * wheelDiameter);

                Debug.DrawLine(worldPoint, worldPoint - (transform.up * maxSuspensionTravel), Color.red);
            }
        }
    }

    private void CalculateAndApplyWheelFriction(
        Vector3 contactPoint,
        Vector3 pointVelocity,
        float normalForce,
        Vector3 longDir,
        Vector3 rightDir,
        float mu,           // współczynnik tarcia nawierzchni (0.3 błoto → 1.1 asfalt)
        float rollingCoeff) // opór toczenia nawierzchni     (0.01 asfalt → 0.15 błoto)
    {
        if (normalForce <= 0f) return;

        float longVel = Vector3.Dot(pointVelocity, longDir);
        float latVel = Vector3.Dot(pointVelocity, rightDir);

        float deadZone = 0.05f; // 0,18 km/h
        if (Mathf.Abs(longVel) < deadZone) longVel = 0f;
        if (Mathf.Abs(latVel) < deadZone) latVel = 0f;
        if (longVel == 0f && latVel == 0f) return;



        float maxForce = mu * normalForce;

        float latGain, latBudget, longGain, longBudget;
        if (isWheeled)
        {
            latGain = 15000f;
            latBudget = 0.90f;
            longGain = 10f;
            longBudget = 0.80f;
        }
        else
        {
            latGain = 10000f;
            latBudget = 0.65f;
            longGain = 50f;
            longBudget = 0.85f;
        }

        // Tarcie boczne
        float latForce = Mathf.Clamp(
            -latVel * latGain,
            -maxForce * latBudget,
             maxForce * latBudget);

        // Rolling resistance z płynnym przejściem przy zerze
        float fadeSign = Mathf.Abs(longVel) > 0.05f
            ? Mathf.Sign(longVel)
            : longVel / 0.05f;
        float rollingForce = -normalForce * rollingCoeff * fadeSign;

        // Viscous drag (liniowy, tylko tarcie wewnętrzne — aero osobno)
        float viscousForce = -longVel * longGain;

        float longForce = Mathf.Clamp(
            viscousForce + rollingForce,
            -maxForce * longBudget,
             maxForce * longBudget);

        // Friction Ellipse
        float combined = Mathf.Sqrt(longForce * longForce + latForce * latForce);
        if (combined > maxForce)
        {
            float scale = maxForce / combined;
            longForce *= scale;
            latForce *= scale;
        }

        rb.AddForceAtPosition(
            longDir * longForce +
            rightDir * latForce,
            contactPoint,
            ForceMode.Force);

        //Debug.Log((longDir * longForce + rightDir * latForce).magnitude);
        //Debug.DrawRay(contactPoint, (longDir * longForce + rightDir * latForce).normalized, Color.magenta);
    }

    private void ApplyAerodynamicDrag()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float forwardVel = localVel.z;
        float lateralVel = localVel.x;

        float deadZone = 0.05f; // 0,18 km/h
        if (Mathf.Abs(forwardVel) < deadZone) forwardVel = 0f;
        if (Mathf.Abs(lateralVel) < deadZone) lateralVel = 0f;
        if (forwardVel == 0f && lateralVel == 0f) return;

        // Osobny współczynnik dla oporu czołowego i bocznego
        float dragForward = aeroFrontalCoeff * forwardVel * Mathf.Abs(forwardVel);
        float dragLateral = aeroLateralCoeff * lateralVel * Mathf.Abs(localVel.x);

        rb.AddRelativeForce(new Vector3(-dragLateral, 0f, -dragForward), ForceMode.Force);
    }

    //public class SurfaceProperties : MonoBehaviour
    //{
    //    [Range(0.1f, 1.5f)]
    //    public float frictionCoefficient = 1.0f;
    //    // Orientacyjnie:
    //    // Suchy beton:  1.0 – 1.1
    //    // Twarda ziemia: 0.7 – 0.8
    //    // Trawa/piasek:  0.5 – 0.6
    //    // Błoto:         0.3 – 0.4
    //    // Lód:           0.1 – 0.15

    //    [Range(0.005f, 0.20f)]
    //    public float rollingResistance = 0.02f;
    //    // Orientacyjnie (gąsienice):
    //    // Ubita droga:  0.03 – 0.05
    //    // Trawa:        0.06 – 0.08
    //    // Piasek/błoto: 0.10 – 0.15
    //}
}
