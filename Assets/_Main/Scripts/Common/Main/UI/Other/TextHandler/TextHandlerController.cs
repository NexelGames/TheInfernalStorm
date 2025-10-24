using UnityEngine;

namespace Game.UI {
    /// <summary>
    /// Example of TextHandler usage
    /// </summary>
    public class TextHandlerController : MonoBehaviour {
        [SerializeField] private TextHandler _textHandler;

        [SerializeField] private string _text;
        [SerializeField] private float _printDelay = 0.5f;
        [SerializeField] private float _removeDelay = 0.5f;

        private void OnEnable() {
            _textHandler.SetText(string.Empty);
            _textHandler.PrintText(_text, _printDelay, 0f, () => {
                _textHandler.RemoveText(_removeDelay, leftToRight: true, 1f);
            });
        }
    }
}
