using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace ureishi.UKeyboard.Udon.Demo
{
    // Keyboard.cs を使うデモです
    // 伝言にパスワードやコマンドが表示されてしまうと困るので、
    // PasswordEvent.OnEndEdit, Command.OnEndEditを
    // 経由してChat.OnEndEditを呼ぶ。
    public class Chat : UdonSharpBehaviour
    {
        [SerializeField]
        private UKeyboard _keyboard;
        // [SerializeField]
        // private UdonSharpBehaviour[] _udonSharpBehaviours;
        [HideInInspector]
        public string text = string.Empty;

        [SerializeField]
        private Text _uiText;
        [SerializeField]
        private InputField _inputField;
        [SerializeField]
        private ScrollRect _scrollRect;

        [UdonSynced]
        private string _syncedString = string.Empty;

        private VRCPlayerApi _localPlayer;

        private int _frameCount = 0;

        public void OnEndEdit()
        {
            // このメソッド内を変更する

            if (string.IsNullOrEmpty(_keyboard.text))
            {
                return;
            }

            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(_localPlayer, gameObject);
            }

            text = $"{_localPlayer.displayName}\n{_keyboard.text}";
            if (text.Length > 42) text = text.Substring(0, 42); // 127byte

            WriteUiText(text);
            _inputField.text = _keyboard.text;
        }

        private void LateUpdate()
        {
            if (_frameCount < 10 && ++_frameCount == 10)
            {
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _localPlayer = player;
            }

            WriteUiText($"System\n[{DateTime.Now:HH:mm:ss}] {player.displayName} joined.");
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            WriteUiText($"System\n[{DateTime.Now:HH:mm:ss}] {player.displayName} left.");
        }

        public override void OnPreSerialization()
        {
            if (!string.IsNullOrEmpty(text))
            {
                _syncedString = text;
                text = string.Empty;
            }
        }

        public override void OnDeserialization()
        {
            if (!string.IsNullOrEmpty(_syncedString) && text != _syncedString)
            {
                text = _syncedString;
                WriteUiText(text);
                _inputField.text = text.Split('\n')[1];
            }
        }

        public void WriteUiText(string text)
        {
            if (_uiText.text.Length < 2048)
            {
                _uiText.text += "\n" + "@" + text;
            }
            else
            {
                _uiText.text = (_uiText.text + "\n" + "@" + text).Substring(2048);
            }
            _frameCount = 0;
        }
    }
}
