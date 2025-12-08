using System;
using UnityEngine;
using NUnit.Framework;

public class AerodynamicSurface {
    /// <summary>The surface area of this aerodynamic surface in square meters.</summary>
    protected float surfaceArea;
    /// <summary>Angle at which this surface has been mounted relative to the aircraft.</summary>
    protected float mountAngle;
    /// <summary>Angle at which this surface will plummit lift production and will enter a stalled attitude (usually varies between 16º and 20º).</summary>
    protected float criticalAngle;
    /// <summary>Lift coeficient at the critical angle.</summary>
    protected float maxLiftCoeficient;

    public AerodynamicSurface(float surfaceArea, float mountAngle = 0, float criticalAngle = 20, float maxLiftCoeficient = 1.5f) {
        this.surfaceArea = surfaceArea;
        this.mountAngle = mountAngle;
        this.criticalAngle = criticalAngle;
        this.maxLiftCoeficient = maxLiftCoeficient;
    }


    /// <summary>
    /// Returns the mount angle in degrees.
    /// </summary>
    /// <returns></returns>
    public float GetMountAngle() => mountAngle;

    /// <summary>
    /// Computes the lift coeficient based on the current angle of attack and the set criticalAngle.
    /// </summary>
    /// <returns>The lift coeficient.</returns>
    private float ComputeLiftCoeficient(float AOA) {
        float effectiveAOA = Mathf.Abs(AOA);
        float lift = effectiveAOA < criticalAngle ? 
        maxLiftCoeficient / criticalAngle * effectiveAOA :                                     // not in stall, linearly increase lift.
        -(effectiveAOA - criticalAngle)*(effectiveAOA - criticalAngle) + maxLiftCoeficient;   // in stall, quadratically decrease lift.
        return lift < 0 ? 0 : Mathf.Sign(AOA) * lift;
    }

    /// <summary>
    /// Computes the drag coeficient based on the current angle of attack.
    /// </summary>
    /// <returns>The drag coeficient.</returns>
    private float ComputeDragCoeficient(float angleOfAttack) {
        // Uses a quadratic equation to compute the drag coeficient, the coeficients for it have been
        // extracted from the data provided by the FAA's chapter 5 on the aerodynamics of flight.
        return 0.00061f * angleOfAttack*angleOfAttack + 0.02f;
    }

    /// <summary>
    /// Computes the total lift using the formula: L = 1/2 * V*V * S * C_L * ρ.
    /// </summary>
    /// <returns></returns>
    public float ComputeLift(float angleOfAttack, float airspeed, float airDensity) {
        float liftCoeficient = ComputeLiftCoeficient(angleOfAttack + mountAngle);
        //Debug.Log("Lift coeficient: " + liftCoeficient);
        //Debug.Log("Surface area: " + this.surfaceArea);
        //Debug.Log("Angle of attack: " + angleOfAttack);
        //Debug.Log("Airspeed: " + airspeed);
        //Debug.Log("Air density: " + airDensity);
        return 1f/2f * airspeed*airspeed * this.surfaceArea * liftCoeficient * airDensity;
    }

    /// <summary>
    /// Computes the total drag using the formula: D = 1/2 V*V * S * C_D * ρ.
    /// </summary>
    /// <returns></returns>
    public float ComputeDrag(float angleOfAttack, float airspeed, float airDensity) {
        float dragCoeficient = ComputeDragCoeficient(angleOfAttack + mountAngle);
        return 1f/2f * airspeed*airspeed * this.surfaceArea * dragCoeficient * airDensity;
    }
}