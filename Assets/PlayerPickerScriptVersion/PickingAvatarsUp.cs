using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/
/// If you have any issues, please contact me on Discord (Reimajo#1009) or Booth or Twitter https://twitter.com/ReimajoChan
/// </summary>
namespace ReimajoBoothAssets
{
    /// <summary>
    /// This script needs to be a single time somewhere in the world. It can be attached to a mesh with a collider to function as a button.
    /// You need to decide for one of those modes, you can't enable multiple at once:
    /// 
    /// ---------------------------------------------------------------------------------------
    /// MODE 1:
    /// When you only want to have a single person as a picker in your world,
    /// you enable "Restrict to one user" and provide the case-sensitive VRChat username.
    /// This way, you could e.g. be the only player in your world with this superpower.
    /// --------------------------------------------------------------------------------------- 
    /// MODE 2:
    /// In case you want to restrict this feature to multiple users in your world, you need to provide a list of usernames and enable "Restrict To White List".
    /// --------------------------------------------------------------------------------------- 
    /// MODE 3:
    /// When this feature should be usable by all players, set "Allow Anyone To Use This Feature".
    /// You may experience issues in the short timeframe when a new user is joining and not all players have registered this new player already.
    /// The script therefor has a one-second timeframe after a new player joined in which it won't accept any picks to minimize this issue.
    /// --------------------------------------------------------------------------------------- 
    /// 
    /// You can set "Auto Enable On Join" if this feature should be enabled by default, else the player needs to press the object first to enable it
    /// in which case it needs to have a collider attached that users can press.
    /// 
    /// A full guide can be found in the README file contained in the zip file from Booth.
    /// </summary>
    public class PickingAvatarsUp : UdonSharpBehaviour
    {
        #region SerializedFields
        //the following variables are declared as private to not mislead other scripts that they can safely modify them at runtime (which is not supported)
        [Space(10)]
        /// <summary> Allows every player in the instance to lift other players up </summary>
        [SerializeField, Tooltip("Allowing anyone to use this feature by clicking this object (requires a collider on this object)")]
        private bool _allowAnyoneToUseThisFeature = true;
        [Space(15)]
        /// <summary> Only the player defined in <see cref="_vrchatUsernameThatOneUser"/> can pick other players up </summary>
        [SerializeField, Tooltip("Restricting the usage to just the one user defined in the following field")]
        private bool _restrictToOneUser = false;
        /// <summary> Name of the only user that can pick other players up when <see cref="_restrictToOneUser"/> is enabled </summary>
        [SerializeField, Tooltip("VRChat username (case sensitive), only needed when 'restriction to one user' is selected")]
        private string _vrchatUsernameThatOneUser = "";
        [Space(15)]
        /// <summary> Restricts the usage to only users who are defined in <see cref="_whitelist"/> </summary>
        [SerializeField, Tooltip("Restricting the usage to any user in the whitelist")]
        private bool _restrictToWhitelist = false;
        /// <summary> List of usernames who can pick other players up when <see cref="_restrictToWhitelist"/> is enabled </summary>
        [SerializeField, Tooltip("List of (case-sensitive) VRChat usernames (required when 'restricted to whitelist' is selected)")]
        private string[] _whitelist;
        [Space(15)]
        /// <summary> Will enable the picker feature automaticly on join when player is allowed to use it </summary>
        [SerializeField, Tooltip("Feature is default-enabled when a player joins, they can still choose to disable it")]
        private bool _autoEnablePickerOnJoin = true;
        /// <summary> Will enable getting picked up automaticly on join </summary>
        [SerializeField, Tooltip("Allowing others to pick them up is default-enabled when a player joins")]
        private bool _autoAllowPickupOnJoin = true;
        /// <summary> User can choose if they want to allow others to pick them up </summary>
        [SerializeField, Tooltip("Allowing others to pick them up can be choosen by user")]
        private bool _allowPickupIsUserChoice = true;
        /// <summary> When enabled, player can escape from being picked up by moving </summary>
        [SerializeField, Tooltip("Allows a player who is being held to escape by moving")]
        private bool _allowPlayerToEscape = true;
        /// <summary> This will detect pressing grip button as picking up (not recommended!) instead of the (more reliable) trigger button
        /// The main issue is that on Index and WMR, this value is analog, so even the slightest press will trigger it,
        /// leading to accidental pickups when a user is just normally holding their controllers </summary>
        [SerializeField, Tooltip("This will detect pressing grip button as picking up (not recommended!) instead of (more reliable) trigger button")]
        private bool _useLessReliableGripButton = false;
        [Space(15)]
        /// <summary> Adds a ground object under the user to prevent falling animation (not recommended) </summary>
        [SerializeField, Tooltip("Adds a ground object under the user to prevent falling animation (not recommended)")]
        private bool _addFakeGround = false;
        /// <summary> Invisible plane with mesh collider set to layer 'Environment' (only needed when <see cref="_addFakeGround"/> is true) </summary>
        [SerializeField, Tooltip("Invisible plane with mesh collider set to layer 'Environment' (only needed when Add Fake Ground is selected)")]
        private GameObject _fakeGroundObj;
        /// <summary> Disables the mesh renderer of the object to which this script is attached if this feature is disabled for the current user </summary>
        [SerializeField, Tooltip("Disables the mesh renderer if this feature is disabled for the current user")]
        private bool _disableMeshRenderer = true;
        /// <summary> Multiplies the force vector at which player is being thrown when released, 3 is recommended </summary>
        [SerializeField, Tooltip("Multiplies the force vector at which player is being dropped (thrown)")]
        private float _yeetForceMultiplier = 3;
        /// <summary> Multiplies the force vector at which player is being thrown when released by their height difference (clamped to 0.2f and 2.5f) </summary>
        [SerializeField, Tooltip("Multiplies the force vector at which player is being dropped (thrown) by the height difference between picker and player")]
        private bool _yeetForcePlayerHeightDiff = true;
        /// <summary> (Optional) scripts that will receive 'PlayerIsHeld()' and 'PlayerIsDropped()' function calls. 
        /// Can be empty, this is optional in case external scripts need to have that information. </summary>
        [SerializeField, Tooltip("Scripts that will receive 'PlayerIsHeld()' and 'PlayerIsDropped()' function calls")]
        private UdonSharpBehaviour[] _optionalIsHeldReceiver;
        [Space(15)]
        /// <summary> Whether or not this object should change color when it's activated. If enabled, <see cref="_buttonMaterialIndex"/>
        /// and <see cref="_buttonMaterialEnabled"/> and <see cref="_buttonMaterialDisabled"/> must be assigned / specified in editor </summary>
        [SerializeField, Tooltip("Whether or not this object should change color when it's activated")]
        private bool _changeColor = true;
        /// <summary> (Optional) material that will be applied to the object where the script is attached to on material
        /// index <see cref="_buttonMaterialIndex"/> to set it into 'Enabled' state </summary>
        [SerializeField, Tooltip("(Optional) material that will be applied to this object to set it into 'Enabled' state")]
        private Material _buttonMaterialEnabled;
        /// <summary> (Optional) material that will be applied to the object where the script is attached to on material
        /// index <see cref="_buttonMaterialIndex"/> to set it into 'Disabled' state </summary>
        [SerializeField, Tooltip("(Optional) material that will be applied to this object to set it into 'Disabled' state")]
        private Material _buttonMaterialDisabled;
        /// <summary> Zero-based index of the button material if the object has multiple material slots, default is 0 for the first material slot </summary>
        [SerializeField, Tooltip("Zero-based index of the button material if the object has multiple material slots, default is 0")]
        private int _buttonMaterialIndex = 0;
        [Space(15)]
        /// <summary> Delay after which a previous picker event will be resend again if <see cref="_resendAmount"/> is bigger than 0 </summary>
        [SerializeField, Tooltip("Delay after which a previous picker event will be resend again if resendAmount is bigger than 0")]
        private float _resendDelay = 0.1f;
        /// <summary> Amount of times that a picker event will be resend again with <see cref="_resendDelay"/> for safety reasons.
        /// only needed when you experience that picked players are not always being dropped when a picker releases the button 
        /// which can happen in busy/complex worlds where network events are dropped. </summary>
        [SerializeField, Tooltip("Amount of times that a picker event will be resend again with Resend Delay for safety reasons. Only needed when you experience that picked players are not always being dropped when a picker releases the button which can happen in busy/complex worlds where network events are dropped.")]
        private int _resendAmount = 1;
        [Space(15)]
        /// <summary> Maximum distance from pick hand to any of their bones to pick them up. If 'scale to all players' is enabled, set this for an avatar of 1.5m height. </summary>
        [SerializeField, Tooltip("Maximum distance from pick hand to any bone to pick an avatar up (if 'scale to all players' is enabled, set this for an avatar of 1.5m height)")]
        private float _maxDistanceToAnyBone = 0.3f;
        /// <summary>
        /// Maximum distance from picker hand to localPlayer root to run a detailed check for that player, 5 is recommended
        /// </summary>
        [SerializeField, Tooltip("Maximum distance from picker hand to localPlayer root to run a detailed check for that player, 5 is recommended")]
        private float MAX_DISTANCE_TO_PLAYER = 5f;  //default value is 5f
        /// <summary> Measures all players within <see cref="MAX_DISTANCE_TO_PLAYER"/> to use accurate bone distance values </summary>
        [SerializeField, Tooltip("Measures all players within distance to use accurate bone distance values (at higher CPU frametime cost)")]
        private bool _scaleToAllPlayers = true;
        [Space(15)]
        /// <summary> When enabled, pickup events will be logged in private or debug-enabled worlds </summary>
        [SerializeField, Tooltip("When enabled, pickup events will be logged (recommended is 'disabled' for better performance). You can filter those logs with the tag '[LIFTUP]'.")]
        private bool _logEvents = false;
        #endregion SerializedFields
        #region PrivateFields
        /// <summary>
        /// Time in seconds after which the player height needs to be updated.
        /// An update will only occur when player height is needed in script.
        /// </summary>
        private const float PLAYER_HEIGHT_UPDATE_INTERVAL = 15;
        private const float GRIP_BUTTON_TRESHOLD = 0.6f; //grip needs a lower treshold like 0.6f
        private const float TRIGGER_BUTTON_TRESHOLD = 1f; //trigger should be fully pressed 1f
        private bool _isAllowedToUseFeature = false;
        private Transform _fakeGround;
        private bool _pickerFeatureEnabled = false;
        private bool _canBePickedUp = false;
        private string _pickButtonLeftString;
        private string _pickButtonRightString;
        private float _pickButtonMinValue;
        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi _pickerPlayer;
        private float _pickerPlayerHandDistance;
        private bool _userIsInVR;
        private bool _isButtonPressedLocalCopyL = false;
        private bool _isButtonPressedLocalCopyR = false;
        private bool _isButtonPressedNetworkCopyL = false;
        private bool _isButtonPressedNetworkCopyR = false;
        private bool _isOwnButtonPressedLocalCopyL = false;
        private bool _isOwnButtonPressedLocalCopyR = false;
        private bool _playerIsHeld = false;
        private bool _playerIsHeldByHandR;
        private HumanBodyBones _pickerHandBone;
        private float _startPickHeight;
        private Vector3 _startPickPos;
        private Vector3 _startPlayerPos;
        private float _startGroundHeight;
        private RaycastHit _hit;
        private const float _maxRaycastDistance = 15;
        private const int _layerMask = 0b00000000_00000000_00000000_00000001; //default layer 0 on which the ground colliders should be
        private Vector3 _positionOneFrameAgo;
        private Vector3 _positionTwoFramesAgo;
        private float _deltaTimeOneFrameAgo;
        private Renderer _buttonMaterialRenderer;
        /// <summary>
        /// fields for resending events
        /// </summary>
        private bool _lastWasHoldEventR;
        private bool _lastWasHoldEventL;
        private int _resendCounterR;
        private int _resendCounterL;
        private float _lastSentTimeR;
        private float _lastSentTimeL;
        /// <summary>
        /// height of localPlayer
        /// </summary>
        private float _playerHeight = 1;
        /// <summary>
        /// Value that is 1x for an avatar of 1.5m height and scales up / down with player height
        /// </summary>
        private float _localPlayerBoneDistance = 0.3f;
        private float _localPlayerPickHandDistance = 0.3f;
        /// <summary>
        /// Distance from hand bone to player that is for an avatar of 1.5m height and scales up / down with player height
        /// </summary>
        private const float PICKER_HAND_DISTANCE_DEFAULT = 0.165f;
        /// <summary>
        /// Distance from hand bone to player that is for an avatar of 1.5m height and scales up / down with player height
        /// </summary>
        private const float PICKER_FINGER_DISTANCE_DEFAULT = 0.075f;
        /// <summary>
        /// An array of all relevant player bones for picking an avatar up. Can be edited (bones added or removed) without any issues.
        /// </summary>
        readonly object[] _bodyBones = new object[] { HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.Spine,
            HumanBodyBones.Chest, HumanBodyBones.Neck, HumanBodyBones.Head, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand, HumanBodyBones.LeftToes, HumanBodyBones.RightToes, HumanBodyBones.LeftEye, HumanBodyBones.RightEye, HumanBodyBones.Jaw,
            HumanBodyBones.UpperChest };
        /// <summary>
        /// An array which always has the references to all players in the world. The array size represents the number of players in the world.
        /// This array is sorted from lowest to highest player ID on all clients
        /// </summary>
        private VRCPlayerApi[] _allPlayersSorted;
        /// <summary>
        /// Current amount of players in the instance
        /// </summary>
        private int _playerAmount;
        /// <summary>
        /// This determines the size of <see cref="_allPlayersSorted"/>. The maximum amount of players in a world instance is [world capacity * 2 + 1]
        /// </summary>
        private const int _maxNumberOfPlayers = 81;
        /// <summary>
        /// Index of localPlayer in <see cref="_allPlayersSorted"/>
        /// </summary>
        private int _localPlayerIndex = 0;
        /// <summary>
        /// Time at which the player list was updated last time
        /// </summary>
        private float _timeOfLastPlayerListUpdate;
        #endregion PrivateFields
        /// <summary>
        /// This is run once before the first Update Cycle, we use this for a one-time setup
        /// </summary>
        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            if (_localPlayer == null)
            {
                //this means we are in editor and this script shouldn't run, else it would just crash due to missing VRChat APIs.
                this.gameObject.SetActive(false);
                return;
            }
            //primary means left hand, secondary means right hand
            _pickButtonLeftString = _useLessReliableGripButton ? "Oculus_CrossPlatform_PrimaryHandTrigger" : "Oculus_CrossPlatform_PrimaryIndexTrigger";
            _pickButtonRightString = _useLessReliableGripButton ? "Oculus_CrossPlatform_SecondaryHandTrigger" : "Oculus_CrossPlatform_SecondaryIndexTrigger";
            _pickButtonMinValue = _useLessReliableGripButton ? GRIP_BUTTON_TRESHOLD : TRIGGER_BUTTON_TRESHOLD;
            _allPlayersSorted = new VRCPlayerApi[_maxNumberOfPlayers];
            if (_changeColor)
            {
                if (_buttonMaterialDisabled == null || _buttonMaterialEnabled == null)
                {
                    _changeColor = false;
                    Debug.LogError("[LIFTUP] Can't change color because materials are not assigned to the script");
                }
                else
                {
                    _buttonMaterialRenderer = GetComponent<Renderer>();
                    if (_buttonMaterialRenderer != null)
                    {
                        Material[] allMaterials = _buttonMaterialRenderer.materials;
                        if (allMaterials == null || allMaterials.Length - 1 < _buttonMaterialIndex)
                        {
                            _changeColor = false;
                            Debug.LogError("[LIFTUP] Can't change color because the number of materials doesn't match Button Material Index");
                        }
                        else
                        {
                            //default color is the deactivated state
                            SetButtonMaterial(_buttonMaterialDisabled);
                        }
                    }
                    else
                    {
                        _changeColor = false;
                        Debug.LogError("[LIFTUP] Can't change color because there is no Renderer component attached to this object");
                    }
                }
            }
            if (_addFakeGround)
            {
                if (_fakeGroundObj != null)
                {
                    _fakeGroundObj.SetActive(false);
                    _fakeGround = _fakeGroundObj.transform;
                }
                else
                {
                    Debug.LogError("[LIFTUP] No ground object defined, so fake ground can't be added.");
                    _addFakeGround = false;
                }
            }
            bool disableInteractions = false;
            bool applyStandardSettings = true;
            _userIsInVR = _localPlayer.IsUserInVR();
            if (!_userIsInVR)
            {
                //this feature can only be used by VR players
                _isAllowedToUseFeature = false; //unneeded assignement to make code more readable
            }
            else if (_restrictToOneUser)
            {
                //this script should only be used by one players so we remove the interactable components for everyone else
                if (_localPlayer.displayName == _vrchatUsernameThatOneUser.Trim())
                {
                    applyStandardSettings = false;
                    Networking.SetOwner(_localPlayer, this.gameObject);
                    _isAllowedToUseFeature = true;
                }
            }
            else if (_restrictToWhitelist)
            {
                //this script should only be used by everyone in the whitelist so we remove the interactable components for everyone else
                if (_whitelist == null || _whitelist.Length == 0)
                {
                    Debug.LogError("[LIFTUP] Restricted to whitelist is selected but no whitelist is provided");
                    disableInteractions = true;
                    applyStandardSettings = false;
                    _isAllowedToUseFeature = false; //unneeded assignement to make code more readable
                }
                else
                {
                    string userName = _localPlayer.displayName;
                    for (int i = 0; i < _whitelist.Length; i++)
                    {
                        if (_whitelist[i].Trim() == userName)
                        {
                            _isAllowedToUseFeature = true;
                            break;
                        }
                    }
                }
            }
            else if (_allowAnyoneToUseThisFeature)
            {
                _isAllowedToUseFeature = true;
            }
            //settings are applied by default unless for special users (see exceptions above)
            if (applyStandardSettings)
            {
                if (!_allowPickupIsUserChoice)
                    disableInteractions = true;
                if (_autoAllowPickupOnJoin)
                    _canBePickedUp = true;
            }
            //auto enable picker if user is allowed to use this feature
            if (_isAllowedToUseFeature && _autoEnablePickerOnJoin)
                SetLocalPlayerAsPicker(true);
            //disables the mesh collider so that Interact() can't be triggered anymore
            if (disableInteractions)
            {
                Collider[] colliderArray = this.gameObject.GetComponents<Collider>();
                if (colliderArray != null)
                    foreach (Collider collider in colliderArray) { collider.enabled = false; }
                MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
                //making the mesh invisible if that option is selected and the user can't use this feature
                if (_disableMeshRenderer && meshRenderer != null && !_pickerFeatureEnabled)
                    meshRenderer.enabled = false;
            }
            //when a user is not allowed to use this feature, the color displays if they can be picked up
            if (!_isAllowedToUseFeature && _changeColor && _canBePickedUp)
                SetButtonMaterial(_buttonMaterialEnabled);
        }
        /// <summary>
        /// When _pickerFeatureEnabled is true, this runs only for PLAYER_PICKER to check if the pick hand trigger is being pressed.
        /// Every user processes button changes from picker player.
        /// When _playerIsHeld is true, the localPlayer is moved here according to the picker player hand position.
        /// </summary>
        private void Update()
        {
            if (_pickerFeatureEnabled)
            {
                //check left trigger
                bool triggerPressedL = Input.GetAxisRaw(_pickButtonLeftString) >= _pickButtonMinValue;
                if (!_isOwnButtonPressedLocalCopyL && triggerPressedL)
                {
                    _isOwnButtonPressedLocalCopyL = true;
                    SendPickerEvent(isRightHand: false, isHoldEvent: true, isResendEvent: false);
                }
                else if (_isOwnButtonPressedLocalCopyL && !triggerPressedL)
                {
                    _isOwnButtonPressedLocalCopyL = false;
                    SendPickerEvent(isRightHand: false, isHoldEvent: false, isResendEvent: false);
                }
                //check right trigger
                bool triggerPressedR = Input.GetAxisRaw(_pickButtonRightString) >= _pickButtonMinValue;
                if (!_isOwnButtonPressedLocalCopyR && triggerPressedR)
                {
                    _isOwnButtonPressedLocalCopyR = true;
                    SendPickerEvent(isRightHand: true, isHoldEvent: true, isResendEvent: false);
                }
                else if (_isOwnButtonPressedLocalCopyR && !triggerPressedR)
                {
                    _isOwnButtonPressedLocalCopyR = false;
                    SendPickerEvent(isRightHand: true, isHoldEvent: false, isResendEvent: false);
                }
                //make sure to resend previous events
                if (_resendCounterR > 0 && Time.timeSinceLevelLoad - _lastSentTimeR > _resendDelay)
                {
                    SendPickerEvent(isRightHand: true, isHoldEvent: _lastWasHoldEventR, isResendEvent: true);
                    _resendCounterR--;
                }
                if (_resendCounterL > 0 && Time.timeSinceLevelLoad - _lastSentTimeL > _resendDelay)
                {
                    SendPickerEvent(isRightHand: false, isHoldEvent: _lastWasHoldEventL, isResendEvent: true);
                    _resendCounterL--;
                }
                //there is nothing else to do for the PLAYER_PICKER if the feature is restricted to one user since they can't be picked up
                if (_restrictToOneUser)
                    return;
            }
            //Every user must process changes on the button states from PLAYER_PICKER at any given time
            if (_canBePickedUp)
            {
                ProcessButtonChangesFromPicker();
                //when the player is being held, we need to update localPlayer position in every frame
                if (_playerIsHeld)
                {
                    //when this option is enabled, the player is released when they start to move using WASD or VR joystick input
                    if (_allowPlayerToEscape && CheckIfPlayerWantsToEscape())
                    {
                        SetPlayerIsHeldFalse();
                        //we need to reset all picker variables so that new pick events can go through
                        _isButtonPressedNetworkCopyR = false;
                        _isButtonPressedNetworkCopyL = false;
                        _isButtonPressedLocalCopyR = false;
                        _isButtonPressedLocalCopyL = false;
                        return;
                    }
                    //we need to move localPlayer with the hand onto which we are attached to
                    Vector3 pickHandPos = _pickerPlayer.GetBonePosition(_pickerHandBone);
                    Vector3 pickPosDiff = pickHandPos - _startPickPos;
                    Vector3 targetPos = _startPlayerPos + pickPosDiff;
                    _localPlayer.SetVelocity((targetPos - _localPlayer.GetPosition()) / Time.deltaTime);
                    //storing velocity
                    _positionTwoFramesAgo = _positionOneFrameAgo;
                    _positionOneFrameAgo = targetPos;
                    _deltaTimeOneFrameAgo = Time.deltaTime;
                    //adding a fake ground to prevent player falling animation
                    if (_addFakeGround)
                    {
                        float pickHeightDiff = pickHandPos.y - _startPickHeight;
                        float newGroundHeight = _startGroundHeight + pickHeightDiff;
                        Vector3 newGroundPos = _localPlayer.GetBonePosition(HumanBodyBones.Head);
                        newGroundPos.y = newGroundHeight;
                        _fakeGround.position = newGroundPos;
                    }
                }
            }
        }
        /// <summary>
        /// External and internal function to set localplayer to be the current picker player with <see cref="setClaimed"/> true.
        /// </summary>
        private void SetLocalPlayerAsPicker(bool setClaimed)
        {
            if (setClaimed)
            {
                _pickerFeatureEnabled = true;
                if (_changeColor)
                    SetButtonMaterial(_buttonMaterialEnabled);
            }
            else
            {
                _pickerFeatureEnabled = false;
                if (_changeColor)
                    SetButtonMaterial(_buttonMaterialDisabled);
            }
        }
        /// <summary>
        /// This script runs for everyone except PLAYER_PICKER to handle the button states
        /// </summary>
        private void ProcessButtonChangesFromPicker()
        {
            if (!_playerIsHeld || _playerIsHeldByHandR)
            {
                if (_isButtonPressedNetworkCopyR && !_isButtonPressedLocalCopyR)
                {
                    _isButtonPressedLocalCopyR = true;
                    HumanBodyBones pickerHandBone = GetBestHandBone(_pickerPlayer, isRightHand: true);
                    //button was pressed by master, time to calculate their hand position
                    if (CheckIfPlayerIsHeld(_localPlayer, _pickerPlayer.GetBonePosition(pickerHandBone), receiverIsLocalPlayer: true, _pickerPlayerHandDistance))
                    {
                        SetPlayerIsHeld(isRightHand: true, pickerHandBone);
                    }
                }
                else if (!_isButtonPressedNetworkCopyR && _isButtonPressedLocalCopyR)
                {
                    //button was released by master (PLAYER_PICKER stopped picking me up)
                    _isButtonPressedLocalCopyR = false;
                    if (_playerIsHeld)
                    {
                        PickerThrowsLocalPlayer();
                        SetPlayerIsHeldFalse();
                    }
                }
            }
            //the right hand is dominant, so left hand can't hold it as well at the same time
            if (!_playerIsHeld || !_playerIsHeldByHandR)
            {
                if (_isButtonPressedNetworkCopyL && !_isButtonPressedLocalCopyL)
                {
                    _isButtonPressedLocalCopyL = true;
                    HumanBodyBones pickerHandBone = GetBestHandBone(_pickerPlayer, isRightHand: false);
                    //button was pressed by master, time to calculate their hand position
                    if (CheckIfPlayerIsHeld(_localPlayer, _pickerPlayer.GetBonePosition(pickerHandBone), receiverIsLocalPlayer: true, _pickerPlayerHandDistance))
                    {
                        SetPlayerIsHeld(isRightHand: false, pickerHandBone);
                    }
                }
                else if (!_isButtonPressedNetworkCopyL && _isButtonPressedLocalCopyL)
                {
                    //button was released by master (PLAYER_PICKER stopped picking me up)
                    _isButtonPressedLocalCopyL = false;
                    if (_playerIsHeld)
                    {
                        PickerThrowsLocalPlayer();
                        SetPlayerIsHeldFalse();
                    }
                }
            }
        }
        /// <summary>
        /// Adds the current velocity of the picker hand to localPlayer 
        /// </summary>
        private void PickerThrowsLocalPlayer()
        {
            //a bigger player should throw a smaller one further, but we should clamp this to reasonable values to avoid yeets into space
            float playerThrowWeightFactor = _yeetForcePlayerHeightDiff ? Mathf.Clamp(MeasurePlayerHeight(_pickerPlayer) / _playerHeight, 0.1f, 2.5f) : 1;
            _localPlayer.SetVelocity(((_positionOneFrameAgo - _positionTwoFramesAgo) / _deltaTimeOneFrameAgo) * _yeetForceMultiplier * playerThrowWeightFactor);

        }
        /// <summary>
        /// If the pick hand is close enough to one bone of localplayer, this player is being held from there on
        /// </summary>
        private bool CheckIfPlayerIsHeld(VRCPlayerApi pickedPlayer, Vector3 pickHandPos, bool receiverIsLocalPlayer, float pickerPlayerHandDistance)
        {
            //player too far away to be picked
            if (Vector3.Distance(pickHandPos, pickedPlayer.GetPosition()) > MAX_DISTANCE_TO_PLAYER)
                return false;
            float maxBoneDistance;
            if (receiverIsLocalPlayer)
            {
                UpdateLocalPlayerHeightFactor();
                if (_logEvents)
                    Debug.Log($"[LIFTUP] LocalPlayer bone distance was updated to {_localPlayerBoneDistance} before checking distance.");
                maxBoneDistance = _localPlayerBoneDistance + pickerPlayerHandDistance;
            }
            else
            {
                if (_scaleToAllPlayers)
                    maxBoneDistance = ((_maxDistanceToAnyBone / 1.5f) * MeasurePlayerHeight(pickedPlayer)) + pickerPlayerHandDistance;
                else
                    maxBoneDistance = (_maxDistanceToAnyBone * 2) + pickerPlayerHandDistance; //this will allow players up to 3m height to still receive the event to check for themselves
            }
            //player in reach, need to check all bones
            for (int i = 0; i < _bodyBones.Length; i++)
            {
                if (Vector3.Distance(pickHandPos, pickedPlayer.GetBonePosition((HumanBodyBones)_bodyBones[i])) <= maxBoneDistance)
                {
                    if (_logEvents)
                        Debug.Log($"[LIFTUP] CheckIfPlayerIsHeld: True (in distance)");
                    return true; //one bone is enough
                }
            }
            //PLAYER_PICKER hand not in max bone distance
            if (_logEvents)
                Debug.Log($"[LIFTUP] CheckIfPlayerIsHeld: Not in distance.");
            return false;
        }
        /// <summary>
        /// Checks if any player bone is within reach to grab them
        /// </summary>
        private bool CheckIfAnyPlayerIsInReach(bool isRightHand)
        {
            //we need to update _localPlayerPickHandDistance
            UpdateLocalPlayerHeightFactor();
            HumanBodyBones pickHandBone = GetBestHandBone(_localPlayer, isRightHand);
            Vector3 pickHandPos = _localPlayer.GetBonePosition(pickHandBone);
            //we check for all players in the list if they are withing MAX_DISTANCE_TO_PLAYER
            VRCPlayerApi player;
            for (int i = 0; i < _playerAmount; i++)
            {
                player = _allPlayersSorted[i];
                if (player == null)
                    continue;
                if (i == _localPlayerIndex)
                    continue;
                if (Vector3.Distance(player.GetPosition(), pickHandPos) < MAX_DISTANCE_TO_PLAYER)
                {
                    //for players in distance, we check if the pick hand is withing reach to their bones
                    if (CheckIfPlayerIsHeld(player, pickHandPos, receiverIsLocalPlayer: false, _localPlayerPickHandDistance))
                        return true;
                    //one player bone in reach is enough
                }
            }
            //we are not within reach to any player bone so we don't have any player to grab
            return false;
        }
        /// <summary>
        /// Sets the localPlayer to be held by the picker player with their specified hand
        /// and stores data which is later used to move localPlayer with that pickerPlayer hand
        /// </summary>
        private void SetPlayerIsHeld(bool isRightHand, HumanBodyBones pickerHandBone)
        {
            if (_optionalIsHeldReceiver != null)
                for (int i = 0; i < _optionalIsHeldReceiver.Length; i++) { _optionalIsHeldReceiver[i].SendCustomEvent("PlayerIsHeld"); }
            _playerIsHeld = true;
            _playerIsHeldByHandR = isRightHand;
            _pickerHandBone = pickerHandBone;
            _startPickHeight = _pickerPlayer.GetBonePosition(_pickerHandBone).y;
            _startPickPos = _pickerPlayer.GetBonePosition(_pickerHandBone);
            _startPlayerPos = _localPlayer.GetPosition();
            if (_addFakeGround)
            {
                _startGroundHeight = GetGroundHeight();
                Vector3 newGroundPos = _localPlayer.GetBonePosition(HumanBodyBones.Head);
                newGroundPos.y = _startGroundHeight;
                _fakeGround.position = newGroundPos;
                _fakeGroundObj.SetActive(true);
            }
        }
        /// <summary>
        /// Releases the player from being held
        /// </summary>
        private void SetPlayerIsHeldFalse()
        {
            _playerIsHeld = false;
            if (_optionalIsHeldReceiver != null)
                for (int i = 0; i < _optionalIsHeldReceiver.Length; i++) { _optionalIsHeldReceiver[i].SendCustomEvent("PlayerIsDropped"); }
            if (_addFakeGround)
                _fakeGroundObj.SetActive(false);
        }
        /// <summary>
        /// Checking if the player tries to move away while being held
        /// </summary>
        private bool CheckIfPlayerWantsToEscape()
        {
            if (_userIsInVR)
            {
                //using the left thumbstick to move is recognized as trying to escape (there is no better way as of now, user rebinding would break this)
                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.7f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.7f)
                    return true;
            }
            else
            {
                //pressing any keyboard walking key is recognizes as trying to escape (there is no better way as of now, user rebinding would break this)
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    return true;
            }
            return false;
        }
        #region RPC_RECEIVER_ONE_USER
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is pressed
        /// </summary>
        public void ButtonPressedL()
        {
            if (_pickerFeatureEnabled)
                return;
            //PLAYER_PICKER pressed left button
            _isButtonPressedNetworkCopyL = true;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is released
        /// </summary>
        public void ButtonReleasedL()
        {
            if (_pickerFeatureEnabled)
                return;
            //PLAYER_PICKER released left button
            _isButtonPressedNetworkCopyL = false;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is pressed
        /// </summary>
        public void ButtonPressedR()
        {
            if (_pickerFeatureEnabled)
                return;
            //PLAYER_PICKER pressed right button
            _isButtonPressedNetworkCopyR = true;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is released
        /// </summary>
        public void ButtonReleasedR()
        {
            if (_pickerFeatureEnabled)
                return;
            //PLAYER_PICKER released right button
            _isButtonPressedNetworkCopyR = false;
        }
        #endregion RPC_RECEIVER_ONE_USER
        /// <summary>
        /// Calculating the ground height under the head of localPlayer
        /// Returns 0f when there is no ground. Only non-trigger colliders on the default layer are recognized as ground.
        /// </summary>
        private float GetGroundHeight()
        {
            Vector3 playerHeadPos = _localPlayer.GetBonePosition(HumanBodyBones.Head);
            if (Physics.Raycast(playerHeadPos, Vector3.down, out _hit, _maxRaycastDistance, _layerMask))
                return _hit.point.y;
            else
                return 0f;
        }
        /// <summary>
        /// When the script is attached to an object with a collider, the player can click on it and this will trigger this function
        /// which enables/disables both picker function and wheter or not they can be picked up by other players.
        /// </summary>
        /*public override void Interact()
        {
            if (!_isAllowedToUseFeature)
            {
                if (_allowPickupIsUserChoice)
                {
                    _canBePickedUp = !_canBePickedUp;
                    if (_changeColor)
                    {
                        if (_canBePickedUp)
                            SetButtonMaterial(_buttonMaterialEnabled);
                        else
                            SetButtonMaterial(_buttonMaterialDisabled);
                    }
                }
            }
            else
            {
                _canBePickedUp = !_pickerFeatureEnabled;
                //this will also set the button material
                SetLocalPlayerAsPicker(!_pickerFeatureEnabled);
            }
        }*/

        public void setCanBePickUp(bool val)
        {
            _canBePickedUp = val;
            Debug.Log("[LIFTUP] Can be lift up :"+ _canBePickedUp);
        }


        /// <summary>
        /// Changing the material of the button to <see cref="newMaterial"/>
        /// </summary>
        private void SetButtonMaterial(Material newMaterial)
        {
            Material[] materials = _buttonMaterialRenderer.materials;
            materials[_buttonMaterialIndex] = newMaterial;
            _buttonMaterialRenderer.materials = materials;
        }
        /// <summary>
        /// Returns the best available hand bone for a player API
        /// </summary>
        private HumanBodyBones GetBestHandBone(VRCPlayerApi player, bool isRightHand)
        {
            if (isRightHand)
                return player.GetBonePosition(HumanBodyBones.RightMiddleProximal) != Vector3.zero ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.RightHand;
            else
                return player.GetBonePosition(HumanBodyBones.LeftMiddleProximal) != Vector3.zero ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.LeftHand;

        }
        #region SortedPlayerList
        /// <summary>
        /// To avoid expensive calls to <see cref="VRCPlayerApi.GetPlayers"/> we keep an updated array of <see cref="VRCPlayerApi"/> ourselves.
        /// When a player joins, we add that player to our array. We sort this array from lowest to highest player ID.
        /// Since player IDs are in sync on all clients, we now have an artificial player ID from 0-80 on all clients which is in sync.
        /// This allows to have a small RPC pool (see below) that we can use. Those artificial player IDs are not in sync when a 
        /// player joins or leaves for around a second, which is why RPCs in that timeframe are filtered.
        /// </summary>
        /// <param name="newPlayer">The player that joined</param>
        public override void OnPlayerJoined(VRCPlayerApi newPlayer)
        {
            //if the script is restricted to one user, a simplified version can run and no user list is needed
            if (_restrictToOneUser)
            {
                if (newPlayer.displayName == _vrchatUsernameThatOneUser.Trim())
                    _pickerPlayer = newPlayer;
                return;
            }
            _timeOfLastPlayerListUpdate = Time.timeSinceLevelLoad;
            if (_playerAmount == _maxNumberOfPlayers)
            {
                Debug.LogError($"[LIFTUP] ERROR: Player joined but maximum number of players ({_maxNumberOfPlayers}) was already reached.");
                return;
            }
            //first player just gets added on first spot since the following logic can't handle an empty player array
            if (_playerAmount == 0)
            {
                _allPlayersSorted.SetValue(newPlayer, 0);
                _playerAmount++;
                if (newPlayer == _localPlayer)
                    _localPlayerIndex = 0;
            }
            else
            {
                //we need to find a player that has a lower player ID to put this player one spot above and shift everyone else one place up
                //iterating from first empty spot downwards, there is always at least one player downwards (see above)
                for (int i = _playerAmount; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        _allPlayersSorted.SetValue(newPlayer, i);
                        _playerAmount++;
                        if (newPlayer == _localPlayer)
                            _localPlayerIndex = i;
                    }
                    else if (_allPlayersSorted[i - 1].playerId < newPlayer.playerId)
                    {
                        _allPlayersSorted.SetValue(newPlayer, i);
                        _playerAmount++;
                        if (newPlayer == _localPlayer)
                            _localPlayerIndex = i;
                        break;
                    }
                    else
                    {
                        _allPlayersSorted.SetValue(_allPlayersSorted.GetValue(i - 1), i);
                        if (_allPlayersSorted.GetValue(i - 1) == _localPlayer)
                            _localPlayerIndex = i;
                    }
                }
            }
            if (_logEvents)
            {
                Debug.Log($"[LIFTUP] Player {newPlayer.displayName} with ID {newPlayer.playerId} joined, localPlayer is Index {_localPlayerIndex} now.");
                Debug.Log("[LIFTUP] Full player list is now: ------------------------");
                for (int i = 0; i < _playerAmount; i++) { Debug.Log($"[LIFTUP] Player [{i}]: ID {_allPlayersSorted[i].playerId}, name '{ _allPlayersSorted[i].displayName}'"); }
                Debug.Log("[LIFTUP] -------------------------------------------------");
            }
        }
        /// <summary>
        /// To minimize calls to <see cref="VRCPlayerApi.GetPlayers"/> we keep an updated array of <see cref="VRCPlayerApi"/>
        /// When a player left, we search this player in our array, remove it and shift every other player one entry down in the list
        /// </summary>
        /// <param name="leftPlayer">The player that left</param>
        public override void OnPlayerLeft(VRCPlayerApi leftPlayer)
        {
            //if the script is restricted to one user, a simnplified version can run and no user list is needed
            if (_restrictToOneUser)
            {
                if (leftPlayer.displayName == _vrchatUsernameThatOneUser.Trim())
                {
                    _pickerPlayer = null;
                    SetPlayerIsHeldFalse();
                }
                return;
            }
            _timeOfLastPlayerListUpdate = Time.timeSinceLevelLoad;
            // This script should stop to run when the PLAYER_PICKER left this instance
            if (leftPlayer == _pickerPlayer)
            {
                //PLAYER_PICKER left the instance
                _pickerPlayer = null;
                SetPlayerIsHeldFalse();
            }
            bool playerFound = false;
            for (int i = 0; i < _playerAmount; i++)
            {
                if (!playerFound)
                {
                    if (_allPlayersSorted[i] == leftPlayer)
                    {
                        playerFound = true;
                        continue;
                    }
                }
                else
                {
                    _allPlayersSorted.SetValue(_allPlayersSorted.GetValue(i), i - 1);
                    if (_allPlayersSorted.GetValue(i) == _localPlayer)
                        _localPlayerIndex = i - 1;
                }
            }
            if (playerFound)
            {
                _allPlayersSorted.SetValue(null, _playerAmount - 1);
                _playerAmount--;
            }
            else
            {
                Debug.LogError("[LIFTUP] ERROR: Couldn't find the player that left");
            }
            if (_logEvents)
            {
                Debug.Log($"[LIFTUP] Player {leftPlayer.displayName} with ID {leftPlayer.playerId} left, localPlayer is Index {_localPlayerIndex} now.");
                Debug.Log("[LIFTUP] Full player list is now: ------------------------");
                for (int i = 0; i < _playerAmount; i++) { Debug.Log($"[LIFTUP] Player [{i}]: ID {_allPlayersSorted[i].playerId}, name '{ _allPlayersSorted[i].displayName}'"); }
                Debug.Log("[LIFTUP] -------------------------------------------------");
            }
        }
        #endregion SortedPlayerList
        /// <summary>
        /// Since we can't send variables over network, we need to use RPCs for that (this might change in the future)
        /// I explained how this works in my article here: https://ask.vrchat.com/t/how-to-send-variables-over-network-udonsharp/2282
        /// </summary>
        /// <param name="isRightHand">true if it's the right hand, else false for the left hand</param>
        /// <param name="isHoldEvent">Wheter it's the hold or release event</param>
        /// <param name="isResendEvent">Wheter it's a resend event or the first original event</param>
        private void SendPickerEvent(bool isRightHand, bool isHoldEvent, bool isResendEvent)
        {
            if (!isResendEvent)
            {
                //a hold event should only be sent when a player bone from someone else is in reach to avoid unnecessary CPU frametime cost on all other clients
                if (isHoldEvent && !CheckIfAnyPlayerIsInReach(isRightHand))
                    return;
                if (isRightHand)
                {
                    //don't send release event when no hold event was sent out before
                    if (!isHoldEvent && !_lastWasHoldEventR)
                        return;
                    //remember to send the corresponding release event next time
                    _lastWasHoldEventR = isHoldEvent;
                    //make sure this event gets resend the specified amount of times
                    _resendCounterR = _resendAmount;
                }
                else
                {
                    //same logic, but for the left hand
                    if (!isHoldEvent && !_lastWasHoldEventL)
                        return;
                    _lastWasHoldEventL = isHoldEvent;
                    _resendCounterL = _resendAmount;
                }

            }
            //store time of last network event sent by localPlayer (for the resent events)
            if (isRightHand)
                _lastSentTimeR = Time.timeSinceLevelLoad;
            else
                _lastSentTimeL = Time.timeSinceLevelLoad;
            //when restricted to one user, we use network ownership to safely transfer the events and we don't need to filter them
            if (_restrictToOneUser)
            {
                //events for one user are unfiltered since they should not bottleneck the network, this is also more secure
                string functionName = isHoldEvent ? (isRightHand ? nameof(ButtonPressedR) : nameof(ButtonPressedL)) : (isRightHand ? nameof(ButtonReleasedR) : nameof(ButtonReleasedL));
                if (_logEvents)
                    Debug.Log($"[LIFTUP] Sending OneUser event {functionName} with playerIndex {_localPlayerIndex}");
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, functionName);
            }
            else
            {
                //don't allow events that happened close to an update of the player list to ensure the list is in sync with all clients
                if (Time.timeSinceLevelLoad - _timeOfLastPlayerListUpdate < 1f)
                    return;
                char handChar = isRightHand ? 'R' : 'L';
                string eventType = isHoldEvent ? "HO" : "RE";
                string functionName = $"P{_localPlayerIndex}{handChar}{eventType}";
                if (_logEvents)
                    Debug.Log($"[LIFTUP] Sending event {functionName} with playerIndex {_localPlayerIndex}, isRightHand {isRightHand}, isHoldEvent {isHoldEvent}");
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, functionName);
            }
        }
        #region RPC_RECEIVER_MULTIPLE_USERS
        /// <summary>
        /// Receiver for RPCs sent by picker players.
        /// </summary>
        /// <param name="playerIndex">Index inside <see cref="_allPlayersSorted"/> of player who called the RPC</param>
        /// <param name="isRightHand">Wheter right or left hand of player from RPC</param>
        /// <param name="isHoldEvent">Wheter it's the hold or release event</param>
        public void ReceivePickerEvent(int playerIndex, bool isRightHand, bool isHoldEvent)
        {
            if (_logEvents)
            {
                string handName = isRightHand ? "right hand" : "left hand";
                string eventName = isHoldEvent ? "hold" : "release";
                Debug.Log($"[LIFTUP] Receiving {eventName} event [{handName}] from '{_allPlayersSorted[playerIndex].displayName}' ({playerIndex})");
            }
            //respect userchoice for not being picked up
            if (!_canBePickedUp)
            {
                if (_logEvents)
                    Debug.Log($"[LIFTUP] [Ignoring event] LocalPlayer can't be picked up.");
                return;
            }
            //skip our own events that we've sent
            if (playerIndex == _localPlayerIndex)
            {
                if (_logEvents)
                    Debug.Log($"[LIFTUP] [Ignoring event] Pick event sent from localPlayer.");
                return;
            }
            //drop-events are always sent, so they should be filtered first
            if (!_playerIsHeld && !isHoldEvent)
            {
                if (_logEvents)
                    Debug.Log($"[LIFTUP] [Ignoring event] Release event while localPlayer is currently not being held is not relevant.");
                return;
            }
            //skip 'invalid' player IDs to avoid nullpointer exceptions
            if (playerIndex >= _playerAmount)
            {
                if (_logEvents)
                    Debug.Log($"[LIFTUP] [Ignoring event] Picker player index exceeded player amount (soft error).");
                return;
            }
            //don't allow events that happened close to an update of the player list to ensure the list is in sync with all clients
            if (Time.timeSinceLevelLoad - _timeOfLastPlayerListUpdate < 1f)
            {
                if (_logEvents)
                    Debug.Log($"[LIFTUP] [Ignoring event] Player list updated < 1 second ago.");
                return;
            }
            //read out the picker player
            VRCPlayerApi pickerPlayer = _allPlayersSorted[playerIndex];
            if (_playerIsHeld)
            {
                //don't allow more than one person at a time to hold localPlayer
                if (pickerPlayer != _pickerPlayer)
                {
                    if (_logEvents)
                        Debug.Log($"[LIFTUP] [Ignoring event] LocalPlayer is currently already held by '{pickerPlayer.displayName}'");
                    return;
                }
                bool currentPickerButtonState = isRightHand ? _isButtonPressedNetworkCopyR : _isButtonPressedNetworkCopyL;
                if (currentPickerButtonState == isHoldEvent)
                {
                    if (_logEvents)
                        Debug.Log($"[LIFTUP] [Ignoring event] Picker button state didn't change since laste event.");
                    return;
                }
                if (!isHoldEvent)
                {
                    //this is ony reached for the same player while being held
                    //a release event means we just set the variable to false
                    if (isRightHand)
                        _isButtonPressedNetworkCopyR = false;
                    else
                        _isButtonPressedNetworkCopyL = false;
                    //don't run the code below if it's not a hold event
                    if (_logEvents)
                        Debug.Log($"[LIFTUP] [accepting event] New picker button state is now 'released'");
                    return;
                }
            }
            //if we reach this point, we need to check if the picker hand is close to our localPlayer
            HumanBodyBones pickHandBone = GetBestHandBone(pickerPlayer, isRightHand);
            float pickerPlayerHeight = MeasurePlayerHeight(pickerPlayer);
            float pickerHandDistance = GetPickerHandDistance(pickerPlayerHeight, pickHandBone);
            if (_logEvents)
            {
                Debug.Log($"[LIFTUP] [accepting event] Now checking for picker size {pickerPlayerHeight} and bone {pickHandBone.ToString()} in max hand distance {pickerHandDistance}");
                Debug.Log($"[LIFTUP] LocalPlayer bone distance is currently {_localPlayerBoneDistance}.");
            }
            //calculate if their hand position was close to one of localPlayer bones
            if (CheckIfPlayerIsHeld(_localPlayer, pickerPlayer.GetBonePosition(pickHandBone), receiverIsLocalPlayer: true, pickerHandDistance))
            {
                if (!_playerIsHeld)
                {
                    //we need to reset all picker variables if the player is currently not held so that incoming events are all accepted again
                    _isButtonPressedNetworkCopyR = false;
                    _isButtonPressedNetworkCopyL = false;
                    _isButtonPressedLocalCopyR = false;
                    _isButtonPressedLocalCopyL = false;
                }
                //check if picker has changed
                if (pickerPlayer != _pickerPlayer)
                {
                    if (_logEvents)
                    {
                        Debug.Log($"[LIFTUP] New picker was accepted, resetting variables from old picker and setting the new ones.");
                    }
                    //positions are unreliable when receiving RPCs so we do the real calculations in Update instead and only set the buttons here to trigger that
                    _pickerPlayer = pickerPlayer;
                }
                else
                {
                    if (_logEvents)
                        Debug.Log($"[LIFTUP] Old picker was accepted again, now setting to hold.");
                }
                //updating hand bone distance value from picker which will be needed next update
                _pickerPlayerHandDistance = pickerHandDistance;
                //now triggering the real check if player is held which will happen next update (where bone positions are more accurate)
                if (isRightHand)
                    _isButtonPressedNetworkCopyR = true;
                else
                    _isButtonPressedNetworkCopyL = true;
            }
            else if (_logEvents)
            {
                Debug.Log($"[LIFTUP] Picker was not in reach.");
            }
        }
        #endregion RPC_RECEIVER_MULTIPLE_USERS
        /// <summary>
        /// Time at which the player height was updated the last time. Is initualizes as a big negative number to force an update on first run.
        /// </summary>
        private float _lastUpdateTime = -1000000;
        /// <summary>
        /// Scales <see cref="_playerThrowWeightFactor"/> and <see cref="_localPlayerBoneDistance"/> to localPlayer height
        /// </summary>
        private void UpdateLocalPlayerHeightFactor()
        {
            //only update the player height when the last update is too old or it's the first run after level load
            if (Time.timeSinceLevelLoad - _lastUpdateTime < PLAYER_HEIGHT_UPDATE_INTERVAL)
                return;
            _lastUpdateTime = Time.timeSinceLevelLoad;
            _playerHeight = MeasurePlayerHeight(_localPlayer);
            _localPlayerBoneDistance = (_maxDistanceToAnyBone / 1.5f) * _playerHeight;
            _localPlayerPickHandDistance = GetPickerHandDistance(_playerHeight, GetBestHandBone(_localPlayer, true));
        }
        /// <summary>
        /// Estimates the player height according to certain bone measurements (foot-to-head bone chain)
        /// </summary>
        private float MeasurePlayerHeight(VRCPlayerApi player)
        {
            //reading player bone positions
            Vector3 head = player.GetBonePosition(HumanBodyBones.Head);
            Vector3 spine = player.GetBonePosition(HumanBodyBones.Spine);
            Vector3 rightUpperLeg = player.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 rightLowerLeg = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 rightFoot = player.GetBonePosition(HumanBodyBones.RightFoot);
            //making sure all bones are valid
            if (head == Vector3.zero || spine == Vector3.zero || rightUpperLeg == Vector3.zero || rightLowerLeg == Vector3.zero || rightFoot == Vector3.zero)
            {
                return 1.5f; //assuming the player has standard height is all we can do when a bone is missing
            }
            else
            {
                return Vector3.Distance(rightFoot, rightLowerLeg) + Vector3.Distance(rightLowerLeg, rightUpperLeg) +
                Vector3.Distance(rightUpperLeg, spine) + Vector3.Distance(spine, head);
            }
        }
        /// <summary>
        /// Returns the hand distance for the picker player depending on pickerBone and scaled to pickerSize
        /// </summary>
        private float GetPickerHandDistance(float pickerSize, HumanBodyBones pickerBone)
        {
            if (pickerBone == HumanBodyBones.LeftHand || pickerBone == HumanBodyBones.RightHand)
                return PICKER_HAND_DISTANCE_DEFAULT / 1.5f * pickerSize;
            else
                return PICKER_FINGER_DISTANCE_DEFAULT / 1.5f * pickerSize;
        }
        #region AllRPCs
        ////this is the blueprint in case we want to change these functions in the future
        //public void P#J1##1##3#()
        //{
        //    ReceivePickerEvent(#J1#, #2#, #4#);
        //}
        public void P0RRE()
        {
            ReceivePickerEvent(0, true, false);
        }
        public void P0LRE()
        {
            ReceivePickerEvent(0, false, false);
        }
        public void P0RHO()
        {
            ReceivePickerEvent(0, true, true);
        }
        public void P0LHO()
        {
            ReceivePickerEvent(0, false, true);
        }
        public void P1RHO()
        {
            ReceivePickerEvent(1, true, true);
        }
        public void P1LHO()
        {
            ReceivePickerEvent(1, false, true);
        }

        public void P2RHO()
        {
            ReceivePickerEvent(2, true, true);
        }
        public void P2LHO()
        {
            ReceivePickerEvent(2, false, true);
        }

        public void P3RHO()
        {
            ReceivePickerEvent(3, true, true);
        }
        public void P3LHO()
        {
            ReceivePickerEvent(3, false, true);
        }

        public void P4RHO()
        {
            ReceivePickerEvent(4, true, true);
        }
        public void P4LHO()
        {
            ReceivePickerEvent(4, false, true);
        }

        public void P5RHO()
        {
            ReceivePickerEvent(5, true, true);
        }
        public void P5LHO()
        {
            ReceivePickerEvent(5, false, true);
        }

        public void P6RHO()
        {
            ReceivePickerEvent(6, true, true);
        }
        public void P6LHO()
        {
            ReceivePickerEvent(6, false, true);
        }

        public void P7RHO()
        {
            ReceivePickerEvent(7, true, true);
        }
        public void P7LHO()
        {
            ReceivePickerEvent(7, false, true);
        }

        public void P8RHO()
        {
            ReceivePickerEvent(8, true, true);
        }
        public void P8LHO()
        {
            ReceivePickerEvent(8, false, true);
        }

        public void P9RHO()
        {
            ReceivePickerEvent(9, true, true);
        }
        public void P9LHO()
        {
            ReceivePickerEvent(9, false, true);
        }

        public void P10RHO()
        {
            ReceivePickerEvent(10, true, true);
        }
        public void P10LHO()
        {
            ReceivePickerEvent(10, false, true);
        }

        public void P11RHO()
        {
            ReceivePickerEvent(11, true, true);
        }
        public void P11LHO()
        {
            ReceivePickerEvent(11, false, true);
        }

        public void P12RHO()
        {
            ReceivePickerEvent(12, true, true);
        }
        public void P12LHO()
        {
            ReceivePickerEvent(12, false, true);
        }

        public void P13RHO()
        {
            ReceivePickerEvent(13, true, true);
        }
        public void P13LHO()
        {
            ReceivePickerEvent(13, false, true);
        }

        public void P14RHO()
        {
            ReceivePickerEvent(14, true, true);
        }
        public void P14LHO()
        {
            ReceivePickerEvent(14, false, true);
        }

        public void P15RHO()
        {
            ReceivePickerEvent(15, true, true);
        }
        public void P15LHO()
        {
            ReceivePickerEvent(15, false, true);
        }

        public void P16RHO()
        {
            ReceivePickerEvent(16, true, true);
        }
        public void P16LHO()
        {
            ReceivePickerEvent(16, false, true);
        }

        public void P17RHO()
        {
            ReceivePickerEvent(17, true, true);
        }
        public void P17LHO()
        {
            ReceivePickerEvent(17, false, true);
        }

        public void P18RHO()
        {
            ReceivePickerEvent(18, true, true);
        }
        public void P18LHO()
        {
            ReceivePickerEvent(18, false, true);
        }

        public void P19RHO()
        {
            ReceivePickerEvent(19, true, true);
        }
        public void P19LHO()
        {
            ReceivePickerEvent(19, false, true);
        }

        public void P20RHO()
        {
            ReceivePickerEvent(20, true, true);
        }
        public void P20LHO()
        {
            ReceivePickerEvent(20, false, true);
        }

        public void P21RHO()
        {
            ReceivePickerEvent(21, true, true);
        }
        public void P21LHO()
        {
            ReceivePickerEvent(21, false, true);
        }

        public void P22RHO()
        {
            ReceivePickerEvent(22, true, true);
        }
        public void P22LHO()
        {
            ReceivePickerEvent(22, false, true);
        }

        public void P23RHO()
        {
            ReceivePickerEvent(23, true, true);
        }
        public void P23LHO()
        {
            ReceivePickerEvent(23, false, true);
        }

        public void P24RHO()
        {
            ReceivePickerEvent(24, true, true);
        }
        public void P24LHO()
        {
            ReceivePickerEvent(24, false, true);
        }

        public void P25RHO()
        {
            ReceivePickerEvent(25, true, true);
        }
        public void P25LHO()
        {
            ReceivePickerEvent(25, false, true);
        }

        public void P26RHO()
        {
            ReceivePickerEvent(26, true, true);
        }
        public void P26LHO()
        {
            ReceivePickerEvent(26, false, true);
        }

        public void P27RHO()
        {
            ReceivePickerEvent(27, true, true);
        }
        public void P27LHO()
        {
            ReceivePickerEvent(27, false, true);
        }

        public void P28RHO()
        {
            ReceivePickerEvent(28, true, true);
        }
        public void P28LHO()
        {
            ReceivePickerEvent(28, false, true);
        }

        public void P29RHO()
        {
            ReceivePickerEvent(29, true, true);
        }
        public void P29LHO()
        {
            ReceivePickerEvent(29, false, true);
        }

        public void P30RHO()
        {
            ReceivePickerEvent(30, true, true);
        }
        public void P30LHO()
        {
            ReceivePickerEvent(30, false, true);
        }

        public void P31RHO()
        {
            ReceivePickerEvent(31, true, true);
        }
        public void P31LHO()
        {
            ReceivePickerEvent(31, false, true);
        }

        public void P32RHO()
        {
            ReceivePickerEvent(32, true, true);
        }
        public void P32LHO()
        {
            ReceivePickerEvent(32, false, true);
        }

        public void P33RHO()
        {
            ReceivePickerEvent(33, true, true);
        }
        public void P33LHO()
        {
            ReceivePickerEvent(33, false, true);
        }

        public void P34RHO()
        {
            ReceivePickerEvent(34, true, true);
        }
        public void P34LHO()
        {
            ReceivePickerEvent(34, false, true);
        }

        public void P35RHO()
        {
            ReceivePickerEvent(35, true, true);
        }
        public void P35LHO()
        {
            ReceivePickerEvent(35, false, true);
        }

        public void P36RHO()
        {
            ReceivePickerEvent(36, true, true);
        }
        public void P36LHO()
        {
            ReceivePickerEvent(36, false, true);
        }

        public void P37RHO()
        {
            ReceivePickerEvent(37, true, true);
        }
        public void P37LHO()
        {
            ReceivePickerEvent(37, false, true);
        }

        public void P38RHO()
        {
            ReceivePickerEvent(38, true, true);
        }
        public void P38LHO()
        {
            ReceivePickerEvent(38, false, true);
        }

        public void P39RHO()
        {
            ReceivePickerEvent(39, true, true);
        }
        public void P39LHO()
        {
            ReceivePickerEvent(39, false, true);
        }

        public void P40RHO()
        {
            ReceivePickerEvent(40, true, true);
        }
        public void P40LHO()
        {
            ReceivePickerEvent(40, false, true);
        }

        public void P41RHO()
        {
            ReceivePickerEvent(41, true, true);
        }
        public void P41LHO()
        {
            ReceivePickerEvent(41, false, true);
        }

        public void P42RHO()
        {
            ReceivePickerEvent(42, true, true);
        }
        public void P42LHO()
        {
            ReceivePickerEvent(42, false, true);
        }

        public void P43RHO()
        {
            ReceivePickerEvent(43, true, true);
        }
        public void P43LHO()
        {
            ReceivePickerEvent(43, false, true);
        }

        public void P44RHO()
        {
            ReceivePickerEvent(44, true, true);
        }
        public void P44LHO()
        {
            ReceivePickerEvent(44, false, true);
        }

        public void P45RHO()
        {
            ReceivePickerEvent(45, true, true);
        }
        public void P45LHO()
        {
            ReceivePickerEvent(45, false, true);
        }

        public void P46RHO()
        {
            ReceivePickerEvent(46, true, true);
        }
        public void P46LHO()
        {
            ReceivePickerEvent(46, false, true);
        }

        public void P47RHO()
        {
            ReceivePickerEvent(47, true, true);
        }
        public void P47LHO()
        {
            ReceivePickerEvent(47, false, true);
        }

        public void P48RHO()
        {
            ReceivePickerEvent(48, true, true);
        }
        public void P48LHO()
        {
            ReceivePickerEvent(48, false, true);
        }

        public void P49RHO()
        {
            ReceivePickerEvent(49, true, true);
        }
        public void P49LHO()
        {
            ReceivePickerEvent(49, false, true);
        }

        public void P50RHO()
        {
            ReceivePickerEvent(50, true, true);
        }
        public void P50LHO()
        {
            ReceivePickerEvent(50, false, true);
        }

        public void P51RHO()
        {
            ReceivePickerEvent(51, true, true);
        }
        public void P51LHO()
        {
            ReceivePickerEvent(51, false, true);
        }

        public void P52RHO()
        {
            ReceivePickerEvent(52, true, true);
        }
        public void P52LHO()
        {
            ReceivePickerEvent(52, false, true);
        }

        public void P53RHO()
        {
            ReceivePickerEvent(53, true, true);
        }
        public void P53LHO()
        {
            ReceivePickerEvent(53, false, true);
        }

        public void P54RHO()
        {
            ReceivePickerEvent(54, true, true);
        }
        public void P54LHO()
        {
            ReceivePickerEvent(54, false, true);
        }

        public void P55RHO()
        {
            ReceivePickerEvent(55, true, true);
        }
        public void P55LHO()
        {
            ReceivePickerEvent(55, false, true);
        }

        public void P56RHO()
        {
            ReceivePickerEvent(56, true, true);
        }
        public void P56LHO()
        {
            ReceivePickerEvent(56, false, true);
        }

        public void P57RHO()
        {
            ReceivePickerEvent(57, true, true);
        }
        public void P57LHO()
        {
            ReceivePickerEvent(57, false, true);
        }

        public void P58RHO()
        {
            ReceivePickerEvent(58, true, true);
        }
        public void P58LHO()
        {
            ReceivePickerEvent(58, false, true);
        }

        public void P59RHO()
        {
            ReceivePickerEvent(59, true, true);
        }
        public void P59LHO()
        {
            ReceivePickerEvent(59, false, true);
        }

        public void P60RHO()
        {
            ReceivePickerEvent(60, true, true);
        }
        public void P60LHO()
        {
            ReceivePickerEvent(60, false, true);
        }

        public void P61RHO()
        {
            ReceivePickerEvent(61, true, true);
        }
        public void P61LHO()
        {
            ReceivePickerEvent(61, false, true);
        }

        public void P62RHO()
        {
            ReceivePickerEvent(62, true, true);
        }
        public void P62LHO()
        {
            ReceivePickerEvent(62, false, true);
        }

        public void P63RHO()
        {
            ReceivePickerEvent(63, true, true);
        }
        public void P63LHO()
        {
            ReceivePickerEvent(63, false, true);
        }

        public void P64RHO()
        {
            ReceivePickerEvent(64, true, true);
        }
        public void P64LHO()
        {
            ReceivePickerEvent(64, false, true);
        }

        public void P65RHO()
        {
            ReceivePickerEvent(65, true, true);
        }
        public void P65LHO()
        {
            ReceivePickerEvent(65, false, true);
        }

        public void P66RHO()
        {
            ReceivePickerEvent(66, true, true);
        }
        public void P66LHO()
        {
            ReceivePickerEvent(66, false, true);
        }

        public void P67RHO()
        {
            ReceivePickerEvent(67, true, true);
        }
        public void P67LHO()
        {
            ReceivePickerEvent(67, false, true);
        }

        public void P68RHO()
        {
            ReceivePickerEvent(68, true, true);
        }
        public void P68LHO()
        {
            ReceivePickerEvent(68, false, true);
        }

        public void P69RHO()
        {
            ReceivePickerEvent(69, true, true);
        }
        public void P69LHO()
        {
            ReceivePickerEvent(69, false, true);
        }

        public void P70RHO()
        {
            ReceivePickerEvent(70, true, true);
        }
        public void P70LHO()
        {
            ReceivePickerEvent(70, false, true);
        }

        public void P71RHO()
        {
            ReceivePickerEvent(71, true, true);
        }
        public void P71LHO()
        {
            ReceivePickerEvent(71, false, true);
        }

        public void P72RHO()
        {
            ReceivePickerEvent(72, true, true);
        }
        public void P72LHO()
        {
            ReceivePickerEvent(72, false, true);
        }

        public void P73RHO()
        {
            ReceivePickerEvent(73, true, true);
        }
        public void P73LHO()
        {
            ReceivePickerEvent(73, false, true);
        }

        public void P74RHO()
        {
            ReceivePickerEvent(74, true, true);
        }
        public void P74LHO()
        {
            ReceivePickerEvent(74, false, true);
        }

        public void P75RHO()
        {
            ReceivePickerEvent(75, true, true);
        }
        public void P75LHO()
        {
            ReceivePickerEvent(75, false, true);
        }

        public void P76RHO()
        {
            ReceivePickerEvent(76, true, true);
        }
        public void P76LHO()
        {
            ReceivePickerEvent(76, false, true);
        }

        public void P77RHO()
        {
            ReceivePickerEvent(77, true, true);
        }
        public void P77LHO()
        {
            ReceivePickerEvent(77, false, true);
        }

        public void P78RHO()
        {
            ReceivePickerEvent(78, true, true);
        }
        public void P78LHO()
        {
            ReceivePickerEvent(78, false, true);
        }

        public void P79RHO()
        {
            ReceivePickerEvent(79, true, true);
        }
        public void P79LHO()
        {
            ReceivePickerEvent(79, false, true);
        }

        public void P80RHO()
        {
            ReceivePickerEvent(80, true, true);
        }
        public void P80LHO()
        {
            ReceivePickerEvent(80, false, true);
        }

        public void P1RRE()
        {
            ReceivePickerEvent(1, true, false);
        }
        public void P1LRE()
        {
            ReceivePickerEvent(1, false, false);
        }

        public void P2RRE()
        {
            ReceivePickerEvent(2, true, false);
        }
        public void P2LRE()
        {
            ReceivePickerEvent(2, false, false);
        }

        public void P3RRE()
        {
            ReceivePickerEvent(3, true, false);
        }
        public void P3LRE()
        {
            ReceivePickerEvent(3, false, false);
        }

        public void P4RRE()
        {
            ReceivePickerEvent(4, true, false);
        }
        public void P4LRE()
        {
            ReceivePickerEvent(4, false, false);
        }

        public void P5RRE()
        {
            ReceivePickerEvent(5, true, false);
        }
        public void P5LRE()
        {
            ReceivePickerEvent(5, false, false);
        }

        public void P6RRE()
        {
            ReceivePickerEvent(6, true, false);
        }
        public void P6LRE()
        {
            ReceivePickerEvent(6, false, false);
        }

        public void P7RRE()
        {
            ReceivePickerEvent(7, true, false);
        }
        public void P7LRE()
        {
            ReceivePickerEvent(7, false, false);
        }

        public void P8RRE()
        {
            ReceivePickerEvent(8, true, false);
        }
        public void P8LRE()
        {
            ReceivePickerEvent(8, false, false);
        }

        public void P9RRE()
        {
            ReceivePickerEvent(9, true, false);
        }
        public void P9LRE()
        {
            ReceivePickerEvent(9, false, false);
        }

        public void P10RRE()
        {
            ReceivePickerEvent(10, true, false);
        }
        public void P10LRE()
        {
            ReceivePickerEvent(10, false, false);
        }

        public void P11RRE()
        {
            ReceivePickerEvent(11, true, false);
        }
        public void P11LRE()
        {
            ReceivePickerEvent(11, false, false);
        }

        public void P12RRE()
        {
            ReceivePickerEvent(12, true, false);
        }
        public void P12LRE()
        {
            ReceivePickerEvent(12, false, false);
        }

        public void P13RRE()
        {
            ReceivePickerEvent(13, true, false);
        }
        public void P13LRE()
        {
            ReceivePickerEvent(13, false, false);
        }

        public void P14RRE()
        {
            ReceivePickerEvent(14, true, false);
        }
        public void P14LRE()
        {
            ReceivePickerEvent(14, false, false);
        }

        public void P15RRE()
        {
            ReceivePickerEvent(15, true, false);
        }
        public void P15LRE()
        {
            ReceivePickerEvent(15, false, false);
        }

        public void P16RRE()
        {
            ReceivePickerEvent(16, true, false);
        }
        public void P16LRE()
        {
            ReceivePickerEvent(16, false, false);
        }

        public void P17RRE()
        {
            ReceivePickerEvent(17, true, false);
        }
        public void P17LRE()
        {
            ReceivePickerEvent(17, false, false);
        }

        public void P18RRE()
        {
            ReceivePickerEvent(18, true, false);
        }
        public void P18LRE()
        {
            ReceivePickerEvent(18, false, false);
        }

        public void P19RRE()
        {
            ReceivePickerEvent(19, true, false);
        }
        public void P19LRE()
        {
            ReceivePickerEvent(19, false, false);
        }

        public void P20RRE()
        {
            ReceivePickerEvent(20, true, false);
        }
        public void P20LRE()
        {
            ReceivePickerEvent(20, false, false);
        }

        public void P21RRE()
        {
            ReceivePickerEvent(21, true, false);
        }
        public void P21LRE()
        {
            ReceivePickerEvent(21, false, false);
        }

        public void P22RRE()
        {
            ReceivePickerEvent(22, true, false);
        }
        public void P22LRE()
        {
            ReceivePickerEvent(22, false, false);
        }

        public void P23RRE()
        {
            ReceivePickerEvent(23, true, false);
        }
        public void P23LRE()
        {
            ReceivePickerEvent(23, false, false);
        }

        public void P24RRE()
        {
            ReceivePickerEvent(24, true, false);
        }
        public void P24LRE()
        {
            ReceivePickerEvent(24, false, false);
        }

        public void P25RRE()
        {
            ReceivePickerEvent(25, true, false);
        }
        public void P25LRE()
        {
            ReceivePickerEvent(25, false, false);
        }

        public void P26RRE()
        {
            ReceivePickerEvent(26, true, false);
        }
        public void P26LRE()
        {
            ReceivePickerEvent(26, false, false);
        }

        public void P27RRE()
        {
            ReceivePickerEvent(27, true, false);
        }
        public void P27LRE()
        {
            ReceivePickerEvent(27, false, false);
        }

        public void P28RRE()
        {
            ReceivePickerEvent(28, true, false);
        }
        public void P28LRE()
        {
            ReceivePickerEvent(28, false, false);
        }

        public void P29RRE()
        {
            ReceivePickerEvent(29, true, false);
        }
        public void P29LRE()
        {
            ReceivePickerEvent(29, false, false);
        }

        public void P30RRE()
        {
            ReceivePickerEvent(30, true, false);
        }
        public void P30LRE()
        {
            ReceivePickerEvent(30, false, false);
        }

        public void P31RRE()
        {
            ReceivePickerEvent(31, true, false);
        }
        public void P31LRE()
        {
            ReceivePickerEvent(31, false, false);
        }

        public void P32RRE()
        {
            ReceivePickerEvent(32, true, false);
        }
        public void P32LRE()
        {
            ReceivePickerEvent(32, false, false);
        }

        public void P33RRE()
        {
            ReceivePickerEvent(33, true, false);
        }
        public void P33LRE()
        {
            ReceivePickerEvent(33, false, false);
        }

        public void P34RRE()
        {
            ReceivePickerEvent(34, true, false);
        }
        public void P34LRE()
        {
            ReceivePickerEvent(34, false, false);
        }

        public void P35RRE()
        {
            ReceivePickerEvent(35, true, false);
        }
        public void P35LRE()
        {
            ReceivePickerEvent(35, false, false);
        }

        public void P36RRE()
        {
            ReceivePickerEvent(36, true, false);
        }
        public void P36LRE()
        {
            ReceivePickerEvent(36, false, false);
        }

        public void P37RRE()
        {
            ReceivePickerEvent(37, true, false);
        }
        public void P37LRE()
        {
            ReceivePickerEvent(37, false, false);
        }

        public void P38RRE()
        {
            ReceivePickerEvent(38, true, false);
        }
        public void P38LRE()
        {
            ReceivePickerEvent(38, false, false);
        }

        public void P39RRE()
        {
            ReceivePickerEvent(39, true, false);
        }
        public void P39LRE()
        {
            ReceivePickerEvent(39, false, false);
        }

        public void P40RRE()
        {
            ReceivePickerEvent(40, true, false);
        }
        public void P40LRE()
        {
            ReceivePickerEvent(40, false, false);
        }

        public void P41RRE()
        {
            ReceivePickerEvent(41, true, false);
        }
        public void P41LRE()
        {
            ReceivePickerEvent(41, false, false);
        }

        public void P42RRE()
        {
            ReceivePickerEvent(42, true, false);
        }
        public void P42LRE()
        {
            ReceivePickerEvent(42, false, false);
        }

        public void P43RRE()
        {
            ReceivePickerEvent(43, true, false);
        }
        public void P43LRE()
        {
            ReceivePickerEvent(43, false, false);
        }

        public void P44RRE()
        {
            ReceivePickerEvent(44, true, false);
        }
        public void P44LRE()
        {
            ReceivePickerEvent(44, false, false);
        }

        public void P45RRE()
        {
            ReceivePickerEvent(45, true, false);
        }
        public void P45LRE()
        {
            ReceivePickerEvent(45, false, false);
        }

        public void P46RRE()
        {
            ReceivePickerEvent(46, true, false);
        }
        public void P46LRE()
        {
            ReceivePickerEvent(46, false, false);
        }

        public void P47RRE()
        {
            ReceivePickerEvent(47, true, false);
        }
        public void P47LRE()
        {
            ReceivePickerEvent(47, false, false);
        }

        public void P48RRE()
        {
            ReceivePickerEvent(48, true, false);
        }
        public void P48LRE()
        {
            ReceivePickerEvent(48, false, false);
        }

        public void P49RRE()
        {
            ReceivePickerEvent(49, true, false);
        }
        public void P49LRE()
        {
            ReceivePickerEvent(49, false, false);
        }

        public void P50RRE()
        {
            ReceivePickerEvent(50, true, false);
        }
        public void P50LRE()
        {
            ReceivePickerEvent(50, false, false);
        }

        public void P51RRE()
        {
            ReceivePickerEvent(51, true, false);
        }
        public void P51LRE()
        {
            ReceivePickerEvent(51, false, false);
        }

        public void P52RRE()
        {
            ReceivePickerEvent(52, true, false);
        }
        public void P52LRE()
        {
            ReceivePickerEvent(52, false, false);
        }

        public void P53RRE()
        {
            ReceivePickerEvent(53, true, false);
        }
        public void P53LRE()
        {
            ReceivePickerEvent(53, false, false);
        }

        public void P54RRE()
        {
            ReceivePickerEvent(54, true, false);
        }
        public void P54LRE()
        {
            ReceivePickerEvent(54, false, false);
        }

        public void P55RRE()
        {
            ReceivePickerEvent(55, true, false);
        }
        public void P55LRE()
        {
            ReceivePickerEvent(55, false, false);
        }

        public void P56RRE()
        {
            ReceivePickerEvent(56, true, false);
        }
        public void P56LRE()
        {
            ReceivePickerEvent(56, false, false);
        }

        public void P57RRE()
        {
            ReceivePickerEvent(57, true, false);
        }
        public void P57LRE()
        {
            ReceivePickerEvent(57, false, false);
        }

        public void P58RRE()
        {
            ReceivePickerEvent(58, true, false);
        }
        public void P58LRE()
        {
            ReceivePickerEvent(58, false, false);
        }

        public void P59RRE()
        {
            ReceivePickerEvent(59, true, false);
        }
        public void P59LRE()
        {
            ReceivePickerEvent(59, false, false);
        }

        public void P60RRE()
        {
            ReceivePickerEvent(60, true, false);
        }
        public void P60LRE()
        {
            ReceivePickerEvent(60, false, false);
        }

        public void P61RRE()
        {
            ReceivePickerEvent(61, true, false);
        }
        public void P61LRE()
        {
            ReceivePickerEvent(61, false, false);
        }

        public void P62RRE()
        {
            ReceivePickerEvent(62, true, false);
        }
        public void P62LRE()
        {
            ReceivePickerEvent(62, false, false);
        }

        public void P63RRE()
        {
            ReceivePickerEvent(63, true, false);
        }
        public void P63LRE()
        {
            ReceivePickerEvent(63, false, false);
        }

        public void P64RRE()
        {
            ReceivePickerEvent(64, true, false);
        }
        public void P64LRE()
        {
            ReceivePickerEvent(64, false, false);
        }

        public void P65RRE()
        {
            ReceivePickerEvent(65, true, false);
        }
        public void P65LRE()
        {
            ReceivePickerEvent(65, false, false);
        }

        public void P66RRE()
        {
            ReceivePickerEvent(66, true, false);
        }
        public void P66LRE()
        {
            ReceivePickerEvent(66, false, false);
        }

        public void P67RRE()
        {
            ReceivePickerEvent(67, true, false);
        }
        public void P67LRE()
        {
            ReceivePickerEvent(67, false, false);
        }

        public void P68RRE()
        {
            ReceivePickerEvent(68, true, false);
        }
        public void P68LRE()
        {
            ReceivePickerEvent(68, false, false);
        }

        public void P69RRE()
        {
            ReceivePickerEvent(69, true, false);
        }
        public void P69LRE()
        {
            ReceivePickerEvent(69, false, false);
        }

        public void P70RRE()
        {
            ReceivePickerEvent(70, true, false);
        }
        public void P70LRE()
        {
            ReceivePickerEvent(70, false, false);
        }

        public void P71RRE()
        {
            ReceivePickerEvent(71, true, false);
        }
        public void P71LRE()
        {
            ReceivePickerEvent(71, false, false);
        }

        public void P72RRE()
        {
            ReceivePickerEvent(72, true, false);
        }
        public void P72LRE()
        {
            ReceivePickerEvent(72, false, false);
        }

        public void P73RRE()
        {
            ReceivePickerEvent(73, true, false);
        }
        public void P73LRE()
        {
            ReceivePickerEvent(73, false, false);
        }

        public void P74RRE()
        {
            ReceivePickerEvent(74, true, false);
        }
        public void P74LRE()
        {
            ReceivePickerEvent(74, false, false);
        }

        public void P75RRE()
        {
            ReceivePickerEvent(75, true, false);
        }
        public void P75LRE()
        {
            ReceivePickerEvent(75, false, false);
        }

        public void P76RRE()
        {
            ReceivePickerEvent(76, true, false);
        }
        public void P76LRE()
        {
            ReceivePickerEvent(76, false, false);
        }

        public void P77RRE()
        {
            ReceivePickerEvent(77, true, false);
        }
        public void P77LRE()
        {
            ReceivePickerEvent(77, false, false);
        }

        public void P78RRE()
        {
            ReceivePickerEvent(78, true, false);
        }
        public void P78LRE()
        {
            ReceivePickerEvent(78, false, false);
        }

        public void P79RRE()
        {
            ReceivePickerEvent(79, true, false);
        }
        public void P79LRE()
        {
            ReceivePickerEvent(79, false, false);
        }

        public void P80RRE()
        {
            ReceivePickerEvent(80, true, false);
        }
        public void P80LRE()
        {
            ReceivePickerEvent(80, false, false);
        }
        #endregion AllRPCs
    }
}