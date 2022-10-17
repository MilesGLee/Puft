using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Camera : MonoBehaviour
{
    [SerializeField] private Transform _target;

    void Update()
    {
        //Move object to targets position
        transform.position = _target.position;
    }
}
