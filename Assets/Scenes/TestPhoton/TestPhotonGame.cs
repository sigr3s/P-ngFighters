using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class TestPhotonGame : MonoBehaviour
{
    string pathRelativeToResources = "PhotonPrefabs";
    string prefabName => "PhotonDummyPlayer";

    void Start() 
    {        
        PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), Vector3.zero, Quaternion.identity);
    }
}
