using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public bool isAI;

    // Serializables
    public WheelColliders wheelColliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;

    // Input
    public float inputGas;
    public float inputBrake;
    public float inputSteering;


    private Rigidbody carRB;
    public GameObject smokePrefab;

    // Car Settings
    public float carGasPower;
    public float carBrakePower;
    private float carSlipAngle;
    public float carSpeed;
    public float maxSteering;
    public AnimationCurve carSteeringCurve;

    

    public ButtonController gasPedal;
    public ButtonController brakePedal;

    public ButtonController leftButton;
    public ButtonController rightButton;



    // Start is called before the first frame update
    void Start()
    {
        carRB = gameObject.GetComponent<Rigidbody>();

        InstantiateSmoke();
    }


    void InstantiateSmoke()
    {
        wheelParticles.WheelFrontLeft = Instantiate(smokePrefab, wheelColliders.WheelFrontLeft.transform.position - Vector3.up * wheelColliders.WheelFrontLeft.radius, Quaternion.identity, wheelColliders.WheelFrontLeft.transform).GetComponent<ParticleSystem>();
        wheelParticles.WheelFrontRight = Instantiate(smokePrefab, wheelColliders.WheelFrontRight.transform.position - Vector3.up * wheelColliders.WheelFrontRight.radius, Quaternion.identity, wheelColliders.WheelFrontRight.transform).GetComponent<ParticleSystem>();
        wheelParticles.WheelRearLeft = Instantiate(smokePrefab, wheelColliders.WheelRearLeft.transform.position - Vector3.up * wheelColliders.WheelRearLeft.radius, Quaternion.identity, wheelColliders.WheelRearLeft.transform).GetComponent<ParticleSystem>();
        wheelParticles.WheelRearRight = Instantiate(smokePrefab, wheelColliders.WheelRearRight.transform.position - Vector3.up * wheelColliders.WheelRearRight.radius, Quaternion.identity, wheelColliders.WheelRearRight.transform).GetComponent<ParticleSystem>();
    }


    // Update is called once per frame
    void Update()
    {
        carSpeed = carRB.velocity.magnitude;

        if (!isAI)
        {
            CheckInput();

            ApplySteering(inputSteering * carSteeringCurve.Evaluate(carSpeed));
        }


        ApplyBrake();
        ApplyGas();
        ApplyUpdateWheels();

        ApplyParticles();
    }


    public void SetInput(float throttleIn, float steeringIn)
    {
        inputGas = throttleIn;

        ApplySteering(steeringIn);

        carSlipAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward);

        //fixed code to brake even after going on reverse by Andrew Alex 
        float movingDirection = Vector3.Dot(transform.forward, carRB.velocity);

        if (movingDirection < -0.5f && inputGas > 0)
        {
            inputBrake = Mathf.Abs(inputGas);
        }
        else if (movingDirection > 0.5f && inputGas < 0)
        {
            inputBrake = Mathf.Abs(inputGas);
        }
        else
        {
            inputBrake = 0;
        }
    }



    void CheckInput()
    {
        inputGas = Input.GetAxis("Vertical");

        if (gasPedal.isPressed)
        {
            inputGas += gasPedal.dampenPress;
        }

        if (brakePedal.isPressed)
        {
            inputGas -= brakePedal.dampenPress;
        }

        inputSteering = Input.GetAxis("Horizontal");

        if (leftButton.isPressed)
        {
            inputSteering -= leftButton.dampenPress;
        }

        if (rightButton.isPressed)
        {
            inputSteering += rightButton.dampenPress;
        }

        carSlipAngle = Vector3.Angle(transform.forward, carRB.velocity-transform.forward);

        if (carSlipAngle < 120f)
        {
            if (inputGas < 0)
            {
                inputBrake = Mathf.Abs(inputGas);
                inputGas = 0;
            }
            else
            {
                inputBrake = 0;
            }
        }
        else
        {
            inputBrake = 0;
        }
    }

    void ApplyBrake()
    {
        wheelColliders.WheelFrontLeft.brakeTorque = inputBrake * carBrakePower * 0.7f;
        wheelColliders.WheelFrontRight.brakeTorque = inputBrake * carBrakePower * 0.7f;
        wheelColliders.WheelRearLeft.brakeTorque = inputBrake * carBrakePower * 0.3f;
        wheelColliders.WheelRearRight.brakeTorque = inputBrake * carBrakePower * 0.3f;
    }

    void ApplySteering(float steeringAngle)
    {
        

        //float steeringAngle = inputSteering * carSteeringCurve.Evaluate(carSpeed);
        //wheelColliders.WheelFrontLeft.steerAngle = steeringAngle;
        //wheelColliders.WheelFrontRight.steerAngle = steeringAngle;

        if (carSlipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, carRB.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteering, maxSteering);

        wheelColliders.WheelFrontLeft.steerAngle = steeringAngle;
        wheelColliders.WheelFrontRight.steerAngle = steeringAngle;
    }

    void ApplyGas()
    {
        wheelColliders.WheelRearLeft.motorTorque = carGasPower * inputGas;
        wheelColliders.WheelRearRight.motorTorque = carGasPower * inputGas;
    }


    void ApplyUpdateWheels()
    {
        UpdateWheel(wheelColliders.WheelFrontLeft, wheelMeshes.WheelFrontLeft);
        UpdateWheel(wheelColliders.WheelFrontRight, wheelMeshes.WheelFrontRight);
        UpdateWheel(wheelColliders.WheelRearLeft, wheelMeshes.WheelRearLeft);
        UpdateWheel(wheelColliders.WheelRearRight, wheelMeshes.WheelRearRight);
    }


    void ApplyParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        wheelColliders.WheelFrontLeft.GetGroundHit(out wheelHits[0]);
        wheelColliders.WheelFrontRight.GetGroundHit(out wheelHits[1]);
        wheelColliders.WheelRearLeft.GetGroundHit(out wheelHits[2]);
        wheelColliders.WheelRearRight.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.5f;

        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            wheelParticles.WheelFrontLeft.Play();
        }
        else
        {
            wheelParticles.WheelFrontLeft.Stop();
        }

        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            wheelParticles.WheelFrontRight.Play();
        }
        else
        {
            wheelParticles.WheelFrontRight.Stop();
        }

        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            wheelParticles.WheelRearLeft.Play();
        }
        else
        {
            wheelParticles.WheelRearLeft.Stop();
        }

        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            wheelParticles.WheelRearRight.Play();
        }
        else
        {
            wheelParticles.WheelRearRight.Stop();
        }
    }


    void UpdateWheel(WheelCollider collider, MeshRenderer mesh)
    {
        Quaternion rotationQuat;
        Vector3 position3;

        collider.GetWorldPose(out position3, out rotationQuat);

        mesh.transform.position = position3;
        mesh.transform.rotation = rotationQuat;
    }
}

[System.Serializable]
public class WheelColliders
{
    public WheelCollider WheelFrontLeft;
    public WheelCollider WheelFrontRight;
    public WheelCollider WheelRearLeft;
    public WheelCollider WheelRearRight;
}

[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer WheelFrontLeft;
    public MeshRenderer WheelFrontRight;
    public MeshRenderer WheelRearLeft;
    public MeshRenderer WheelRearRight;
}

[System.Serializable]
public class WheelParticles
{
    public ParticleSystem WheelFrontLeft;
    public ParticleSystem WheelFrontRight;
    public ParticleSystem WheelRearLeft;
    public ParticleSystem WheelRearRight;
}