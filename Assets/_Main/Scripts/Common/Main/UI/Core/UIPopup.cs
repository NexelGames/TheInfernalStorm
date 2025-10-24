using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI {
    public enum PopupResults : byte {
        Closed,
        Accepted,
        Denied
    }

    [System.Serializable]
    public class PopupButton {
        public PopupResults ClickResult;
        public Button ButtonReferece;
    }

    public class UIPopup : UICanvasController {
        /// <summary>
        /// Can be used to stop player movement and etc
        /// </summary>
        public static UnityEvent<UIPopup> OnOpen = new();
        /// <summary>
        /// Can be used to unlock player movement and etc
        /// </summary>
        public static UnityEvent<UIPopup> OnClose = new();

        [HideInInspector] public UnityEvent OnPopupOpen = new();
        /// <summary>
        /// You can subscribe and get <see cref="Result"/> value
        /// </summary>
        [HideInInspector] public UnityEvent OnPopupClose = new();

        public PopupResults Result => _result;
        public bool SelfHideOnResult => _hidePopupOnResult;

        [Header("Dialog settings")]
        [SerializeField] private bool _showOnTopOtherUIElements;
        [SerializeField] private PopupButton[] _popupButtons;
        [SerializeField] protected float _popupDisappearFadeDuration = 0.2f;
        [SerializeField] protected float _popupAppearFadeDuration = 0.2f;

        [SerializeField] protected bool _hidePopupOnResult = true;

        protected PopupResults _result;
        private UnityAction<PopupResults> _onResultCallback;

        public override void InitReferences() {
            base.InitReferences();

            for (int i = 0; i < _popupButtons.Length; i++) {
                switch (_popupButtons[i].ClickResult) {
                    case PopupResults.Closed:
                        _popupButtons[i].ButtonReferece.onClick.AddListener(OnPlayerCloseClicked);
                        break;
                    case PopupResults.Accepted:
                        _popupButtons[i].ButtonReferece.onClick.AddListener(OnPlayerAcceptClicked);
                        break;
                    case PopupResults.Denied:
                        _popupButtons[i].ButtonReferece.onClick.AddListener(OnPlayerDenyClicked);
                        break;
                }
            }
        }

        /// <summary>
        /// User action callback, new callback will override previous one<br>
        /// You can also use <see cref="OnPopupClose"/> on referenced UIPopup object that will fire events every time popup closes</br>
        /// </summary>
        public void SetResultCallback(UnityAction<PopupResults> callback) {
            _onResultCallback = callback;
        }

        /// <summary>
        /// To override Show logic it use <see cref="EnableCanvas(bool, float)"/> override
        /// </summary>
        public void Show() => EnableCanvas(true, _popupAppearFadeDuration);

        /// <summary>
        /// To override Hide logic it use <see cref="EnableCanvas(bool, float)"/> override
        /// </summary>
        public void Hide() => EnableCanvas(false, _popupDisappearFadeDuration);

        public override void EnableCanvas(bool active, float fadeDuration) {
            if (active) {
                if (_showOnTopOtherUIElements) {
                    transform.SetAsLastSibling(); // so it will be on top of other elements inside canvas
                }
                base.EnableCanvas(active, fadeDuration);
                OnOpen.Invoke(this);
                OnPopupOpen.Invoke();
            }
            else {
                base.EnableCanvas(active, fadeDuration);
                OnClose.Invoke(this);
                OnPopupClose.Invoke();
            }
        }

        protected void InvokeResultCallback() {
            _onResultCallback?.Invoke(_result);
        }

        /// <returns>Returns first button from <see cref="_popupButtons"/> with <see cref="PopupResults"/> result</returns>
        protected PopupButton GetButton(PopupResults withResult) {
            for (int i = 0; i < _popupButtons.Length; i++) {
                if (_popupButtons[i].ClickResult.Equals(withResult)) {
                    return _popupButtons[i];
                }
            }
            return null;
        }

        protected virtual void OnPlayerAcceptClicked() {
            _result = PopupResults.Accepted;
            InvokeResultCallback();
            if (_hidePopupOnResult) Hide();
        }

        protected virtual void OnPlayerDenyClicked() {
            _result = PopupResults.Denied;
            InvokeResultCallback();
            if (_hidePopupOnResult) Hide();
        }

        protected virtual void OnPlayerCloseClicked() {
            _result = PopupResults.Closed;
            InvokeResultCallback();
            if (_hidePopupOnResult) Hide();
        }
    }
}