using System;
using UnityEngine;

[Serializable]
public class SubsonicEngine {
    [SerializeField] private float maxEnginePressureRatio, engineIntakeArea,
    combustionTemperature, adiabaticNozzleEfficiency;

    public float ComputeThrust(float altitude, float inletAirspeed, float throttle, Atmosphere atmosphere) {
        float airDensity = atmosphere.ComputeAirDensity(altitude);
        float massFlowRateCap = ComputeMassFlowRateCap(altitude, atmosphere);
        float massFlowRate = Mathf.Min(engineIntakeArea * inletAirspeed * airDensity, massFlowRateCap);
        float airSpecificHeatRatio = atmosphere.GetAirSpecificHeatRatio();
        float enginePressureRatio = ComputeEnginePressureRatio(throttle);

        float exp = (airSpecificHeatRatio - 1) / airSpecificHeatRatio;
        float exitAirspeed = Mathf.Sqrt(2 * atmosphere.GetAirSpecificHeat() * adiabaticNozzleEfficiency * combustionTemperature
        * (1 - Mathf.Pow(1 / enginePressureRatio, exp)));


        float thrust = massFlowRate * (exitAirspeed - inletAirspeed);
        
        //Vector3 direction = new (0, Mathf.Sin(engineMountAngle * Mathf.Deg2Rad), Mathf.Cos(engineMountAngle * Mathf.Deg2Rad));
        Debug.Log($"air density: {airDensity}\tthrust: {thrust}");
        return thrust;
    }

    private float ComputeEnginePressureRatio(float throttle) => Mathf.Lerp(1, maxEnginePressureRatio, throttle);

    private float ComputeMassFlowRateCap(float altitude, Atmosphere atmosphere) {
        float airSpecificHeatRatio = atmosphere.GetAirSpecificHeatRatio();
        float temperature = atmosphere.ComputeTemperature(altitude);
        float massFlowRateCap = engineIntakeArea * atmosphere.ComputePressure(altitude);
        massFlowRateCap *= Mathf.Sqrt(airSpecificHeatRatio / (atmosphere.GetAirSpecificGasConstant() * temperature));
        float exp = (airSpecificHeatRatio + 1) / (2 * airSpecificHeatRatio - 2);
        massFlowRateCap *= Mathf.Pow(2 / (airSpecificHeatRatio + 1), exp);
        return massFlowRateCap;
    }
}
