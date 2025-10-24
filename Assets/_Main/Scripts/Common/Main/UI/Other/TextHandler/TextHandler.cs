using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Game.UI {
    /// <summary>
    /// Handles text mesh pro or basic text<br>
    /// Can be use to create <b>typing</b> text animations</br>
    /// </summary>
	[AddComponentMenu("Common/UI/Text Handler")]
    public class TextHandler : MonoBehaviour {
        /// <summary>
        /// Is text change process currenly running
        /// </summary>
        public bool IsTextChanging => _textChangeCoroutine != null;
        /// <summary>
        /// Text value in Text component
        /// </summary>
        public string Text {
            get => _textBridge.text;
            set => SetText(value);
        }
        /// <summary>
        /// Interface from what you can get Graphic and convert it to TMP or Text
        /// </summary>
        public IText TextBridge => _textBridge;

        protected IText _textBridge;
        protected Coroutine _textChangeCoroutine;
        protected Action _textChangeCompletedCallback;
        protected bool _inited = false;

        /// <summary>
        /// Call it on before Text change if you want to change text in Awake
        /// </summary>
        protected virtual void Init() {
            if (TryGetComponent(out Text legacyText)) _textBridge = new LegacyTextBridge(legacyText);
            else if (TryGetComponent(out TMP_Text textMeshPro)) _textBridge = new TMPTextBridge(textMeshPro);
            else Debug.LogError("[TextValueUpdater] The is no text component attached to gameObject or it's not supported", gameObject);
            _inited = true;
        }

        protected virtual Coroutine GetPrintCoroutine(string text, float eachSymbolPrintDelay = 0f,
                                                      float firstSymbolPrintDelay = 0f) {
            return StartCoroutine(Printing(text, eachSymbolPrintDelay, firstSymbolPrintDelay));
        }

        protected virtual Coroutine GetRemoveCoroutine(float eachSymbolRemoveDelay = 0f, bool leftToRight = true,
                                                       float firstSymbolRemoveDelay = 0f) {
            return StartCoroutine(Removing(eachSymbolRemoveDelay, leftToRight, firstSymbolRemoveDelay));
        }

        /// <summary>
        /// Print/Add text to the end of existing content
        /// </summary>
        /// <param name="text">what we want to print (add to the end of text)</param>
        /// <param name="eachSymbolPrintDelay">delay (typing speed), less value - faster typing</param>
        /// <param name="firstSymbolPrintDelay">delay (before first symbol typed)</param>
        /// <param name="onCompleteCallback">callback that will be raise on text print complete</param>
        public TextHandler PrintText(string text, float eachSymbolPrintDelay = 0f,
                              float firstSymbolPrintDelay = 0f, Action onCompleteCallback = null) {
            if (_inited == false) Init();

            StopChangeCoroutine(raiseCompleteCallback: false);

            _textChangeCompletedCallback = onCompleteCallback;
            _textChangeCoroutine = GetPrintCoroutine(text, eachSymbolPrintDelay, firstSymbolPrintDelay);

            return this;
        }

        /// <summary>
        /// Change text simultaneously
        /// </summary>
        /// <param name="text">text to set</param>
        /// <param name="stopCurrentTextChange">[experimental] you can use false value, but text can behave unexpected</param>
        public virtual void SetText(string text, bool stopCurrentTextChange = true) {
            if (_inited == false) Init();
            if (stopCurrentTextChange) {
                StopChangeCoroutine(raiseCompleteCallback: false);
            }
            _textBridge.text = text;
        }

        public bool IsTextEmpty() {
            if (_inited == false) Init();
            return string.IsNullOrEmpty(_textBridge.text);
        }

        /// <summary>
        /// Stops current text change
        /// </summary>
        /// <param name="completeChange">raise text change complete callback</param>
        public void StopTextChange(bool completeChange) {
            if (_textChangeCoroutine != null) {
                if (completeChange) {
                    OnTextChangeEnd();
                }
            }
        }

        /// <param name="leftToRight">true -> text will be removed from left to right per symbol, and vice versa</param>
        /// <param name="eachSymbolRemoveDelay">less value -> faster text remove</param>
        /// <param name="firstSymbolRemoveDelay">delay (before first symbol removed)</param>
        /// <param name="onCompleteCallback">callback that will be raise on text remove complete</param>
        public TextHandler RemoveText(float eachSymbolRemoveDelay = 0f, bool leftToRight = true,
                               float firstSymbolRemoveDelay = 0f, Action onCompleteCallback = null) {
            if (_inited == false) Init();
            StopChangeCoroutine(raiseCompleteCallback: false);

            _textChangeCompletedCallback = null;
            AddCallbackCompleteTextSet(string.Empty);
            _textChangeCompletedCallback += onCompleteCallback;
            _textChangeCoroutine = GetRemoveCoroutine(eachSymbolRemoveDelay, leftToRight, firstSymbolRemoveDelay);

            return this;
        }

        protected void OnTextChangeEnd() {
            _textChangeCoroutine = null;
            _textChangeCompletedCallback?.Invoke();
        }

        protected void StopChangeCoroutine(bool raiseCompleteCallback) {
            if (_textChangeCoroutine != null) {
                StopCoroutine(_textChangeCoroutine);
                _textChangeCoroutine = null;

                if (raiseCompleteCallback) {
                    _textChangeCompletedCallback?.Invoke();
                }
            }
        }

        protected IEnumerator Printing(string text, float symbolPrintDelay, float firstSymbolPrintDelay = 0f) {
            AddCallbackCompleteTextSet(text);
            WaitForSeconds waitForSymbolDelay = new WaitForSeconds(symbolPrintDelay);
            if (firstSymbolPrintDelay > 0f) {
                yield return new WaitForSeconds(firstSymbolPrintDelay);
            }
            int textLen = text.Length;
            int startIndex = _textBridge.text.Length;
            for (int i = startIndex; i < textLen; i++) {
                _textBridge.text += text[i];
                yield return waitForSymbolDelay;
            }
            OnTextChangeEnd();
        }

        protected IEnumerator Removing(float symbolRemoveDelay, bool leftToRight = true, float firstSymbolPrintDelay = 0f) {
            WaitForSeconds waitForSymbolDelay = new WaitForSeconds(symbolRemoveDelay);
            if (firstSymbolPrintDelay > 0f) {
                yield return new WaitForSeconds(firstSymbolPrintDelay);
            }
            int textLen = _textBridge.text.Length - 1;
            if (leftToRight) {
                for (int i = 0; i < textLen; i++) {
                    _textBridge.text = _textBridge.text.Remove(i, 1).Insert(i, " ");
                    yield return waitForSymbolDelay;
                }
            }
            else {
                for (int i = 0; i < textLen; i++) {
                    _textBridge.text = _textBridge.text.Remove(_textBridge.text.Length - 1 - i, 1)
                                               .Insert(_textBridge.text.Length - 1 - i, " ");
                    yield return waitForSymbolDelay;
                }
            }

            OnTextChangeEnd();
        }

        /// <summary>
        /// What text should we apply on the end of change coroutine
        /// </summary>
        /// <param name="setText">text on coroutine end</param>
        protected void AddCallbackCompleteTextSet(string setText) {
            _textChangeCompletedCallback += () => {
                _textBridge.text = setText;
            };
        }
    }
}