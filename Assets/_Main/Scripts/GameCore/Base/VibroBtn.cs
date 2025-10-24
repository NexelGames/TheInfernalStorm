using Game.Audio;
using Lofelt.NiceVibrations;
using PrimeTween;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Vibro Button")]
    public class VibroBtn : Button {
        public static event Action<HapticPatterns.PresetType> OnClick;
        public static event Action<SoundAudioClip> OnSoundClick;
        public HapticPatterns.PresetType VibrationType = HapticPatterns.PresetType.LightImpact;
        public bool UseMultiImageTint = false;
        public bool UseAnimation = true;
        public bool UseClickSound = false;
        public SoundAudioClip ClickSound;

        private static readonly float AnimationScale = 0.9f;
        private static readonly float AnimationTime = 0.1f;

        private Vector4 _startRaycastPadding;
        private float _startScale;
        private Tween _scaleTween;

        private Graphic[] _graphics;

#if UNITY_EDITOR
        public static void Editor_ClearEvents() {
            OnClick = null;
            OnSoundClick = null;
        }
#endif

        protected override void Awake() {
            base.Awake();

            if (UseMultiImageTint && _graphics == null) {
                _graphics = GetComponentsInChildren<Graphic>(true);
            }

            if (UseAnimation) {
                _startScale = transform.localScale.x;
                _startRaycastPadding = targetGraphic.raycastPadding;
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            OnClick?.Invoke(VibrationType);
            if (UseClickSound) {
                OnSoundClick?.Invoke(ClickSound);
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            if (UseMultiImageTint == false || transition != Transition.ColorTint) {
                base.DoStateTransition(state, instant);
                return;
            }

            var targetColor =
                state == SelectionState.Disabled ? colors.disabledColor :
                state == SelectionState.Highlighted ? colors.highlightedColor :
                state == SelectionState.Normal ? colors.normalColor :
                state == SelectionState.Pressed ? colors.pressedColor :
                state == SelectionState.Selected ? colors.selectedColor : Color.white;

            if (_graphics == null) {
                _graphics = GetComponentsInChildren<Graphic>(true);
            }

            foreach (var graphic in _graphics)
                graphic.CrossFadeColor(targetColor, instant ? 0 : colors.fadeDuration, true, true);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            if (UseAnimation && interactable) {
                Vector2 buttonSize = ((RectTransform)transform).sizeDelta;
                Vector2 raycastDelta = (buttonSize - (buttonSize * AnimationScale)) * 0.5f;
                targetGraphic.raycastPadding -= new Vector4(raycastDelta.x, raycastDelta.y, raycastDelta.x, raycastDelta.y) / AnimationScale;

                _scaleTween.Stop();
                _scaleTween = Tween.Scale(transform, _startScale * AnimationScale, AnimationTime, useUnscaledTime: true);
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            if (UseAnimation && interactable) {
                targetGraphic.raycastPadding = _startRaycastPadding;

                _scaleTween.Stop();
                _scaleTween = Tween.Scale(transform, _startScale, AnimationTime, useUnscaledTime: true);
            }
        }
    }
}