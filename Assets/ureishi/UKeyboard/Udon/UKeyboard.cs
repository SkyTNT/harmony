using System.Globalization;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace ureishi.UKeyboard.Udon
{
    public class UKeyboard : UdonSharpBehaviour
    {
        [HideInInspector]
        public readonly string VERSION = "UKeyboard v1.1.0.0";

        [SerializeField]
        private UdonSharpBehaviour[] _udonSharpBehaviours;
        [HideInInspector]
        public string text = string.Empty;

        [SerializeField]
        private Color _defaultColor = new Color(0x00, 0x60, 0xff, 0x00) / 0x100;
        [SerializeField]
        private Color _clickedColor = new Color(0x00, 0x60, 0xff, 0x80) / 0x100;

        [SerializeField]
        private Text _stringHolder;
        [SerializeField]
        private InputField _inputField;
        [SerializeField]
        private Transform _eventTriggersParent;
        [SerializeField]
        private Canvas[] _switchedCanvases;

        private Image[] _controlKeys;
        private GameObject _blocker;

        private bool _isAlt = false;
        private bool _isCapsLock = false;
        private bool _isCommand = false;
        private bool _isControl = false;
        private bool _isExchange = false;
        private bool _isKatakana = false;
        private bool _isShift = false;

        [SerializeField]
        private int _characterLimit = 42;
        [SerializeField]
        private float _autoOffTime = 300f;

        private bool _isInUse = false;
        private float _countTime = 0f;

        private const int _bufLength = 6;
        private string _buf = string.Empty;

        public void OnEndEdit()
        {
            Debug.Log("[" + nameof(UKeyboard) + "] text:\"" + text + "\"", this);
            // このメソッド内を変更する
            // 今回はパスワードシステム、一言伝言板の入力部分として

            // Relay Event OnEndEdit()
            foreach (var udonSharpBehaviour in _udonSharpBehaviours)
            {
                if (udonSharpBehaviour)
                    udonSharpBehaviour.SendCustomEvent(nameof(OnEndEdit));
                else
                    Debug.LogError($"[{nameof(UKeyboard)}] Target Udon Sharp Behaviour is null.", this);
            }
        }

        private void Start()
        {
            _inputField.characterLimit = _characterLimit;

            _controlKeys = new Image[] {
                _eventTriggersParent.Find("Alt" + "_L").GetComponent<Image>(),
                _eventTriggersParent.Find("Alt" + "_R").GetComponent<Image>(),
                _eventTriggersParent.Find("Command" + "_L").GetComponent<Image>(),
                _eventTriggersParent.Find("Command" + "_R").GetComponent<Image>(),
                _eventTriggersParent.Find("Control" + "_L").GetComponent<Image>(),
                _eventTriggersParent.Find("Control" + "_R").GetComponent<Image>(),
                _eventTriggersParent.Find("Shift" + "_L").GetComponent<Image>(),
                _eventTriggersParent.Find("Shift" + "_R").GetComponent<Image>()};

            _blocker = _eventTriggersParent.parent.Find("Blocker").gameObject;
        }

        private void Update()
        {
            if (!_isInUse) return;

            _countTime += Time.deltaTime;
            if (_countTime >= _autoOffTime)
            {
                // VRC_UiShapeは怖いのでCanvasで。
                foreach (var canvas in _switchedCanvases)
                {
                    canvas.GetComponent<BoxCollider>().enabled = false;
                }
                _eventTriggersParent.gameObject.SetActive(false);
                GetComponent<BoxCollider>().enabled = true;

                _isInUse = false;
            }
        }

        public override void Interact()
        {
            GetComponent<BoxCollider>().enabled = false;
            foreach (var canvas in _switchedCanvases)
            {
                canvas.GetComponent<BoxCollider>().enabled = true;
            }
            _blocker.SetActive(true);

            _countTime = 0f;
            _isInUse = true;
        }

        public void OnPointerDown()
        {
            string input = _stringHolder.text;
            _stringHolder.text = string.Empty;
            _eventTriggersParent.Find(input).GetComponent<Image>().color = _clickedColor;
        }

        public void OnPointerUp()
        {
            string input = _stringHolder.text;
            _stringHolder.text = string.Empty;
            _eventTriggersParent.Find(input).GetComponent<Image>().color = _defaultColor;
        }

        public void OnPointerDown_Enter()
        {
            _eventTriggersParent.Find("Enter" + "_A").GetComponent<Image>().color = _clickedColor;
            _eventTriggersParent.Find("Enter" + "_B").GetComponent<Image>().color = _clickedColor;
        }

        public void OnPointerUp_Enter()
        {
            _eventTriggersParent.Find("Enter" + "_A").GetComponent<Image>().color = _defaultColor;
            _eventTriggersParent.Find("Enter" + "_B").GetComponent<Image>().color = _defaultColor;
        }

        public void OnClick()
        {
            _countTime = 0f;

            string input = _stringHolder.text;
            _stringHolder.text = string.Empty;

            if (string.IsNullOrEmpty(input))
            {
                text = _inputField.text;
                _buf = string.Empty;
                return;
            }
            else if (input == "Alt")
            {
                _isAlt = !_isAlt;
                _eventTriggersParent.Find("Alt" + "_L").GetComponent<Image>().color = _isAlt ? _clickedColor : _defaultColor;
                _eventTriggersParent.Find("Alt" + "_R").GetComponent<Image>().color = _isAlt ? _clickedColor : _defaultColor;
            }
            else if (input == "Command")
            {
                _isCommand = !_isCommand;
                _eventTriggersParent.Find("Command" + "_L").GetComponent<Image>().color = _isCommand ? _clickedColor : _defaultColor;
                _eventTriggersParent.Find("Command" + "_R").GetComponent<Image>().color = _isCommand ? _clickedColor : _defaultColor;
            }
            else if (input == "Control")
            {
                _isControl = !_isControl;
                _eventTriggersParent.Find("Control" + "_L").GetComponent<Image>().color = _isControl ? _clickedColor : _defaultColor;
                _eventTriggersParent.Find("Control" + "_R").GetComponent<Image>().color = _isControl ? _clickedColor : _defaultColor;
            }
            else if (input == "Exchange")
            {
                _isExchange = !_isExchange;
                _eventTriggersParent.Find("Exchange").GetComponent<Image>().color = _isExchange ? _clickedColor : _defaultColor;
                _buf = string.Empty;
            }
            else if (input == "Katakana")
            {
                _isKatakana = !_isKatakana;
                _eventTriggersParent.Find("Katakana").GetComponent<Image>().color = _isKatakana ? _clickedColor : _defaultColor;
            }
            else if (input == "CapsLock")
            {
                _isCapsLock = !_isCapsLock;
                _eventTriggersParent.Find("CapsLock").GetComponent<Image>().color = _isCapsLock ? _clickedColor : _defaultColor;
            }
            else if (input == "Shift")
            {
                _isShift = !_isShift;
                _eventTriggersParent.Find("Shift" + "_L").GetComponent<Image>().color = _isShift ? _clickedColor : _defaultColor;
                _eventTriggersParent.Find("Shift" + "_R").GetComponent<Image>().color = _isShift ? _clickedColor : _defaultColor;
            }
            else
            {
                if (input == "Enter")
                {
                    OnEndEdit();
                    text = _buf = string.Empty;
                }
                else if (input == "BackSpace")
                {
                    if (_isControl)
                    {
                        text = _buf = string.Empty;
                    }
                    else
                    {
                        if (text.Length > 0)
                        {
                            text = text.Substring(0, text.Length - 1);
                        }

                        if (_buf.Length > 0)
                        {
                            _buf = _buf.Substring(0, _buf.Length - 1);
                        }
                    }
                }
                else if (input.Length == 1 && text.Length < _characterLimit)
                {
                    char chr = char.Parse(input);

                    if (_isShift ^ _isCapsLock)
                    {
                        switch (chr)
                        {
                            case '1': chr = '!'; break;
                            case '2': chr = '"'; break;
                            case '3': chr = '#'; break;
                            case '4': chr = '$'; break;
                            case '5': chr = '%'; break;
                            case '6': chr = '&'; break;
                            case '7': chr = '\''; break;
                            case '8': chr = '('; break;
                            case '9': chr = ')'; break;
                            case '0': chr = char.MinValue; break;
                            case '-': chr = '='; break;
                            case '^': chr = '~'; break;
                            case '@': chr = '`'; break;
                            case '[': chr = '{'; break;
                            case ';': chr = '+'; break;
                            case ':': chr = '*'; break;
                            case ']': chr = '}'; break;
                            case ',': chr = '<'; break;
                            case '.': chr = '>'; break;
                            case '/': chr = '?'; break;
                            default: chr = char.ToUpper(chr); break;
                        }
                    }
                    else
                    {
                        if (chr == '|' || chr == '_')
                        {
                            chr = '\\';
                        }
                    }

                    text += chr;

                    if (_isExchange && chr != char.MinValue)
                    {
                        _buf += chr;

                        string buf = _buf.PadLeft(_bufLength);
                        buf = buf.Substring(buf.Length - _bufLength);

                        string[] ss = new string[buf.Length];
                        for (int i = 0; i < buf.Length; i++)
                        {
                            // Udonの文字列インデクサ実装待ち
                            ss[i] = buf.Substring(buf.Length - (i + 1), 1);
                        }

                        bool maybeUnicode;
                        {
                            int at = buf.IndexOf("U+", 0, buf.Length);

                            maybeUnicode = at >= 0;
                            if (maybeUnicode)
                            {
                                for (int i = ss.Length - 1 - (at + 2); i >= 0; i--)
                                {
                                    // int.TryParse待ち
                                    maybeUnicode &= "0123456789ABCDEFabcdef".Contains(ss[i]);
                                }
                            }
                        }

                        string str = string.Empty;
                        int length = 0;

                        string sub = buf.Substring(buf.Length - 4);

                        if (maybeUnicode)
                        {
                            if (buf.StartsWith("U+"))
                            {
                                length = 6;
                                int value = int.Parse(sub, NumberStyles.HexNumber);
                                str = ((char)value).ToString();
                            }
                        }
                        else
                        {
                            {
                                if (ss[1] == "n" && !"'aeinouy".Contains(ss[0]))
                                {
                                    length = 2;
                                    str = "ん" + ss[0];
                                }
                            }

                            if (string.IsNullOrEmpty(str))
                            {
                                if (ss[1] == ss[0]
                                && "bcdfghjklmpqrstvwxyz".Contains(ss[0]))
                                {
                                    length = 2;
                                    str = "っ" + ss[0];
                                }
                            }

                            if (string.IsNullOrEmpty(str))
                            {
                                length = 4;
                                switch (sub)
                                {
                                    case "d'yu": str = "で" + "ゅ"; break;
                                    case "hwyu": str = "ふ" + "ゅ"; break;
                                    case "ltsu": str = "っ"; break;
                                    case "t'yu": str = "て" + "ゅ"; break;
                                    case "tcha": str = "っ" + "ち" + "ゃ"; break;
                                    case "tche": str = "っ" + "ち" + "ぇ"; break;
                                    case "tchi": str = "っ" + "ち"; break;
                                    case "tcho": str = "っ" + "ち" + "ょ"; break;
                                    case "tchu": str = "っ" + "ち" + "ゅ"; break;
                                    case "xtsu": str = "っ"; break;
                                    default: str = string.Empty; break;
                                }
                            }

                            if (string.IsNullOrEmpty(str))
                            {
                                length = 3;
                                switch (sub.Substring(1))
                                {
                                    case "bya": str = "び" + "ゃ"; break;
                                    case "bye": str = "び" + "ぇ"; break;
                                    case "byi": str = "び" + "ぃ"; break;
                                    case "byo": str = "び" + "ょ"; break;
                                    case "byu": str = "び" + "ゅ"; break;
                                    case "cha": str = "ち" + "ゃ"; break;
                                    case "che": str = "ち" + "ぇ"; break;
                                    case "chi": str = "ち"; break;
                                    case "cho": str = "ち" + "ょ"; break;
                                    case "chu": str = "ち" + "ゅ"; break;
                                    case "cya": str = "ち" + "ゃ"; break;
                                    case "cye": str = "ち" + "ぇ"; break;
                                    case "cyi": str = "ち" + "ぃ"; break;
                                    case "cyo": str = "ち" + "ょ"; break;
                                    case "cyu": str = "ち" + "ゅ"; break;
                                    case "dha": str = "で" + "ゃ"; break;
                                    case "dhe": str = "で" + "ぇ"; break;
                                    case "dhi": str = "で" + "ぃ"; break;
                                    case "dho": str = "で" + "ょ"; break;
                                    case "dhu": str = "で" + "ゅ"; break;
                                    case "d'i": str = "で" + "ぃ"; break;
                                    case "d'u": str = "ど" + "ぅ"; break;
                                    case "dwa": str = "ど" + "ぁ"; break;
                                    case "dwe": str = "ど" + "ぇ"; break;
                                    case "dwi": str = "ど" + "ぃ"; break;
                                    case "dwo": str = "ど" + "ぉ"; break;
                                    case "dwu": str = "ど" + "ぅ"; break;
                                    case "dya": str = "ぢ" + "ゃ"; break;
                                    case "dye": str = "ぢ" + "ぇ"; break;
                                    case "dyi": str = "ぢ" + "ぃ"; break;
                                    case "dyo": str = "ぢ" + "ょ"; break;
                                    case "dyu": str = "ぢ" + "ゅ"; break;
                                    case "fya": str = "ふ" + "ゃ"; break;
                                    case "fyo": str = "ふ" + "ょ"; break;
                                    case "fyu": str = "ふ" + "ゅ"; break;
                                    case "gwa": str = "ぐ" + "ぁ"; break;
                                    case "gwe": str = "ぐ" + "ぇ"; break;
                                    case "gwi": str = "ぐ" + "ぃ"; break;
                                    case "gwo": str = "ぐ" + "ぉ"; break;
                                    case "gwu": str = "ぐ" + "ぅ"; break;
                                    case "gya": str = "ぎ" + "ゃ"; break;
                                    case "gye": str = "ぎ" + "ぇ"; break;
                                    case "gyi": str = "ぎ" + "ぃ"; break;
                                    case "gyo": str = "ぎ" + "ょ"; break;
                                    case "gyu": str = "ぎ" + "ゅ"; break;
                                    case "hwa": str = "ふ" + "ぁ"; break;
                                    case "hwe": str = "ふ" + "ぇ"; break;
                                    case "hwi": str = "ふ" + "ぃ"; break;
                                    case "hwo": str = "ふ" + "ぉ"; break;
                                    case "hya": str = "ひ" + "ゃ"; break;
                                    case "hye": str = "ひ" + "ぇ"; break;
                                    case "hyi": str = "ひ" + "ぃ"; break;
                                    case "hyo": str = "ひ" + "ょ"; break;
                                    case "hyu": str = "ひ" + "ゅ"; break;
                                    case "jya": str = "じ" + "ゃ"; break;
                                    case "jye": str = "じ" + "ぇ"; break;
                                    case "jyi": str = "じ" + "ぃ"; break;
                                    case "jyo": str = "じ" + "ょ"; break;
                                    case "jyu": str = "じ" + "ゅ"; break;
                                    case "kwa": str = "く" + "ぁ"; break;
                                    case "kwe": str = "く" + "ぇ"; break;
                                    case "kwi": str = "く" + "ぃ"; break;
                                    case "kwo": str = "く" + "ぉ"; break;
                                    case "kwu": str = "く" + "ぅ"; break;
                                    case "kya": str = "き" + "ゃ"; break;
                                    case "kye": str = "き" + "ぇ"; break;
                                    case "kyi": str = "き" + "ぃ"; break;
                                    case "kyo": str = "き" + "ょ"; break;
                                    case "kyu": str = "き" + "ゅ"; break;
                                    case "lka": str = "ゕ"; break;
                                    case "lke": str = "ゖ"; break;
                                    case "ltu": str = "っ"; break;
                                    case "lwa": str = "ゎ"; break;
                                    case "lya": str = "ゃ"; break;
                                    case "lye": str = "ぇ"; break;
                                    case "lyi": str = "ぃ"; break;
                                    case "lyo": str = "ょ"; break;
                                    case "lyu": str = "ゅ"; break;
                                    case "mya": str = "み" + "ゃ"; break;
                                    case "mye": str = "み" + "ぇ"; break;
                                    case "myi": str = "み" + "ぃ"; break;
                                    case "myo": str = "み" + "ょ"; break;
                                    case "myu": str = "み" + "ゅ"; break;
                                    case "nya": str = "に" + "ゃ"; break;
                                    case "nye": str = "に" + "ぇ"; break;
                                    case "nyi": str = "に" + "ぃ"; break;
                                    case "nyo": str = "に" + "ょ"; break;
                                    case "nyu": str = "に" + "ゅ"; break;
                                    case "pya": str = "ぴ" + "ゃ"; break;
                                    case "pye": str = "ぴ" + "ぇ"; break;
                                    case "pyi": str = "ぴ" + "ぃ"; break;
                                    case "pyo": str = "ぴ" + "ょ"; break;
                                    case "pyu": str = "ぴ" + "ゅ"; break;
                                    case "rya": str = "り" + "ゃ"; break;
                                    case "rye": str = "り" + "ぇ"; break;
                                    case "ryi": str = "り" + "ぃ"; break;
                                    case "ryo": str = "り" + "ょ"; break;
                                    case "ryu": str = "り" + "ゅ"; break;
                                    case "sha": str = "し" + "ゃ"; break;
                                    case "she": str = "し" + "ぇ"; break;
                                    case "shi": str = "し"; break;
                                    case "sho": str = "し" + "ょ"; break;
                                    case "shu": str = "し" + "ゅ"; break;
                                    case "swa": str = "す" + "ぁ"; break;
                                    case "swe": str = "す" + "ぇ"; break;
                                    case "swi": str = "す" + "ぃ"; break;
                                    case "swo": str = "す" + "ぉ"; break;
                                    case "swu": str = "す" + "ぅ"; break;
                                    case "sya": str = "し" + "ゃ"; break;
                                    case "sye": str = "し" + "ぇ"; break;
                                    case "syi": str = "し" + "ぃ"; break;
                                    case "syo": str = "し" + "ょ"; break;
                                    case "syu": str = "し" + "ゅ"; break;
                                    case "tha": str = "て" + "ゃ"; break;
                                    case "the": str = "て" + "ぇ"; break;
                                    case "thi": str = "て" + "ぃ"; break;
                                    case "tho": str = "て" + "ょ"; break;
                                    case "thu": str = "て" + "ゅ"; break;
                                    case "t'i": str = "て" + "ぃ"; break;
                                    case "tsa": str = "つ" + "ぁ"; break;
                                    case "tse": str = "つ" + "ぇ"; break;
                                    case "tsi": str = "つ" + "ぃ"; break;
                                    case "tso": str = "つ" + "ぉ"; break;
                                    case "tsu": str = "つ"; break;
                                    case "t'u": str = "と" + "ぅ"; break;
                                    case "twa": str = "と" + "ぁ"; break;
                                    case "twe": str = "と" + "ぇ"; break;
                                    case "twi": str = "と" + "ぃ"; break;
                                    case "two": str = "と" + "ぉ"; break;
                                    case "twu": str = "と" + "ぅ"; break;
                                    case "tya": str = "ち" + "ゃ"; break;
                                    case "tye": str = "ち" + "ぇ"; break;
                                    case "tyi": str = "ち" + "ぃ"; break;
                                    case "tyo": str = "ち" + "ょ"; break;
                                    case "tyu": str = "ち" + "ゅ"; break;
                                    case "vya": str = "ゔ" + "ゃ"; break;
                                    case "vye": str = "ゔ" + "ぇ"; break;
                                    case "vyi": str = "ゔ" + "ぃ"; break;
                                    case "vyo": str = "ゔ" + "ょ"; break;
                                    case "vyu": str = "ゔ" + "ゅ"; break;
                                    case "wha": str = "う" + "ぁ"; break;
                                    case "whe": str = "う" + "ぇ"; break;
                                    case "whi": str = "う" + "ぃ"; break;
                                    case "who": str = "う" + "ぉ"; break;
                                    case "whu": str = "う"; break;
                                    case "wye": str = "ゑ"; break;
                                    case "wyi": str = "ゐ"; break;
                                    case "xka": str = "ゕ"; break;
                                    case "xke": str = "ゖ"; break;
                                    case "xtu": str = "っ"; break;
                                    case "xwa": str = "ゎ"; break;
                                    case "xya": str = "ゃ"; break;
                                    case "xye": str = "ぇ"; break;
                                    case "xyi": str = "ぃ"; break;
                                    case "xyo": str = "ょ"; break;
                                    case "xyu": str = "ゅ"; break;
                                    case "zwa": str = "ず" + "ぁ"; break;
                                    case "zwe": str = "ず" + "ぇ"; break;
                                    case "zwi": str = "ず" + "ぃ"; break;
                                    case "zwo": str = "ず" + "ぉ"; break;
                                    case "zwu": str = "ず" + "ぅ"; break;
                                    case "zya": str = "じ" + "ゃ"; break;
                                    case "zye": str = "じ" + "ぇ"; break;
                                    case "zyi": str = "じ" + "ぃ"; break;
                                    case "zyo": str = "じ" + "ょ"; break;
                                    case "zyu": str = "じ" + "ゅ"; break;
                                    default: str = string.Empty; break;
                                }
                            }

                            if (string.IsNullOrEmpty(str))
                            {
                                length = 2;
                                switch (sub.Substring(2))
                                {
                                    case "ba": str = "ば"; break;
                                    case "be": str = "べ"; break;
                                    case "bi": str = "び"; break;
                                    case "bo": str = "ぼ"; break;
                                    case "bu": str = "ぶ"; break;
                                    case "ca": str = "か"; break;
                                    case "ce": str = "せ"; break;
                                    case "ci": str = "し"; break;
                                    case "co": str = "こ"; break;
                                    case "cu": str = "く"; break;
                                    case "da": str = "だ"; break;
                                    case "de": str = "で"; break;
                                    case "di": str = "ぢ"; break;
                                    case "do": str = "ど"; break;
                                    case "du": str = "づ"; break;
                                    case "fa": str = "ふ" + "ぁ"; break;
                                    case "fe": str = "ふ" + "ぇ"; break;
                                    case "fi": str = "ふ" + "ぃ"; break;
                                    case "fo": str = "ふ" + "ぉ"; break;
                                    case "fu": str = "ふ"; break;
                                    case "ga": str = "が"; break;
                                    case "ge": str = "げ"; break;
                                    case "gi": str = "ぎ"; break;
                                    case "go": str = "ご"; break;
                                    case "gu": str = "ぐ"; break;
                                    case "ha": str = "は"; break;
                                    case "he": str = "へ"; break;
                                    case "hi": str = "ひ"; break;
                                    case "ho": str = "ほ"; break;
                                    case "hu": str = "ふ"; break;
                                    case "ja": str = "じ" + "ゃ"; break;
                                    case "je": str = "じ" + "ぇ"; break;
                                    case "ji": str = "じ"; break;
                                    case "jo": str = "じ" + "ょ"; break;
                                    case "ju": str = "じ" + "ゅ"; break;
                                    case "ka": str = "か"; break;
                                    case "ke": str = "け"; break;
                                    case "ki": str = "き"; break;
                                    case "ko": str = "こ"; break;
                                    case "ku": str = "く"; break;
                                    case "la": str = "ぁ"; break;
                                    case "le": str = "ぇ"; break;
                                    case "li": str = "ぃ"; break;
                                    case "lo": str = "ぉ"; break;
                                    case "lu": str = "ぅ"; break;
                                    case "ma": str = "ま"; break;
                                    case "me": str = "め"; break;
                                    case "mi": str = "み"; break;
                                    case "mo": str = "も"; break;
                                    case "mu": str = "む"; break;
                                    case "n'": str = "ん"; break;
                                    case "na": str = "な"; break;
                                    case "ne": str = "ね"; break;
                                    case "ni": str = "に"; break;
                                    case "nn": str = "ん"; break;
                                    case "no": str = "の"; break;
                                    case "nu": str = "ぬ"; break;
                                    case "pa": str = "ぱ"; break;
                                    case "pe": str = "ぺ"; break;
                                    case "pi": str = "ぴ"; break;
                                    case "po": str = "ぽ"; break;
                                    case "pu": str = "ぷ"; break;
                                    case "qa": str = "く" + "ぁ"; break;
                                    case "qe": str = "く" + "ぇ"; break;
                                    case "qi": str = "く" + "ぃ"; break;
                                    case "qo": str = "く" + "ぉ"; break;
                                    case "qu": str = "く"; break;
                                    case "ra": str = "ら"; break;
                                    case "re": str = "れ"; break;
                                    case "ri": str = "り"; break;
                                    case "ro": str = "ろ"; break;
                                    case "ru": str = "る"; break;
                                    case "sa": str = "さ"; break;
                                    case "se": str = "せ"; break;
                                    case "si": str = "し"; break;
                                    case "so": str = "そ"; break;
                                    case "su": str = "す"; break;
                                    case "ta": str = "た"; break;
                                    case "te": str = "て"; break;
                                    case "ti": str = "ち"; break;
                                    case "to": str = "と"; break;
                                    case "tu": str = "つ"; break;
                                    case "va": str = "ゔ" + "ぁ"; break;
                                    case "ve": str = "ゔ" + "ぇ"; break;
                                    case "vi": str = "ゔ" + "ぃ"; break;
                                    case "vo": str = "ゔ" + "ぉ"; break;
                                    case "vu": str = "ゔ"; break;
                                    case "wa": str = "わ"; break;
                                    case "we": str = "う" + "ぇ"; break;
                                    case "wi": str = "う" + "ぃ"; break;
                                    case "wo": str = "を"; break;
                                    case "wu": str = "う"; break;
                                    case "xa": str = "ぁ"; break;
                                    case "xe": str = "ぇ"; break;
                                    case "xi": str = "ぃ"; break;
                                    case "xn": str = "ん"; break;
                                    case "xo": str = "ぉ"; break;
                                    case "xu": str = "ぅ"; break;
                                    case "ya": str = "や"; break;
                                    case "ye": str = "い" + "ぇ"; break;
                                    case "yo": str = "よ"; break;
                                    case "yu": str = "ゆ"; break;
                                    case "z-": str = "〜"; break;
                                    case "z,": str = "‥"; break;
                                    case "z.": str = "…"; break;
                                    case "z/": str = "・"; break;
                                    case "z[": str = "『"; break;
                                    case "z]": str = "』"; break;
                                    case "za": str = "ざ"; break;
                                    case "ze": str = "ぜ"; break;
                                    case "zh": str = "←"; break;
                                    case "zi": str = "じ"; break;
                                    case "zj": str = "↓"; break;
                                    case "zk": str = "↑"; break;
                                    case "zl": str = "→"; break;
                                    case "zo": str = "ぞ"; break;
                                    case "zu": str = "ず"; break;
                                    default: str = string.Empty; break;
                                }
                            }

                            if (string.IsNullOrEmpty(str))
                            {
                                length = 1;
                                switch (sub.Substring(3))
                                {
                                    case ",": str = "、"; break;
                                    case "-": str = "ー"; break;
                                    case ".": str = "。"; break;
                                    case "[": str = "「"; break;
                                    case "]": str = "」"; break;
                                    case "{": str = "【"; break;
                                    case "}": str = "】"; break;
                                    case "~": str = "～"; break;
                                    case "a": str = "あ"; break;
                                    case "e": str = "え"; break;
                                    case "i": str = "い"; break;
                                    case "o": str = "お"; break;
                                    case "u": str = "う"; break;
                                    default: str = string.Empty; break;
                                }
                            }

                            if (_isKatakana)
                            {
                                char[] chars = str.ToCharArray();

                                for (int i = 0; i < chars.Length; i++)
                                {
                                    char c = chars[i];
                                    if (c >= 'ぁ' && c <= 'ゖ')
                                    {
                                        chars[i] = (char)(c + '`');
                                    }
                                    else if (c == '、')
                                    {
                                        chars[i] = '，';
                                    }
                                    else if (c == '。')
                                    {
                                        chars[i] = '．';
                                    }
                                }

                                str = new string(chars);
                            }
                        }

                        if (!string.IsNullOrEmpty(str))
                        {
                            text = text.Substring(0, text.Length - length) + str;
                            _buf = _buf.Substring(0, _buf.Length - length) + str;
                        }
                    }
                }

                _isAlt = _isControl = _isCommand = _isShift = false;
                foreach (var controlKey in _controlKeys)
                {
                    controlKey.color = _defaultColor;
                }
            }

            _inputField.text = text;
        }
    }
}
