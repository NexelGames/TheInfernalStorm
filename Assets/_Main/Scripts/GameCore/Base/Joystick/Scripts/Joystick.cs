using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.JoystickUI {
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        /// <summary>
        /// returns true interaction on started, false interaction on ended
        /// </summary>
        [HideInInspector] public UnityEvent<bool> OnInteractionChange = new();

        public JoystickBehaviours BehaviourType => _behaviourType;

        public RectTransform StickBackgroundRect => _stickBackground;

        public bool IsInProgress => _isInProgress;
        /// <summary>
        /// Joystick move dir on plane XZ
        /// </summary>
        public Vector3 MoveDirection => _behaviour.MoveDirection;
        /// <summary>
        /// MoveCoef [0..1]
        /// </summary>
        public float MoveCoef => _behaviour.MoveCoef;
        /// <summary>
        /// returns MoveDirection * MoveCoef
        /// </summary>
        public Vector3 Movement => _behaviour.Movement;
        /// <summary>
        /// returns radius scaled with current resolution in pixels
        /// </summary>
        public float JoystickScaledBackgroundRadius => _stickBackground.sizeDelta.x * 0.5f * _rootUI_CanvasScalerRect.localScale.x;

        public bool IsEnabled {
            get => _isEnabled;
            set {
                if (value == false && _isEnabled) {
                    if (_isInProgress) {
                        _behaviour.OnTouchRelease();
                        OnInteractionChange.Invoke(false);

                        if (_hideAfterInteraction == false) UpdatePositionsAnchored(_startPosition, _startPosition);

                        _isInProgress = false;
                    }

                    SetVisible(false);
                }
                else if (value == true && _isEnabled != value) {
                    if (_hideAfterInteraction == false) SetVisible(true);
                }

                _isEnabled = value;
            }
        }

        [SerializeField] private JoystickSettingsSO _settings;
        [SerializeField] private RectTransform _rootUI_CanvasScalerRect;
        [SerializeField] private RectTransform _stickBackground;
        [SerializeField] private RectTransform _stick;
        [Tooltip("If you need static joystick zone - toggle it to false state and replace gameObject where it should be")]
        [SerializeField] private bool _hideAfterInteraction = true;

        internal JoystickBehaviour Behaviour => _behaviour;

        private Image _stickBackgroundImg;
        private Image _stickImg;

        private Vector3 _startPosition;
        private JoystickBehaviour _behaviour;
        private JoystickBehaviours _behaviourType;
        private bool _isInProgress = false;
        private bool _isEnabled = true;
        private int _pointerId;

        /// <param name="behaviourType">Set none to use default</param>
        public void Init(JoystickBehaviours behaviourType = JoystickBehaviours.None) {
            if (_rootUI_CanvasScalerRect == null) {
                _rootUI_CanvasScalerRect = (RectTransform)GetComponentInParent<CanvasScaler>(true).transform;
            }

            if (behaviourType == JoystickBehaviours.None) {
                _behaviourType = _settings.DefaultJoystickBehaviour;
            }
            else {
                _behaviourType = behaviourType;
            }

            _stickBackgroundImg = _stickBackground.GetComponent<Image>();
            _stickImg = _stick.GetComponent<Image>();

            SetVisible(_hideAfterInteraction == false);
            if (_behaviour == null) {
                SwitchBehaviour(_behaviourType);
            }

            _startPosition = _stick.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (_isInProgress || _isEnabled == false) {
                return;
            }


            _behaviour.OnTouchBegan(eventData.position);
            _pointerId = eventData.pointerId;
            _isInProgress = true;
            
            if (_hideAfterInteraction) {
                SetVisible(true);
            }
            
            OnInteractionChange.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (_isInProgress && eventData.pointerId == _pointerId) {
                _behaviour.OnTouchRelease();
                _isInProgress = false;

                if (_hideAfterInteraction) {
                    SetVisible(false);
                }
                else {
                    UpdatePositionsAnchored(_startPosition, _startPosition);
                }

                OnInteractionChange.Invoke(false);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (_isInProgress && eventData.pointerId == _pointerId) {
                _behaviour.OnTouchMove(eventData.position);
            }
        }

        public void SwitchBehaviour(JoystickBehaviours newBehaviourType) {
            switch (newBehaviourType) {
                case JoystickBehaviours.Fixed:
                    _behaviour = new FixedJoysticBehaviour(this);
                    break;
                case JoystickBehaviours.Dynamic:
                    _behaviour = new DynamicJoysticBehaviour(this);
                    break;
                default:
                    Debug.LogError("Switch behaviour: target behaviour wasn't found! Did you forget to add it in switch case?");
                    return;
            }
            _behaviourType = newBehaviourType;
        }

        public void AppearByScale(float duration, Ease ease) {
            _stickBackground.localScale = Vector3.zero;
            _stick.localScale = Vector3.zero;

            if (gameObject.activeSelf == false) gameObject.SetActive(true);

            var seq = Sequence.Create();
            seq.Group(Tween.Scale(_stickBackground, 1f, duration, ease))
               .Group(Tween.Scale(_stick, 1f, duration, ease));
        }

        /// <summary>
        /// Takes current resolution positions and changes joystic position
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="touchPos"></param>
        internal void UpdatePositions(Vector2 circleCenter, Vector3 touchPos) {
            _stickBackground.position = circleCenter;
            _stick.position = touchPos;
        }

        internal void UpdatePositionsAnchored(Vector2 circleCenter, Vector3 touchPos) {
            _stickBackground.anchoredPosition = circleCenter;
            _stick.anchoredPosition = touchPos;
        }

        private void OnApplicationFocus(bool focus) {
            if (focus == false && _isInProgress) {
                OnPointerUp(null);
            }
        }

        private void SetVisible(bool visible) {
            _stickBackgroundImg.enabled = visible;
            _stickImg.enabled = visible;
        }

        private void OnDisable() {
            if (_isInProgress) {
                _behaviour.OnTouchRelease();
                _isInProgress = false;

                UpdatePositionsAnchored(_startPosition, _startPosition);
            }
        }
    }
}