/// <summary>
/// Interface for objects that need long initialization<br>
/// Use it with Initialization singleton</br>
/// </summary>
namespace Game.Core {
	public interface IInitializable
	{
		/// <summary>
		/// Get initialization progress <br>
		/// Progress should be <b>[0..1]</b>!</br>
		/// </summary>
		public float GetInitProgress();
	}
}
