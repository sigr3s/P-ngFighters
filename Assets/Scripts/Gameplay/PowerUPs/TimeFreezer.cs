using UnityEngine;

public class TimeFreezer : PowerUP {

    public override void StartPowerUP(PlayerController pc){
        Debug.Log("Freeze");
        Hazard.HazardSimulationRate = 0f;
    }

    public override void FinishPowerUP(PlayerController player){
        Hazard.HazardSimulationRate = 1f;
    }

}