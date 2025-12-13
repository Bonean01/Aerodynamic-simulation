using UnityEngine;

public class SimulationSettings : MonoBehaviour {
    [SerializeField] public static float gravity = 9.8f;
    [SerializeField] public static float airDensity = 1.293f;
    public static SimulationSettings Singleton { get; private set; }

    void Awake() {
        if (Singleton == null) Singleton = this;
    }
}
