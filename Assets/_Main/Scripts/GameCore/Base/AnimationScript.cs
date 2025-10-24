#if USE_UNITASK
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

using System;
using UnityEngine;

/// <summary>
/// All kind of animation states that you can trigger with <see cref="AnimationScript"/>
/// </summary>
public enum AnimationStates : byte {
    Idle01 = 0,
    Idle10 = 9,

    Walk01 = 10,
    Walk10 = 19,

    Run01 = 20,
    Run10 = 29,

    Death01 = 30,
    Death10 = 39,

    Die01 = 40,
    Die10 = 49,

    Attack01 = 50,
    Attack05 = 54,

    AttackRanged01 = 55,
    AttackRanged05 = 59,

    Special01 = 60,
    Special10 = 69,

    GotHit01 = 70,

    Aim01 = 80,

    Open01 = 100,
    Close01 = 110,
}

/// <summary>
/// Animator wrapper to handle animations. <br>
/// Has basic state switch logic by <see cref="AnimationStates"/> </br>
/// </summary>
[AddComponentMenu("Common/Animation/Animation Script")]
[RequireComponent(typeof(Animator))]
public class AnimationScript : MonoBehaviour {
    /// <summary>
    /// Each animation state should have it's own <see cref="AnimationStates"/> enum value condition <br>
    /// This property returns currently running <see cref="AnimationStates"/> in animator </br>
    /// </summary>
    public AnimationStates CurrentAnimState {
        get => _currentAnimState;
        set => SetAnimationState(value);
    }

    public bool UniTaskInProgress => _unitaskInProgress;

    protected Animator _animator;

    private AnimationStates _currentAnimState = AnimationStates.Idle01;
    private float _defaultAnimatorSpeed;

    private static readonly int AnimationStateIntegerID = Animator.StringToHash("AnimState");
    private static readonly int AnyStateTriggerID = Animator.StringToHash("AnyState");

    protected Action _eventCallback;
    private bool _unitaskInProgress;

    private void Awake() {
        _animator = GetComponent<Animator>();
        _defaultAnimatorSpeed = _animator.speed;
    }

    public void SetController(AnimatorOverrideController overrideController) {
        _animator.runtimeAnimatorController = overrideController;
    }

    /// <summary>
    /// Switches current animation state in animator by <paramref name="animationState"/>
    /// </summary>
    public void SetAnimationState(AnimationStates animationState) {
        _animator.SetInteger(AnimationStateIntegerID, (int)animationState);
        _animator.SetTrigger(AnyStateTriggerID);

        _currentAnimState = animationState;
    }

    public void SetFloat(string id, float value) {
        _animator.SetFloat(id, value);
    }

    public void SetBool(int id, bool active) {
        _animator.SetBool(id, active);
    }
    public bool GetBool(int id) {
        return _animator.GetBool(id);
    }

    public void PlayAnimation(string name) {
        _animator.Play(name);
    }

    public void PlayAnimation(string name, int layer, float normalizedTime) {
        _animator.Play(name, layer, normalizedTime);
    }

    public void PlayAnimationWithEvent(AnimationStates state, Action onHitEvent, bool executePreviousEvent = false) {
        SetOneShotEventCallback(onHitEvent, executePreviousEvent);
        SetAnimationState(state);
    }

#if USE_UNITASK
    /// <summary>
    /// This will trigger animation and await until event will be
    /// </summary>
    public async UniTask PlayAnimationWithEvent(AnimationStates state, CancellationToken cancellation = default, bool executePreviousEvent = false) {
        _unitaskInProgress = true;
        bool eventCalled = false;

        if (gameObject.activeSelf == false) {
            // prevent issue when calling this method but event never will be because animator is disabled with object
            return;
        }

        SetOneShotEventCallback(() => eventCalled = true, executePrevious: executePreviousEvent);
        SetAnimationState(state);

        await UniTask.WaitUntil(() => eventCalled, cancellationToken: cancellation);
        _unitaskInProgress = false;
    }
#endif

    public void SetAnimatorSpeed(float speed) {
        _animator.speed = speed;
    }

    public void SetAnimatorDefaultSpeed() {
        _animator.speed = _defaultAnimatorSpeed;
    }

    public virtual void ResetState(AnimationStates defaultState = AnimationStates.Idle01) {
        _currentAnimState = defaultState;
    }

    public void SetOneShotEventCallback(Action callback, bool executePrevious = false) {
        if (executePrevious && _eventCallback != null) _eventCallback.Invoke();
        _eventCallback = callback;
    }

    public void OnEventInvoke() { // invoke this in animation with event
        var callback = _eventCallback;
        _eventCallback = null;
        callback?.Invoke();
    }

    public float GetCurrentStateNormalizedTime() {
        return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
