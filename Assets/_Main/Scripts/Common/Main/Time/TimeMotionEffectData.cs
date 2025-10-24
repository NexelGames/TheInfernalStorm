namespace Game.Core {
    /// <summary>
    /// Contains main settings for slow-motion effect
    /// </summary>
    [System.Serializable]
    public class TimeMotionEffectData {
        [UnityEngine.Min(0.01f)]
        public float Scale = 0.2f;
        public float TimeIn = 0.5f;
        public float Duration = 0.5f;
        public float TimeOut = 0.5f;
        public UnityEngine.AnimationCurve CurveIn = UnityEngine.AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public UnityEngine.AnimationCurve CurveOut = UnityEngine.AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}