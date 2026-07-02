using UnityEngine;

public class TankAIController : MonoBehaviour
{
    public TankNavigation Navigation {  get; private set; }
    public TankAIPerception Perception { get; private set; }
    public AimingController AimingController { get; private set; }
    public WeaponsController WeaponsController { get; private set; }

    private TankAIState currentState;
    private void Awake()
    {
        Navigation = GetComponent<TankNavigation>();
        Perception = GetComponent<TankAIPerception>();
        AimingController = GetComponent<AimingController>();
        WeaponsController = GetComponent<WeaponsController>();
    }

    // Przejściowo start od walki
    // TODO Różne scenariusze 
    private void Start() => SwitchState(new EnganeState(this));

    private void Update() => currentState?.Update();

    public void SwitchState(TankAIState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

}
