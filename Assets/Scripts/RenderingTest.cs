using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingTest : MonoBehaviour
{
    public Camera Cam;

    public Material t;

    public List<MeshRenderer> list;

    private bool check;

    private void Awake()
    {
        check = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            Cam.Render();

            if (!check) 
            {
                check = true;
                foreach (MeshRenderer r in list) 
                {
                    r.material = t;
                }
            }
        }
    }
}
