using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public Transform playerCar;
    public List<Transform> AICars;

    public List<GameObject> buttons;

    public int cameraIndex = 0;

    private Rigidbody targetRB;
    public Vector3 Offset;
    public float speed;

    public ButtonController cameraButton;
    private bool isCameraButtonAlreadyPressed = false;


    // Start is called before the first frame update
    void Start()
    {
        target = playerCar;
        targetRB = target.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (cameraButton.isPressed && !isCameraButtonAlreadyPressed)
        {
            cameraIndex++;
            if (cameraIndex == AICars.Count + 1)
            {
                cameraIndex = 0;
            }

            if (cameraIndex == 0)
            {
                target = playerCar;
                foreach(GameObject b in buttons)
                {
                    b.SetActive(true);
                }
            }
            else
            {
                target = AICars[cameraIndex-1];
                foreach (GameObject b in buttons)
                {
                    b.SetActive(false);
                }
            }

            isCameraButtonAlreadyPressed = true;
        }
        else if (!cameraButton.isPressed)
        {
            isCameraButtonAlreadyPressed = false;
        }

        Vector3 targetForward = (targetRB.velocity + target.transform.forward).normalized;
        transform.position = Vector3.Lerp(transform.position,
            target.position + target.transform.TransformVector(Offset)
            + targetForward * (-5f),
            speed * Time.deltaTime);
        transform.LookAt(target);
    }
}
