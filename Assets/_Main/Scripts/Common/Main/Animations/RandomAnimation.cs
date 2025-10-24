using Alchemy.Inspector;
using UnityEngine;

/// <summary>
/// Class to random animation<br>
/// Usage - place this script on animator obj and set script properties</br>
/// </summary>
[AddComponentMenu("Common/Animation/Random Animation")]
[RequireComponent(typeof(Animator))]
public class RandomAnimation : MonoBehaviour
{
    private Animator _animator;

    [SerializeField, Tooltip("Write here all Animation names to random from Animator")]
    private string[] _animationNames;

    [SerializeField, Tooltip("Random animation start phase/time")]
    private bool _randomPhase = true;
    /// <summary>
    /// Start play normalized time of animation [0..1]
    /// </summary>
    [SerializeField, HideIf("randomPhase"), Range(0f, 1f)]
    private float _normilizedStartPlayTime = 0f;

    private void Start() {
        _animator = GetComponent<Animator>();
        _animator.enabled = true;
        if (_randomPhase) {
            _animator.Play(_animationNames[Random.Range(0, _animationNames.Length)], -1, Random.value);
        }
        else {
            _animator.Play(_animationNames[Random.Range(0, _animationNames.Length)], -1, _normilizedStartPlayTime);
        }
    }
}
