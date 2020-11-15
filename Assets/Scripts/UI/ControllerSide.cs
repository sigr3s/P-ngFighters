using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;
using DG.Tweening;

public class ControllerSide : MonoBehaviour {
    public InputDevice inputDevice;
    private LocalJoiner joiner;
    private ControllerSideDevice currentSideDevice;

    [SerializeField] private List<ControllerSideDevice> sideDevices = new List<ControllerSideDevice>();


    public void Initialize(InputDevice id, LocalJoiner localJoiner){
        inputDevice = id;

        foreach(ControllerSideDevice csd in sideDevices){
            if(id.path.ToLower().Contains(csd.path.ToLower()) || id.description.capabilities.ToLower().Contains(csd.path.ToLower())){
                currentSideDevice = csd;
                currentSideDevice.Enable();
            }
            else{
                csd.Disable();
            }
        }

        this.joiner = localJoiner;
    }

    private double startTime = 0;
    private PlayerID currentSlot = PlayerID.NP;
 

    public void OnMove(CallbackContext context)
    {
        if(context.control.device.deviceId == inputDevice.deviceId ){
            Vector2 val = context.ReadValue<Vector2>();

            if(Mathf.Abs(val.x) < 0.5f) return;
            if( (context.startTime - startTime) < 0.25f){
                return;
            }

            switch(currentSlot){
                case PlayerID.NP:
                    if(val.x > 0){
                        if(joiner.TryJoin(PlayerID.Player2, inputDevice)){
                            ToggleState(PlayerID.Player2);
                            startTime = context.startTime;
                        }
                    }
                    else{
                        if(joiner.TryJoin(PlayerID.Player1, inputDevice)){
                            ToggleState(PlayerID.Player1);
                            startTime = context.startTime;
                        }
                    }
                break;
                case PlayerID.Player1:
                    if(val.x > 0){
                        ToggleState(PlayerID.NP);
                        startTime = context.startTime;
                        joiner.RemovePlayer(inputDevice);
                    }
                break;
                case PlayerID.Player2:
                    if(val.x < 0){
                        ToggleState(PlayerID.NP);
                        startTime = context.startTime;
                        joiner.RemovePlayer(inputDevice);
                    }
                break;
            }
        }
    }

    private void ToggleState(PlayerID playerID)
    {
        currentSideDevice.ToggleState(playerID);

        currentSlot = playerID;
    }
}

[System.Serializable]
public class ControllerSideDevice{
    public string path;
    [SerializeField] private Image DeviceOnP1 = null;
    [SerializeField] private Image DeviceOnP2 = null;
    [SerializeField] private Image DeviceOnNeut = null;

    public void Disable()
    {
        DeviceOnP1.gameObject.SetActive(false);
        DeviceOnP2.gameObject.SetActive(false);
        DeviceOnNeut.gameObject.SetActive(false);
    }

    public void Enable()
    {
        DeviceOnP1.gameObject.SetActive(true);
        DeviceOnP2.gameObject.SetActive(true);
        DeviceOnNeut.gameObject.SetActive(true);
    }

    public void ToggleState(PlayerID playerID)
    {
        switch(playerID){
            case PlayerID.NP:
                DeviceOnP1.DOFade(0.15f, 0.25f);
                DeviceOnP2.DOFade(0.15f, 0.25f);
                DeviceOnNeut.DOFade(1, 0.25f);
            break;
            case PlayerID.Player1:
                DeviceOnP1.DOFade(1, 0.25f);
                DeviceOnP2.DOFade(0.15f, 0.25f);
                DeviceOnNeut.DOFade(0.15f, 0.25f);
            break;
            case PlayerID.Player2:
                DeviceOnP1.DOFade(0.15f, 0.25f);
                DeviceOnP2.DOFade(1f, 0.25f);
                DeviceOnNeut.DOFade(0.15f, 0.25f);
            break;
        }
    }
}