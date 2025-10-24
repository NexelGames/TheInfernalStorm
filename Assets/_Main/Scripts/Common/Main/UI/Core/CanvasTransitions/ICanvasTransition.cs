namespace Game.UI {
    public interface ICanvasTransition {
        public void Init();
        public bool IsRunning { get; }
        /// <summary>
        /// Appear or disappear, depends on <paramref name="active"/> value
        /// </summary>
        public void DoFade(bool active, float fadeDuration);
        public void StopTransition();
    }
}