using System;
using UnityEngine;

[Serializable]
public class SubsonicEngine {
    [SerializeField] private float engineMaxPower, engineIntakeArea,
    enginePressureRatio, engineTemperatureRatio, adiabaticNozzleEfficiency;
    [SerializeField] private float engineMountAngle;

    public Vector3 ComputeThrust(float altitude, float inletAirspeed, Atmosphere atmosphere) {
        float airDensity = atmosphere.ComputeAirDensity(altitude);
        float massFlowRate = engineIntakeArea * inletAirspeed * airDensity;
        float inletTemperature = atmosphere.ComputeTemperature(altitude);
        float totalNozzleTemperature = inletTemperature / engineTemperatureRatio;
        float nozzlePressureRatio = enginePressureRatio;
        float airSpecificHeatRatio = atmosphere.GetAirSpecificHeatRatio();
        float exp = (airSpecificHeatRatio - 1) / airSpecificHeatRatio;
        float exitAirspeed = Mathf.Sqrt(2 * atmosphere.GetAirSpecificHeat() * adiabaticNozzleEfficiency * totalNozzleTemperature 
        * Mathf.Pow(1 - 1 / nozzlePressureRatio, exp));
        float thrust = massFlowRate * (exitAirspeed - inletAirspeed);
        
        Vector3 direction = new (0, Mathf.Sin(engineMountAngle * Mathf.Deg2Rad), Mathf.Cos(engineMountAngle * Mathf.Deg2Rad));
        return thrust * direction;
    }
}
