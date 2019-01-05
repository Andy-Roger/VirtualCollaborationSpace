using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class hoverTransformMover : MonoBehaviour
{
    private Hand hand;

    void Start()
    {
        hand = GetComponent<Hand>();
    }

    private void Update()
    {
        if (hand.hoveringInteractable == null)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<Interactable>())
                {
                    hand.hoverSphereTransform.position = hit.collider.bounds.center;
                }
            }
        }
        else
        {
            hand.hoverSphereTransform.position = transform.position;
        }

    }
}

