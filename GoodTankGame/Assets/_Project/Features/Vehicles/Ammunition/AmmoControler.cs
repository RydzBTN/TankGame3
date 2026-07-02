using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Pool;

public class AmmoControler : MonoBehaviour
{
    public AmmoData ammoData;

    [SerializeField] private Vector3 currentVelocity;
    [SerializeField] private Vector3 currentLogicPos;
    [SerializeField] private Vector3 lastLogicPos;
    [SerializeField] private float distance;
    [Space(15)]
    [Header("EFFECTS")]
    [SerializeField] private ParticleSystem impactMetalPrefab;
    [SerializeField] private ParticleSystem impactDirtPrefab;

    private LayerMask hitboxLayer;
    private Vector3 impactPoint;
    private bool hasHit = false;

    private IObjectPool<AmmoControler> pool;

    private void Update()
    {
        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        Vector3 interpolatedPos = Vector3.Lerp(lastLogicPos, currentLogicPos, t);
        transform.position = interpolatedPos;
        transform.rotation = Quaternion.LookRotation(currentVelocity);

        Debug.DrawRay(transform.position, currentVelocity.normalized, Color.red);
    }

    private void FixedUpdate()
    {
        if (hasHit) return;
        if (distance > ammoData.destroyDistance)
        {
            ReturnToPool();
            return;
        }

        CalculateTrajectory();

        Vector3 displacement = currentVelocity * Time.fixedDeltaTime;
        Vector3 nextPos = currentLogicPos + displacement;

        // Jeśli pocisk uderzył w collider z warstwą Hitbox
        if (Physics.Raycast(currentLogicPos, displacement.normalized, out RaycastHit hit, displacement.magnitude, hitboxLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.blue);

            //Debug
            //impactPoint = hit.point;
            //transform.position = impactPoint;
            //currentVelocity = Vector3.zero;
            //hasHit = true;
            //return;

            SpawnImpactEffect(hit);
            currentLogicPos = hit.point;

            HandleHit(hit);
        }
        else
        {
            currentLogicPos = nextPos;
            Debug.DrawLine(transform.position, nextPos, Color.blue);
        }

    }

    private void OnDrawGizmos()
    {
        if (hasHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(impactPoint, 0.5f);
        }
    }

    // Wywoływane przez pule przy pobraniu obiektu
    public void Init(IObjectPool<AmmoControler> pool, Vector3 startDirection, Vector3 tankVelocity)
    {
        this.pool = pool;
        currentVelocity = startDirection.normalized * ammoData.muzzleVelocity;
        currentVelocity += tankVelocity;
        lastLogicPos = transform.position;
        currentLogicPos = transform.position;
        distance = 0f;
        hasHit = false;
        hitboxLayer = LayerMask.GetMask("ProjectileHitbox", "WorldStatic");

        CalculateTrajectory();
    }
    private void ReturnToPool()
    {
        currentVelocity = Vector3.zero;
        lastLogicPos = Vector3.zero;
        currentLogicPos = Vector3.zero;
        pool.Release(this);
    }
    private void CalculateTrajectory()
    {
        lastLogicPos = currentLogicPos;

        currentVelocity += Vector3.down * 9.81f * Time.fixedDeltaTime;

        float dragForce = 0.5f *
                            1.293f *
                            Mathf.Sqrt(currentVelocity.magnitude) *
                            ammoData.dragCoefficient *
                            ammoData.GetFrontSurfaceArea();
        currentVelocity += -currentVelocity.normalized * dragForce / ammoData.mass * Time.fixedDeltaTime;


        distance += currentVelocity.magnitude * Time.fixedDeltaTime;
    }

    private void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("armor"))
        {
            ArmorPlate hitArmor = hit.collider.GetComponent<ArmorPlate>();
            float hitAngle = 180f - Vector3.Angle(currentVelocity.normalized, hit.normal);

            if (hitAngle > 70)//rykoszet
            {
                currentVelocity = Vector3.Reflect(currentVelocity.normalized, hit.normal) * currentVelocity.magnitude;
                currentVelocity *= 0.5f;
                return;
            }

            float effectiveArmor = hitArmor.GetEffectiveThickness(hitAngle);
            float penetrationPower = ammoData.GetPenetrationPower(distance);
            Debug.Log($"Pen: ({penetrationPower}), Eff-armor ({effectiveArmor}), Angle ({hitAngle}), Distance ({distance})");


            if (effectiveArmor < penetrationPower)
            {
                CreateSpall(hit.point + currentVelocity.normalized * 0.001f, currentVelocity.normalized, 30f, 15);
                ReturnToPool();
            }
            else
            {
                ReturnToPool();
            }
        }
        else
        {
            ReturnToPool();
        }
        
    }
    private void CreateSpall(Vector3 spallPosition, Vector3 coneDirection, float coneAngle, int quantity) 
    {
        Debug.Log("creating spall");
        for(int i = 0; i < quantity; i++)
        {
            //TODO zbieranie danych do tablicy i dopiero potem wywolywanie getdamage
            Vector3 direction = GetRandomDirectionInCone(coneDirection, coneAngle);
            if (Physics.Raycast(spallPosition, direction, out RaycastHit hit, 5f))
            {
                if (hit.collider.CompareTag("module"))
                {
                    Module module = hit.collider.GetComponent<Module>();
                    module.GetDamage(UnityEngine.Random.Range(10, 90));
                    Debug.DrawLine(spallPosition, hit.point, Color.orangeRed, 5f);
                }
            }
        }
    }
    private void Explode(float tntEquivalent)
    {

    }
    private Vector3 GetRandomDirectionInCone(Vector3 coneDirection, float maxConeAngle)
    {
        // Losowy kąt w obwodzie stożka (0-360°)
        float randomAngle = UnityEngine.Random.Range(0f, 360f);

        // Losowy kąt od osi stożka (0 do maxConeAngle)
        float randomConeAngle = UnityEngine.Random.Range(0f, maxConeAngle);

        // Konwersja na wektor
        Quaternion rotation = Quaternion.AngleAxis(randomConeAngle, UnityEngine.Random.insideUnitSphere);
        Quaternion circumRotation = Quaternion.AngleAxis(randomAngle, coneDirection);

        return circumRotation * rotation * coneDirection;
    }

    #region EFFECTS
    private void SpawnImpactEffect(RaycastHit hit)
    {
        GameObject impactPrefab;
        if (hit.collider.CompareTag("armor"))
        {
            impactPrefab = impactMetalPrefab.gameObject;
        }
        else
        {
            impactPrefab = impactDirtPrefab.gameObject;
        }

        Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
    }
    #endregion

    
}
