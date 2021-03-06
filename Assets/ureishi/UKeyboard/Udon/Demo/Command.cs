using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace ureishi.UKeyboard.Udon.Demo
{
    // Keyboard.cs を使うデモです
    public class Command : UdonSharpBehaviour
    {
        [SerializeField]
        private UKeyboard _keyboard;
        [SerializeField]
        private UdonSharpBehaviour[] _udonSharpBehaviours;
        [HideInInspector]
        public string text = string.Empty;

        [SerializeField]
        private Text _uiText;

        [SerializeField]
        private ScrollRect _scrollRect;
        private int _frameCount = 0;

        public void OnEndEdit()
        {
            // このメソッド内を変更する
            text = _keyboard.text;

            if (text == "/version")
            {
                _uiText.text += $"\n{_keyboard.VERSION}";
            }
            else if (text == "/clear")
            {
                _uiText.text = string.Empty;
            }
            else if (text == "/date")
            {
                _uiText.text += $"\n{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            }
            else if (text.StartsWith("/size="))
            {
                // UdonSharpのインライン宣言の実装待ち
                int size;
                if (int.TryParse(_keyboard.text.Split('=')[1], out size))
                {
                    _uiText.fontSize = size;
                }
            }
            else
            {
                // Relay Event OnEndEdit()
                foreach (var udonSharpBehaviour in _udonSharpBehaviours)
                {
                    if (udonSharpBehaviour)
                        udonSharpBehaviour.SendCustomEvent(nameof(OnEndEdit));
                    else
                        Debug.LogWarning($"[{nameof(Command)}] Target Udon Sharp Behaviour is null.", this);
                }
            }

            _frameCount = 0;
        }

        private void LateUpdate()
        {
            if (_frameCount < 10 && ++_frameCount == 10)
            {
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }
    }
}
