
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/
/// </summary>
namespace ReimajoBoothAssets
{
    /// <summary>
    /// 
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// ATTENTION: THIS IS AN OLD AND DEPRECATED VERSION, USE THE NEWER VERSION INSTEAD!
    /// I'll leave this in here for educational reasons only. I worked on this to enable multiple script instances to work together
    /// until I came up with a better idea. It's probably interesting to look at, but the new script will be a better solution alltogether.
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// 
    /// Script that needs to be present a single time somewhere in the world.
    /// When set to "Restrict to one user", there is nothing else needed.
    /// You enable "Restrict to one user" and provide the case-sensitive VRChat username.
    /// This way, you could e.g. be the only player in your world with this superpower.
    /// 
    /// When this feature should be usable by all users, this script needs to be on an object with a collider on it
    /// that users can click to enable this feature for them. Per script, there can only be one user being able at
    /// any given time to lift other players up due to current VRChat limitations. In this case, the last person that
    /// clicked on the script object will be able to lift other players up.
    /// 
    /// When needed, you can place the script multiple times in the world to enable more users to use this feature
    /// at the same time, but all those instances need to be parented to the same parent object to sync to each other.
    /// 
    /// In case you want to restrict this feature to multiple users in your world, you'd need to provide a list of usernames.
    /// </summary>
    public class OldPickingAvatarsUpVersion : UdonSharpBehaviour
    {
        #region SerializedFields
        [Space(10)]
        [SerializeField, Tooltip("Allowing anyone to use this feature by clicking this object (requires a collider on this object)")]
        private bool _allowAnyoneToUseThisFeature = true; //this bool isn't used, it's just here to explain the default behaviour
        [Space(15)]
        [SerializeField, Tooltip("Restricting the usage to just the one user defined in the following field")]
        private bool _restrictToOneUser = false;
        [SerializeField, Tooltip("VRChat username (case sensitive), only needed when 'restriction to one user' is selected")]
        private string _vrchatUsernameThatOneUser = "";
        [Space(15)]
        [SerializeField, Tooltip("Restricting the usage to any user in the list (requires a collider on this object)")]
        private bool _restrictToWhiteList = false;
        [SerializeField, Tooltip("List of (case-sensitive) VRChat usernames (required when 'restricted to whitelist' is selected)")]
        private string[] _whiteList;
        [SerializeField, Tooltip("Automaticly enables the feature for the latest joined whitelisted user")]
        private bool _enableOnWhitelistedJoin = false;
        [Space(15)]
        [SerializeField, Tooltip("Allows a player who is being held to escape by moving")]
        private bool _allowPlayerToEscape = false;
        [SerializeField, Tooltip("Minimum required distance to any bone to pick an avatar up")]
        private float _minDistanceToAnyBone = 0.3f;
        [SerializeField, Tooltip("Adds a ground object under the user to prevent falling animation (not recommended)")]
        private bool _addFakeGround = false;
        [SerializeField, Tooltip("Invisible plane with mesh collider set to layer 'Environment' (only needed when Add Fake Ground is selected)")]
        private GameObject _fakeGroundObj;
        [SerializeField, Tooltip("Disables the mesh renderer if this feature is disabled for the current user")]
        private bool _disableMeshRenderer = true;
        [SerializeField, Tooltip("Multiplies the force vector at which player is being dropped (thrown)")]
        private float _yeetForceMultiplier = 3;
        [SerializeField, Tooltip("Material that will be applied to this object to set it into 'claimed' state")]
        private Material _buttonMaterialClaimed;
        [SerializeField, Tooltip("Material that will be applied to this object to set it into 'unclaimed' state")]
        private Material _buttonMaterialUnclaimed;
        [SerializeField, Tooltip("Material that will be applied to this object to set it into 'locked' state")]
        private Material _buttonMaterialLocked;
        [SerializeField, Tooltip("Zero-based index of the button material if the object has multiple material slots, default is 0")]
        private int _buttonMaterialIndex = 0;
        [SerializeField, Tooltip("Whether or not this object should change color when it's activated")]
        private bool _changeColor = true;
        [SerializeField, Tooltip("Time in seconds how long this behaviour is locked after it was claimed by someone")]
        private float _lockTimeWhenClaimed = 5;
        #endregion SerializedFields
        #region PrivateFields
        private float _timeWhenClaimed;
        private bool _isLocked = false;
        private bool _isWhitelistedUser = false;
        private Transform _fakeGround;
        private bool _pickerOwnerClaimed = false;
        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi _pickerPlayer;
        private bool _userIsInVR;
        private bool _isButtonPressedLocalCopyL = false;
        private bool _isButtonPressedLocalCopyR = false;
        private bool _isButtonPressedNetworkCopyL = false;
        private bool _isButtonPressedNetworkCopyR = false;
        private bool _playerIsHeld = false;
        private bool _playerIsHeldByHandR;
        private float _startPickHeight;
        private Vector3 _startPickPos;
        private Vector3 _startPlayerPos;
        private float _startGroundHeight;
        private Vector3 _positionOneFrameAgo;
        private Vector3 _positionTwoFramesAgo;
        private float _deltaTimeOneFrameAgo;
        private Renderer _buttonMaterialRenderer;
        private OldPickingAvatarsUpVersion[] _otherScriptInstances = null;
        readonly object[] _bodyBones = new object[] { HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.Neck, HumanBodyBones.Head, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftHand, HumanBodyBones.RightHand, HumanBodyBones.LeftToes, HumanBodyBones.RightToes, HumanBodyBones.LeftEye, HumanBodyBones.RightEye, HumanBodyBones.Jaw, HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal, HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal, HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal, HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal, HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal, HumanBodyBones.UpperChest };
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
            if (transform.parent != null)
            {
                OldPickingAvatarsUpVersion[] allScriptInstances = transform.parent.GetComponentsInChildren<OldPickingAvatarsUpVersion>();
                if (allScriptInstances.Length > 1)
                {
                    _otherScriptInstances = new OldPickingAvatarsUpVersion[allScriptInstances.Length - 1];
                    int x = 0;
                    for (int i = 0; i < allScriptInstances.Length; i++)
                    {
                        if (allScriptInstances[i] != this)
                        {
                            _otherScriptInstances[x] = allScriptInstances[i];
                            x++;
                        }
                    }
                }
            }
            if (_changeColor)
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
                        SetButtonMaterial(_buttonMaterialUnclaimed);
                    }
                }
                else
                {
                    _changeColor = false;
                }
            }
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
            _userIsInVR = _localPlayer.IsUserInVR();
            bool disableInteractions = false;
            if (!_userIsInVR)
            {
                //this script should only be used by VR players so we remove the interactable components for any desktop player
                disableInteractions = true;
            }
            else if (_restrictToOneUser)
            {
                //this script should only be used by one players so we remove the interactable components for everyone else
                if (_localPlayer.displayName == _vrchatUsernameThatOneUser.Trim())
                {
                    SetLocalPlayerAsPicker(true);
                }
                disableInteractions = true;
            }
            else if (_restrictToWhiteList)
            {
                disableInteractions = true;
                //this script should only be used by everyone in the whitelist so we remove the interactable components for everyone else
                if (_whiteList == null || _whiteList.Length == 0)
                {
                    Debug.LogError("[LIFTUP] Restricted to whitelist is selected but no whitelist is provided");
                }
                else
                {
                    string userName = _localPlayer.displayName;
                    for (int i = 0; i < _whiteList.Length; i++)
                    {
                        if (_whiteList[i].Trim() == userName)
                        {
                            disableInteractions = false;
                            _isWhitelistedUser = true;
                            if (_enableOnWhitelistedJoin)
                            {
                                SetLocalPlayerAsPicker(true);
                            }
                            break;
                        }
                    }
                }
            }
            //disables the mesh collider so that Interact() can't be triggered anymore
            if (disableInteractions)
            {
                Collider[] colliderArray = this.gameObject.GetComponents<Collider>();
                if (colliderArray != null)
                    foreach (Collider collider in colliderArray) { collider.enabled = false; }
                MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
                //making the mesh invisible if that option is selected and the user can't use this feature
                if (_disableMeshRenderer && meshRenderer != null && !_pickerOwnerClaimed)
                    meshRenderer.enabled = false;
                Debug.Log("[LIFTUP] All interactions disabled successfully");
            }
        }
        /// <summary>
        /// This runs only for PLAYER_PICKER to check if the pick hand trigger is being pressed
        /// </summary>
        private void Update()
        {
            if (_isLocked && Time.timeSinceLevelLoad - _timeWhenClaimed > _lockTimeWhenClaimed)
            {
                SetButtonMaterial(_buttonMaterialUnclaimed);
                _isLocked = false;
            }
            if (_pickerOwnerClaimed)
            {
                //check left trigger
                bool triggerPressedL = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") == 1f;
                if (!_isButtonPressedLocalCopyL && triggerPressedL)
                {
                    _isButtonPressedLocalCopyL = true;
                    SendPickerEvent(nameof(ButtonPressedL));
                }
                else if (_isButtonPressedLocalCopyL && !triggerPressedL)
                {
                    _isButtonPressedLocalCopyL = false;
                    SendPickerEvent(nameof(ButtonReleasedL));
                }
                //check right trigger
                bool triggerPressedR = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") == 1f;
                if (!_isButtonPressedLocalCopyR && triggerPressedR)
                {
                    _isButtonPressedLocalCopyR = true;
                    SendPickerEvent(nameof(ButtonPressedR));
                }
                else if (_isButtonPressedLocalCopyR && !triggerPressedR)
                {
                    _isButtonPressedLocalCopyR = false;
                    SendPickerEvent(nameof(ButtonReleasedR));
                }
                //there is nothing else to do for the PLAYER_PICKER
                return;
            }
            //Every other user must process changes on the button states from PLAYER_PICKER at any given time
            ProcessButtonChangesFromPicker();
            if (_playerIsHeld)
            {
                if (_allowPlayerToEscape && CheckIfPlayerWantsToEscape())
                {
                    _playerIsHeld = false;
                    return;
                }
                Vector3 pickHandPos = _playerIsHeldByHandR ? _pickerPlayer.GetBonePosition(HumanBodyBones.RightHand) : _pickerPlayer.GetBonePosition(HumanBodyBones.LeftHand);
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
        /// <summary>
        /// External and internal function to set localplayer to be the current picker player with <see cref="setClaimed"/> true.
        /// </summary>
        private void SetLocalPlayerAsPicker(bool setClaimed)
        {
            if (setClaimed)
            {
                Networking.SetOwner(_localPlayer, this.gameObject);
                _pickerOwnerClaimed = true;
                SetButtonMaterial(_buttonMaterialClaimed);
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(SetClaimedByNetwork));
            }
            else
            {
                _pickerOwnerClaimed = false;
                SetButtonMaterial(_buttonMaterialUnclaimed);
            }
        }
        /// <summary>
        /// Allows the network to detach localPlayer from picker if they are being held
        /// </summary>
        public void RemoteDetachPlayer()
        {
            _playerIsHeld = false;
        }
        /// <summary>
        /// Network function which is called when someone else claims to be picker so that
        /// this function is locked for a certain time for everyone else
        /// </summary>
        public void SetClaimedByNetwork()
        {
            if (!_pickerOwnerClaimed)
            {
                _timeWhenClaimed = Time.timeSinceLevelLoad;
                _isLocked = true;
                SetButtonMaterial(_buttonMaterialLocked);
            }
        }
        /// <summary>
        /// Returns true if localPlayer is currently the PICKER_PLAYER
        /// </summary>
        public bool GetIfLocalPlayerIsPicker()
        {
            return _pickerOwnerClaimed;
        }
        /// <summary>
        /// Distributing a picker event over network only if the localPlayer is still owner of this script
        /// </summary>
        private void SendPickerEvent(string functionName)
        {
            if (MakeSurePlayerIsStillOwner())
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, functionName);
            }
        }
        /// <summary>
        /// Checking if localPlayer is still owner of this script
        /// </summary>
        /// <returns></returns>
        private bool MakeSurePlayerIsStillOwner()
        {
            if (_localPlayer != Networking.GetOwner(this.gameObject))
            {
                //in the special case that the script is locked to one user, we can force-assign ownership in this case
                if (_restrictToOneUser)
                {
                    Networking.SetOwner(_localPlayer, this.gameObject);
                    return true;
                }
                SetLocalPlayerAsPicker(false);
                return false;
            }
            return true;
        }
        /// <summary>
        /// This script runs for everyone except PLAYER_PICKER to handle the button states
        /// </summary>
        public void ProcessButtonChangesFromPicker()
        {
            if (!_playerIsHeld || _playerIsHeldByHandR)
            {
                if (!_isButtonPressedLocalCopyR && _isButtonPressedNetworkCopyR)
                {
                    _isButtonPressedLocalCopyR = true;
                    //read current owner of this script
                    VRCPlayerApi currentOwner = Networking.GetOwner(this.gameObject);
                    //button was pressed by master, time to calculate their hand position
                    if (CheckIfPlayerIsHeld(currentOwner.GetBonePosition(HumanBodyBones.RightHand)))
                    {
                        SetPlayerIsHeld(pickerPlayer: currentOwner, isRightHand: true);
                    }
                }
                else if (_isButtonPressedLocalCopyR && !_isButtonPressedNetworkCopyR)
                {
                    //button was released by master (PLAYER_PICKER stopped picking me up)
                    _isButtonPressedLocalCopyR = false;
                    if (_playerIsHeld)
                        _localPlayer.SetVelocity(((_positionOneFrameAgo - _positionTwoFramesAgo) / _deltaTimeOneFrameAgo) * _yeetForceMultiplier);
                    _playerIsHeld = false;
                    if (_addFakeGround)
                        _fakeGroundObj.SetActive(false);
                }
            }
            //the right hand is dominant, so left hand can't hold it as well at the same time
            if (!_playerIsHeld || !_playerIsHeldByHandR)
            {
                if (!_isButtonPressedLocalCopyL && _isButtonPressedNetworkCopyL)
                {
                    _isButtonPressedLocalCopyL = true;
                    //read current owner of this script
                    VRCPlayerApi currentOwner = Networking.GetOwner(this.gameObject);
                    //button was pressed by master, time to calculate their hand position
                    if (CheckIfPlayerIsHeld(currentOwner.GetBonePosition(HumanBodyBones.LeftHand)))
                    {
                        SetPlayerIsHeld(pickerPlayer: currentOwner, isRightHand: false);
                    }
                }
                else if (_isButtonPressedLocalCopyL && !_isButtonPressedNetworkCopyL)
                {
                    //button was released by master (PLAYER_PICKER stopped picking me up)
                    _isButtonPressedLocalCopyL = false;
                    if (_playerIsHeld)
                        _localPlayer.SetVelocity(((_positionOneFrameAgo - _positionTwoFramesAgo) / _deltaTimeOneFrameAgo) * _yeetForceMultiplier);
                    _playerIsHeld = false;
                    if (_addFakeGround)
                        _fakeGroundObj.SetActive(false);
                }
            }
        }
        /// <summary>
        /// If the pick hand is close enough to one bone of localplayer, this player is being held from there on
        /// </summary>
        public bool CheckIfPlayerIsHeld(Vector3 pickHandPos)
        {
            //player too far away to be picked
            if (Vector3.Distance(pickHandPos, _localPlayer.GetPosition()) > 5f)
                return false;
            //player in reach, need to check all bones
            for (int i = 0; i < _bodyBones.Length; i++)
            {
                if (CheckDistanceToBone(pickHandPos, (HumanBodyBones)_bodyBones[i]))
                    return true; //one bone is enough
            }
            Debug.Log("[LIFTUP] PLAYER_PICKER hand not in distance");
            return false;
        }
        /// <summary>
        /// Sets the localPlayer to be held by the picker player with their specified hand
        /// </summary>
        private void SetPlayerIsHeld(VRCPlayerApi pickerPlayer, bool isRightHand)
        {
            if (pickerPlayer == _localPlayer)
                return; //security check, should never happen
            _playerIsHeld = true;
            //detach player from all other script instances if there are any
            if (_otherScriptInstances != null)
                for (int i = 0; i < _otherScriptInstances.Length; i++)
                    _otherScriptInstances[i].RemoteDetachPlayer();
            _pickerPlayer = pickerPlayer;
            _playerIsHeldByHandR = isRightHand;
            HumanBodyBones pickerHand = isRightHand ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            _startPickHeight = _pickerPlayer.GetBonePosition(pickerHand).y;
            _startPickPos = _pickerPlayer.GetBonePosition(pickerHand);
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
        /// Checking if the player tries to move away while being held
        /// </summary>
        private bool CheckIfPlayerWantsToEscape()
        {
            if (_userIsInVR)
            {
                //using the left thumbstick to move is recognized as trying to escape
                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.3f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.3f)
                    return true;
            }
            else
            {
                //pressing any keyboard walking key is recognizes as trying to escape
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Checking the distance of a point against one player bone
        /// </summary>
        public bool CheckDistanceToBone(Vector3 point, HumanBodyBones bone)
        {
            return Vector3.Distance(point, _localPlayer.GetBonePosition(bone)) <= _minDistanceToAnyBone;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is pressed
        /// </summary>
        public void ButtonPressedL()
        {
            if (_pickerOwnerClaimed && MakeSurePlayerIsStillOwner())
                return;
            //PLAYER_PICKER pressed left button
            _isButtonPressedNetworkCopyL = true;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is released
        /// </summary>
        public void ButtonReleasedL()
        {
            if (_pickerOwnerClaimed && MakeSurePlayerIsStillOwner())
                return;
            //PLAYER_PICKER released left button
            _isButtonPressedNetworkCopyL = false;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is pressed
        /// </summary>
        public void ButtonPressedR()
        {
            if (_pickerOwnerClaimed && MakeSurePlayerIsStillOwner())
                return;
            //PLAYER_PICKER pressed right button
            _isButtonPressedNetworkCopyR = true;
        }
        /// <summary>
        /// Is called by PLAYER_PICKER for all players when the pick hand trigger is released
        /// </summary>
        public void ButtonReleasedR()
        {
            if (_pickerOwnerClaimed && MakeSurePlayerIsStillOwner())
                return;
            //PLAYER_PICKER released right button
            _isButtonPressedNetworkCopyR = false;
        }
        /// <summary>
        /// This script should stop to run when the PLAYER_PICKER left this instance
        /// </summary>
        override public void OnPlayerLeft(VRCPlayerApi leftPlayerApi)
        {
            if (leftPlayerApi == _pickerPlayer)
            {
                //PLAYER_PICKER left the instance
                _playerIsHeld = false;
                _pickerPlayer = null;
            }
        }
        private RaycastHit _hit;
        private const float _maxRaycastDistance = 15;
        private const int _layerMask = 0b00000000_00000000_00000000_00000000; //default layer on which the ground collider should be
        /// <summary>
        /// Calculating the ground height under the head of localPlayer
        /// Returns 0f when there is no ground. Only non-trigger colliders on the default layer are recognized as ground.
        /// </summary>
        public float GetGroundHeight()
        {
            Vector3 playerHeadPos = _localPlayer.GetBonePosition(HumanBodyBones.Head);
            if (Physics.Raycast(playerHeadPos, Vector3.down, out _hit, _maxRaycastDistance, _layerMask))
            {
                return _hit.point.y;
            }
            else
            {
                return 0f;
            }
        }
        /// <summary>
        /// Claim to be the picker
        /// </summary>
        public override void Interact()
        {
            if (_isLocked)
                return;
            if (_restrictToOneUser || (_restrictToWhiteList && !_isWhitelistedUser))
                return;
            if (_otherScriptInstances != null)
                for (int i = 0; i < _otherScriptInstances.Length; i++)
                    if (_otherScriptInstances[i].GetIfPickerOwnerClaimed()) { return; }
            SetLocalPlayerAsPicker(true);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ButtonReleasedL));
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ButtonReleasedR));
        }
        /// <summary>
        /// Public access to check if picker owner is claimed on that script instance
        /// </summary>
        public bool GetIfPickerOwnerClaimed()
        {
            return _pickerOwnerClaimed;
        }
        /// <summary>
        /// Changing the material of the button to <see cref="newMaterial"/>
        /// </summary>
        public void SetButtonMaterial(Material newMaterial)
        {
            Material[] materials = _buttonMaterialRenderer.materials;
            materials[_buttonMaterialIndex] = newMaterial;
            _buttonMaterialRenderer.materials = materials;
        }
        /// <summary>
        /// Each time ownership changed, we check if we are still owner in case we are PLAYER_PICKER. 
        /// Warning: This function gets called multiple times and is not safe itself, so we do additional checks on button presses as well.
        /// </summary>
        public void OnOwnerTransferred()
        {
            if (_pickerOwnerClaimed)
            {
                if (MakeSurePlayerIsStillOwner())
                    return;
            }
            VRCPlayerApi newOwner = Networking.GetOwner(this.gameObject);
            if (newOwner.isMaster)
            {
                //this means the object just got transfered to the instance master which could mean it
                //was transfered automaticly and not claimed by anyone.
                if (newOwner == _localPlayer)
                {
                    //the instance master should therefor handle this situation and set the synced claimed to none
                    //this is just a prototype function, I don't want to have a synced claim since synced variables are expensive.
                    //if any customer wants that feature, it can easily be added here. I added the timed lock for now instead.
                }
            }
        }
    }
}