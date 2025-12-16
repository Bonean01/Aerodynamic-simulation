using UnityEngine;

[CreateAssetMenu(fileName = "AirplaneConfig", menuName = "Scriptable Objects/AirplaneConfig")]
public class AirplaneConfig : ScriptableObject {
    [Header("Airplane specifications")]
    [SerializeField] private float mass;
    [SerializeField] private AerodynamicSurface wings, tail;

    [Header("Engine specifications")]
    [SerializeField] private float engineNumber;
    [SerializeField] private SubsonicEngine engine;

    public float Mass => mass;
    public AerodynamicSurface Wings => wings;
    public AerodynamicSurface Tail => tail;
    public float EngineNumber => engineNumber;
    public SubsonicEngine Engine => engine;
}
