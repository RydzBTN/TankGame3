using UnityEngine;

public abstract class TankAIState
{
    protected TankAIController AI;

    protected TankAIState(TankAIController ai) {  AI = ai; }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}


public class EnganeState : TankAIState
{
    public EnganeState(TankAIController ai) : base(ai) { }

    public override void Enter()
    {
        AI.Navigation.BrakeToStop();
    }

    public override void Update()
    {
        if (AI.Perception.CurrentTarget == null) return;
        Vector3 AimPoint = AI.Perception.CurrentTarget.transform.position + Vector3.up;

        AI.AimingController.AimAtPoint(AimPoint);

        if(AI.AimingController.IsAimed) AI.WeaponsController.TryFireMainGun();
    }

    public override void Exit()
    {
        AI.AimingController.ResetAimInput();
    }

}