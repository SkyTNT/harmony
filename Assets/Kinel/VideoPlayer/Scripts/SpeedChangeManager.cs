
using Kinel.VideoPlayer.Scripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpeedChangeManager : UdonSharpBehaviour
{

    [SerializeField] private KinelVideoScript player;
    [SerializeField] private float speed = 1f;

    public override void Interact()
    {
        //player.SpeedChange(speed);
        Debug.Log("[VideoPlayer] Intracted");
    }
}
