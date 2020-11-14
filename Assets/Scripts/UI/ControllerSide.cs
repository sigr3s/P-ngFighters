using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class ControllerSide : MonoBehaviour {
    public InputDevice inputDevice;
    private LocalJoiner joiner;

    public void Initialize(InputDevice id, LocalJoiner localJoiner){
        inputDevice = id;
        this.joiner = localJoiner;
    }

    private double startTime = 0;
 

    public void OnMove(CallbackContext context)
    {
        if(context.control.device.deviceId == inputDevice.deviceId ){

            HorizontalLayoutGroup hlg = GetComponentInChildren<HorizontalLayoutGroup>();
            Vector2 val = context.ReadValue<Vector2>();

            if(Mathf.Abs(val.x) < 0.5f) return;
            if( (context.startTime - startTime) < 0.25f){
                return;
            }

            switch(hlg.childAlignment){
                case TextAnchor.MiddleCenter:
                    if(val.x > 0){
                        if(joiner.TryJoin(PlayerID.Player2, inputDevice)){
                            hlg.childAlignment = TextAnchor.MiddleRight;
                            startTime = context.startTime;
                        }
                    }
                    else{
                        if(joiner.TryJoin(PlayerID.Player1, inputDevice)){
                            hlg.childAlignment =  TextAnchor.MiddleLeft;
                            startTime = context.startTime;
                        }
                    }
                break;
                case TextAnchor.MiddleLeft:
                    if(val.x > 0){
                        hlg.childAlignment =  TextAnchor.MiddleCenter;
                        startTime = context.startTime;
                        joiner.RemovePlayer(inputDevice);
                    }
                break;
                case TextAnchor.MiddleRight:
                    if(val.x < 0){
                        hlg.childAlignment =  TextAnchor.MiddleCenter;
                        startTime = context.startTime;
                        joiner.RemovePlayer(inputDevice);
                    }
                break;
            }
        }
    }
}