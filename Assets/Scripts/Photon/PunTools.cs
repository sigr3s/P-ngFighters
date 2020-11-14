using System;
using UnityEngine;
using Photon.Pun;

public class PunTools : MonoBehaviour
{	
    public static void ExecuteIfMine(PhotonView photonView, Action action)
    {
        if(photonView.IsMine)
        {
            if(action != null){ action.Invoke(); }   
        }
    }

    public static void ExecuteIfMaster(Action action)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(action != null){ action.Invoke(); }    
        }
    }

    public static void ExecuteIfNotMine(PhotonView photonView, Action action)
    {
        if(!photonView.IsMine)
        {
            if(action != null){ action.Invoke(); }    
        }
    }

    public static void PhotonRpcMine(PhotonView photonView, string methodName, RpcTarget target, params object[] parameters)
    {
        ExecuteIfMine(photonView, () => photonView.RPC(methodName, target, parameters));
    }

    public static void PhotonRpcMineAndMaster(PhotonView photonView, string methodName, RpcTarget target, params object[] parameters)
    {
        ExecuteIfMaster(() => ExecuteIfMine(photonView, () => photonView.RPC(methodName, target, parameters)));
    }

    public static void PhotonRPC(PhotonView photonView, string methodName, RpcTarget target, params object[] parameters)
    {
        photonView.RPC(methodName, target, parameters);
    }
}