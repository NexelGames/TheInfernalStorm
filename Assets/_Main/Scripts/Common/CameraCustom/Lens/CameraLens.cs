namespace Game.Core
{
    public interface CameraLens
    {
        public bool orthographic { get; set; }
        public float aspect { get; set; }
        public float fieldOfView { get; set; }
        public float orthographicSize { get; set; }
    }
}