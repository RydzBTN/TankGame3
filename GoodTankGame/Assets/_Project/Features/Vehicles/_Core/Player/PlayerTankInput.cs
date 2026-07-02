using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTankInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private PowertrainSystem powertrain;
    private VehicleCameraController cameraController;
    private AimingController aimingController;
    private WeaponsController weaponsController;


    private void Awake()
    {
        powertrain = GetComponent<PowertrainSystem>();
        cameraController = GetComponent<VehicleCameraController>();
        aimingController = GetComponent<AimingController>();
        weaponsController = GetComponent<WeaponsController>();
    }

    private void Start()
    {
        playerInput = InputManager.Instance.playerInput;
    }

    private void LateUpdate()
    {
        Aim();
        if (playerInput.actions["FireMainGun"].IsPressed()) weaponsController.TryFireMainGun();
        if (playerInput.actions["FireMG"].IsPressed()) weaponsController.TryFireMG();
    }
    private void Update()
    {
        powertrain.Drive(
            playerInput.actions["Move"].ReadValue<Vector2>(),
            playerInput.actions["Upshift"].WasPressedThisFrame(),
            playerInput.actions["Downshift"].WasPressedThisFrame()
            );
    }

    private void Aim()
    {

        Vector2 mouseInput = playerInput.actions["Look"].ReadValue<Vector2>();

        if (playerInput.actions["ChangeAim"].WasPressedThisFrame())
            cameraController.ToggleCameraMode();

        bool canAim = playerInput.actions["EnableAiming"].IsPressed();

        aimingController.AimFromSight(
            mouseInput,
            cameraController.MouseSensitivity,
            cameraController.ScopeFactorByZoom,
            canAim);

        if (playerInput.actions["EnableAiming"].WasReleasedThisFrame())
            aimingController.ResetAimInput();

        if (playerInput.actions["Zoom"].WasPressedThisFrame())
        {
            float zoomDirection = playerInput.actions["Zoom"].ReadValue<Vector2>().y;
            cameraController.Zoom((int)zoomDirection);
        }
        cameraController.UpdateCamera(mouseInput);
    }
}
