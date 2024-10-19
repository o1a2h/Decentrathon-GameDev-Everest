using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CarController))]

public class AiCarController : MonoBehaviour
{

    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public int currentWaypoint;
    private CarController carController;
    public float waypointRange;

    private float currentAngle;

    public float gasInput;
    public bool isInsideBraking;
    public float maxAngle = 45f;
    public float maxSpeed = 40f;


    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<CarController>();
        waypoints = waypointContainer.waypoints;
        currentWaypoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) < waypointRange)
        {
            currentWaypoint++;
            if (currentWaypoint == waypoints.Count)
            {
                currentWaypoint = 0;
            }
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        currentAngle = Vector3.SignedAngle(forward, waypoints[currentWaypoint].position - transform.position, Vector3.up);

        gasInput = Mathf.Clamp01(maxAngle - Mathf.Abs(carController.carSpeed * 0.01f * currentAngle) / maxAngle);
        if (isInsideBraking)
        {
            gasInput = -gasInput * (Mathf.Clamp01(carController.carSpeed / maxSpeed) * 2 - 1f);
        }

        carController.SetInput(gasInput, currentAngle);

        Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.green);
    }
}
