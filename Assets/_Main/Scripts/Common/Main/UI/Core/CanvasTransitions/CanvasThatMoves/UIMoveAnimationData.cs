using Extensions;
using UnityEngine;

namespace Game.UI {
    [System.Serializable]
    public class UIMoveAnimationData {
        [SerializeField] private RectTransform _elementsContainer;
        [SerializeField] private Vector2 _hideMoveDelta;
        [SerializeField] private AnimationCurve _appearMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _hideMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private Vector2 _hidenAchoredPositionWithSafeArea;
        private Vector2 _initRectAnchoredPosition;
        private RectSnapTypes _snappedTo;
        private SafeArea _connectedSafeArea;

        public void Init() {
            _initRectAnchoredPosition = _elementsContainer.anchoredPosition;
            _hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta;
            _snappedTo = _elementsContainer.GetSnapType();
            _connectedSafeArea = _elementsContainer.parent.GetComponent<SafeArea>();
        }

        /// <param name="safeArea"><see cref="SafeAreaManager.SafeAreaOffsets"/></param>
        public void RecalculateHidenPosition(Vector4 safeArea) {
			_hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta;
			
            if (_connectedSafeArea == null) {
                return;
            }

            if (_connectedSafeArea.ConformsX) {
                if (_snappedTo == RectSnapTypes.CenterOfLeftEdge) {
                    _hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta - new Vector2(safeArea.x, 0f);
                    return;
                }
                else if (_snappedTo == RectSnapTypes.CenterOfRightEdge) {
                    _hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta + new Vector2(safeArea.y, 0f);
                    return;
                }
            }

            if (_connectedSafeArea.ConformsY) {
                if (_snappedTo == RectSnapTypes.BottomLeftCorner || _snappedTo == RectSnapTypes.BottomRightCorner ||
                        _snappedTo == RectSnapTypes.CenterOfBottomEdge) {
                    _hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta - new Vector2(0f, safeArea.w);
                    return;
                }
                else if (_snappedTo == RectSnapTypes.TopLeftCorner || _snappedTo == RectSnapTypes.TopRightCorner ||
                        _snappedTo == RectSnapTypes.CenterOfTopEdge) {
                    _hidenAchoredPositionWithSafeArea = _initRectAnchoredPosition + _hideMoveDelta + new Vector2(0f, safeArea.z);
                    return;
                }
            }
        }

        public void Hide(float progress) {
            _elementsContainer.anchoredPosition = Vector2.LerpUnclamped(_initRectAnchoredPosition, _hidenAchoredPositionWithSafeArea,
                _hideMoveCurve.Evaluate(progress));
        }
        public void Appear(float progress) {
            _elementsContainer.anchoredPosition = Vector2.LerpUnclamped(_hidenAchoredPositionWithSafeArea, _initRectAnchoredPosition,
                _appearMoveCurve.Evaluate(progress));
        }
    }
}