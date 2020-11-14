using UnityEngine;

public class DoubleShoot : PowerUP {
    
    public override void StartPowerUP(PlayerController pc){
        pc.instantShoot = true;
    }

    public override void FinishPowerUP(PlayerController pc){
        pc.instantShoot = false;
    }
}