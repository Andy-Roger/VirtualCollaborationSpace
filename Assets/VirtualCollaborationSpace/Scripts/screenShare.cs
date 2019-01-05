using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class screenShare : MonoBehaviour
{
    private CallAppUi callappUI;
    private VdmDesktop vdmDesktop;
    public static bool started = false;
    public Dropdown videoDropDowns;
    public Transform virtualCamera;

	void Start ()
    {
        //callappUI = FindObjectOfType<CallAppUi>();
        Invoke("joinAndShareScreen", 3);
    }

    void joinAndShareScreen()
    {
        //videoDropDowns.value = 5;
        //callappUI.JoinButtonPressed();
        vdmDesktop = FindObjectOfType<VdmDesktop>();
        vdmDesktop.Show();
        virtualCamera.parent = vdmDesktop.transform;
    }
}
