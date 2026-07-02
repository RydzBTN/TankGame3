using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class VehicleCameraController : MonoBehaviour
{

    public enum CameraState
    {
        ThirdPerson,
        FirstPerson
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform sightPoint;
    [SerializeField] private UIDocument sightHud;
    [SerializeField] private AimingController aimingController;

    [Header("Third Person")]
    [SerializeField] private Vector3 centerPoint = new Vector3(0f, 2f, 0f);
    [SerializeField] private float distanceFromCenter = 7f;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float scopeAimingFactor = 0.2f;

    [Header("Zoom")]
    [SerializeField] private float[] lensZooms = { 50f, 12.32f };

    private CameraState cameraState = CameraState.ThirdPerson;

    private float pitch;
    private float yaw;
    private int currentZoom;
    private float scopeFactorByZoom;

    public bool IsFirstPerson => cameraState == CameraState.FirstPerson;
    public float MouseSensitivity => mouseSensitivity;
    public float ScopeFactorByZoom => scopeFactorByZoom;

    private void Awake()
    {
        scopeFactorByZoom = scopeAimingFactor;

        if (sightHud != null)
            sightHud.enabled = false;
    }

    public void ToggleCameraMode()
    {
        cameraState = cameraState == CameraState.ThirdPerson
            ? CameraState.FirstPerson
            : CameraState.ThirdPerson;

        if (sightHud != null)
            sightHud.enabled = cameraState == CameraState.FirstPerson;
    }

    public void UpdateCamera(Vector2 mouseInput)
    {
        if (cameraState == CameraState.ThirdPerson)
        {
            UpdateThirdPersonCamera(mouseInput);
        }
        else
        {
            UpdateSightCamera();
        }
    }

    private void UpdateThirdPersonCamera(Vector2 mouseInput)
    {
        yaw += mouseInput.x * mouseSensitivity;
        pitch -= mouseInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 worldCenter = transform.TransformPoint(centerPoint);
        Vector3 position = worldCenter - rotation * Vector3.forward * distanceFromCenter;

        playerCamera.transform.position = position;
        playerCamera.transform.LookAt(worldCenter);
    }

    private void UpdateSightCamera()
    {
        Vector3 pos = sightPoint.position;

        Quaternion rot = Quaternion.Euler(
            aimingController.TargetMantletPitch,
            aimingController.TurretYaw,
            0f);

        playerCamera.transform.SetPositionAndRotation(pos, rot);
    }
    public void Zoom(int direction)
    {
        if (direction == 0f)
            return;

        currentZoom += direction > 0f ? 1 : -1;
        currentZoom = Mathf.Clamp(currentZoom, 0, lensZooms.Length - 1);

        playerCamera.fieldOfView = lensZooms[currentZoom];
        scopeFactorByZoom = scopeAimingFactor / (currentZoom + 1f);
    }
}
