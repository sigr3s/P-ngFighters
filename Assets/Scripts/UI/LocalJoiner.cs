﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalJoiner : MonoBehaviour
{
    [SerializeField] private Transform content = null;
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private GameObject startGame = null;
    [SerializeField] private CanvasGroup canvasGroup = null;

    private Dictionary<InputDevice, GameObject> UIDevices = new Dictionary<InputDevice, GameObject>();
    private bool gameLoading = false;
    private InputAction startAction;


    private void Start() {
        DataUtility.gameData.player1Device = null;
        DataUtility.gameData.player2Device = null;
    }

    private bool init = false;

    private void Update() {
        if(canvasGroup.interactable && DataUtility.gameData.player1Device != null && DataUtility.gameData.player2Device != null){
            startGame.SetActive(true);
        }
        else{
            startGame.SetActive(false);
        }

        if(canvasGroup.interactable){
            if(!init){
                var devices = InputSystem.devices.Where(x => x is Gamepad || x is Keyboard );

                foreach(var d in devices){
                    GameObject go = Instantiate(prefab, content);
                    UIDevices.Add(d, go);
                    go.GetComponent<ControllerSide>().Initialize(d, this);
                }

                InputSystem.onDeviceChange += OnChange;

                startAction = new InputAction();
                startAction.AddBinding("<Gamepad>/Start");
                startAction.AddBinding("<Keyboard>/Enter");
                startAction.performed += OnStart;
                startAction.Enable();

                init = true;
            }
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
       StartGmae();
    }

    public void StartGmae(){
        if(!gameLoading && startGame.activeSelf){
            gameLoading = true;
            DataUtility.gameData.isNetworkedGame = false;
            startAction.Disable();
            SceneManager.LoadScene("Game");
        }
    }

    private void OnChange(InputDevice d, InputDeviceChange state)
    {
        Debug.Log($"Changed {d.name}   {state}");

        switch(state){
            case InputDeviceChange.Added:
                GameObject go = Instantiate(prefab, content);
                UIDevices.Add(d, go);
                go.GetComponent<ControllerSide>().Initialize(d, this);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) content);
            break;

            case InputDeviceChange.Removed:
                if(UIDevices.ContainsKey(d)){
                    GameObject g = UIDevices[d];
                    Destroy(g);
                    UIDevices.Remove(d);
                    RemovePlayer(d);
                }
            break;
        }
    }

    public bool TryJoin(PlayerID player, InputDevice device)
    {
        if(gameLoading || !canvasGroup.interactable) return false;
        switch(player){
            case PlayerID.Player1:
                if(DataUtility.gameData.player1Device == null){
                    DataUtility.gameData.player1Device = device;
                    return true;
                }
            break;
            case PlayerID.Player2:
                if(DataUtility.gameData.player2Device == null){
                    DataUtility.gameData.player2Device = device;
                    return true;
                }
            break;
        }

        return false;
    }

    public void RemovePlayer(InputDevice device)
    {
        if(gameLoading) return;

        if(DataUtility.gameData.player1Device != null && DataUtility.gameData.player1Device.deviceId == device.deviceId){
            DataUtility.gameData.player1Device = null;
        }

        if(DataUtility.gameData.player2Device != null && DataUtility.gameData.player2Device.deviceId == device.deviceId){
            DataUtility.gameData.player2Device = null;
        }
    }
}
