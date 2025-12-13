/* using System;
using UnityEngine;

[ExecuteAlways]
public class AeroSimAdapter : MonoBehaviour {
    [Header("Simulation constants")]
    [Range(7, 12)] [SerializeField] private float gravity = 9.8f;
    [Range(0, 5)] [SerializeField] private float airDensity = 1.293f;
    
    [Header("Airplane specifications")]
    [SerializeField] private float mass;
    [Range(0, 200)] [SerializeField] private float speed;
    [SerializeField] private Transform centerOfLift, centerOfGravity;

    [Header("Wing specifications")]
    [Range(100, 200)] [SerializeField] private float wingSurfaceArea;
    [Range(16, 20)] [SerializeField] private float wingCriticalAngle;
    [Range(0, 20)] [SerializeField] private float wingMountAngle;
    
    [Header("Visualization parameters")]
    [Range(0, 1)] [SerializeField] private float forceDisplayScale;
    [SerializeField] private Rigidbody rb;

    [Header("READONLY for debugging purpouses")]
    [SerializeField] private float angleOfAttack;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 lift, drag, weight;
    
    private Airplane simAirplane;
    
    void Start() {
        
    }

    void Update() {
        simAirplane = new Airplane(mass, wingSurfaceArea, wingMountAngle, wingCriticalAngle);
        simAirplane.Velocity = Vector3.forward * speed;
        velocity = simAirplane.Velocity;
        simAirplane.SetPitchYawRoll(-transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

        angleOfAttack = simAirplane.AngleOfAttack;

        lift = simAirplane.ComputeLift(airDensity);
        weight = simAirplane.ComputeWeight(gravity);
        drag = simAirplane.ComputeDrag(airDensity);

        rb.centerOfMass = simAirplane.CG;
        rb.mass = simAirplane.Mass;
        rb.AddForceAtPosition(weight, simAirplane.CG);
        rb.AddForceAtPosition(lift, simAirplane.CL);
    }

    public void OnDrawGizmos() {
        DisplayForceAtPosition(centerOfLift.position, lift, Color.green);
        Gizmos.DrawSphere(centerOfLift.position, .2f);
        DisplayForceAtPosition(centerOfGravity.position, weight, Color.red);
        Gizmos.DrawSphere(centerOfGravity.position, .2f);
        DisplayForceAtPosition(centerOfGravity.position, drag, Color.blue);
    }

    private void DisplayForceAtPosition(Vector3 position, Vector3 force, Color? color = null) {
        if (color != null) Gizmos.color = (Color)color;
        Gizmos.DrawLine(position, position + force / mass * forceDisplayScale);
    }
}
 */