using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;

public class clientTypeManager : MonoBehaviour
{
    public static bool isVR = false;
    private Player player;

    public GameObject avatar;
    private GameObject spawnedAvatar;
    public Transform headTransform;

    public GameObject avatarHand;
    private GameObject spawnedAvatarHand1;
    private GameObject spawnedAvatarHand2;
    private bool handAreInScene = false;

    private void Awake()
    {
        spawnedAvatar = PhotonNetwork.Instantiate(avatar.name, headTransform.position, headTransform.rotation, 0);
        spawnedAvatarHand1 = PhotonNetwork.Instantiate(avatarHand.name, Vector3.zero, Quaternion.identity, 0);
        spawnedAvatarHand2 = PhotonNetwork.Instantiate(avatarHand.name, Vector3.zero, Quaternion.identity, 0);
        
        foreach (MeshRenderer mesh in spawnedAvatar.GetComponentsInChildren<MeshRenderer>())
        {
            mesh.enabled = false;
        }
    }

    void Start ()
    {
        player = FindObjectOfType<Player>();
        Invoke("VRStatusCheck", 2);
    }

    private void OnDisable()
    {
        PhotonNetwork.Destroy(spawnedAvatarHand1.GetComponent<PhotonView>());
        PhotonNetwork.Destroy(spawnedAvatarHand2.GetComponent<PhotonView>());
        PhotonNetwork.Destroy(avatar.GetComponent<PhotonView>());
    }

    void Update()
    {
        spawnedAvatar.transform.position = headTransform.position;
        spawnedAvatar.transform.rotation = headTransform.rotation;

        if(isVR)
        {
            if (player.hands[0].gameObject.activeInHierarchy)
            {
                spawnedAvatarHand1.transform.position = player.hands[0].transform.position;
                spawnedAvatarHand1.transform.rotation = player.hands[0].transform.rotation;
            }
            if (player.hands[1].gameObject.activeInHierarchy)
            {
                spawnedAvatarHand2.transform.position = player.hands[1].transform.position;
                spawnedAvatarHand2.transform.rotation = player.hands[1].transform.rotation;
            }
        }
    }

    void VRStatusCheck()
    {
        if (XRSettings.isDeviceActive)
        {
            ActivateRig(player.rigSteamVR);
            isVR = true;
        }
        else
        {
            ActivateRig(player.rig2DFallback);
            isVR = false;
        }
    }

    public void toggleVR()
    {
        if(!isVR && XRSettings.isDeviceActive)
        {
            ActivateRig(player.rigSteamVR);
            isVR = true;
        }
        else
        {
            ActivateRig(player.rig2DFallback);
            isVR = false;
        }
    }

    private void ActivateRig(GameObject rig)
    {
        player.rigSteamVR.SetActive(rig == player.rigSteamVR);
        player.rig2DFallback.SetActive(rig == player.rig2DFallback);

        if (player.audioListener)
        {
            player.audioListener.transform.parent = player.hmdTransform;
            player.audioListener.transform.localPosition = Vector3.zero;
            player.audioListener.transform.localRotation = Quaternion.identity;
        }
    }
}
