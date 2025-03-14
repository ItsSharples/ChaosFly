using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera controlledCamera;
    public CameraConfig config;

    public bool spin = false;
    public float spinSpeed = 0.025f;

	// Start is called before the first frame update
	void Start()
    {
        if (controlledCamera == null)
        {
			controlledCamera = FindObjectOfType<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spin)
        {
            config.longitude += Time.fixedDeltaTime * spinSpeed;
        }

        controlledCamera.transform.position = config.Position;
        controlledCamera.transform.LookAt(Vector3.zero);
    }
}

