using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ureishi.UKeyboard.Udon.Demo
{
    // Keyboard.cs を使うデモです
    // パスワードに該当しなければ、他のUdonのOnEndEditを呼び出します。
    public class PasswordEvent : UdonSharpBehaviour
    {
        [SerializeField]
        private UKeyboard _keyboard;
        [SerializeField]
        private UdonSharpBehaviour[] _udonSharpBehaviours;
        [HideInInspector]
        public string text = string.Empty;

        [SerializeField]
        private string _pass1, _pass2, _pass3;
        [SerializeField]
        private GameObject _go1, _go2, _go3, _go4;

        private VRCPlayerApi _localPlayer;

        public void OnEndEdit()
        {
            // このメソッド内を変更する
            text = _keyboard.text;

            if (text == _pass1)
            {
                _go1.SetActive(!_go1.activeSelf);
            }
            else if (text == _pass2)
            {
                _go2.SetActive(!_go2.activeSelf);
            }
            else if (text == _pass3)
            {
                _go3.SetActive(!_go3.activeSelf);
            }
            else if (text == _localPlayer.displayName)
            {
                _go4.SetActive(!_go4.activeSelf);
            }
            else
            {
                // Relay Event OnEndEdit()
                foreach (var udonSharpBehaviour in _udonSharpBehaviours)
                {
                    if (udonSharpBehaviour)
                        udonSharpBehaviour.SendCustomEvent(nameof(OnEndEdit));
                    else
                        Debug.LogWarning($"[{nameof(PasswordEvent)}] Target Udon Sharp Behaviour is null.", this);
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) _localPlayer = player;
        }
    }
}
