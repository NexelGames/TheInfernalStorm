using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(InterpolatedTransformUpdater))]
public class Interpolation_Controller : MonoBehaviour
{
    private float[] _lastFixedUpdateTimes;
    private int _newTimeIndex;

    private static float _interpolationFactor;
    public static float InterpolationFactor
    {
        get { return _interpolationFactor; }
    }

    private void Awake() {
        GetComponent<InterpolatedTransformUpdater>().SubscribeToEvents();
    }

    public void Start()
    {
        _lastFixedUpdateTimes = new float[2];
        _newTimeIndex = 0;
    }

    public void FixedUpdate()
    {
        _newTimeIndex = OldTimeIndex();
        _lastFixedUpdateTimes[_newTimeIndex] = Time.fixedTime;
    }

    public void Update()
    {
        float newerTime = _lastFixedUpdateTimes[_newTimeIndex];
        float olderTime = _lastFixedUpdateTimes[OldTimeIndex()];

        if (newerTime != olderTime)
        {
            _interpolationFactor = (Time.time - newerTime) / (newerTime - olderTime);
        }
        else
        {
            _interpolationFactor = 1;
        }
    }

    private int OldTimeIndex()
    {
        return (_newTimeIndex == 0 ? 1 : 0);
    }
}
