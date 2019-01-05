using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class meshExtrusionManager : MonoBehaviour
{
    public Hand hand;
    private GameObject extrudedObj;
    public GameObject extrudeCubePrefab;
    private GameObject remoteExtrudedCube;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update ()
    {
        //for networking send photon rpc's every tick of the hand or cube transform positions
        if (clientTypeManager.isVR)
        {
            if (hand.controller.GetHairTrigger())
            {
                if (extrudedObj != null)
                {
                    extrudedObj.transform.position = hand.transform.position;
                    extrudedObj.transform.rotation = hand.transform.rotation;

                    photonView.RPC("updateExtrudedCubesPosition", PhotonTargets.OthersBuffered, hand.transform.position, hand.transform.rotation);
                }
            }

            if (hand.controller.GetHairTriggerUp())
            {
                extrudedObj = null;
            }

            if (hand.controller.GetHairTriggerDown())
            {
                extrudedObj = Instantiate(extrudeCubePrefab, hand.transform.position, hand.transform.rotation, null);
                photonView.RPC("instantiateExtrudeCube", PhotonTargets.OthersBuffered, hand.transform.position, hand.transform.rotation);
            }
        }
    }

    //instantiate a new one
    //every tick set

    [PunRPC]
    void instantiateExtrudeCube(Vector3 handPosition, Quaternion handRotation)
    {
        //instantiate a cube thing and make a ref
        remoteExtrudedCube = Instantiate(extrudeCubePrefab, handPosition, handRotation, null);
    }

    [PunRPC]
    void updateExtrudedCubesPosition(Vector3 handPosition, Quaternion handRotation)
    {
        remoteExtrudedCube.transform.position = handPosition;
        remoteExtrudedCube.transform.rotation = handRotation;
    }
}
