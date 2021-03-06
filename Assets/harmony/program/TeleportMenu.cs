
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeleportMenu : UdonSharpBehaviour
{
    public Transform anchor1;
    public Transform anchor2;
    void Start()
    {
        
    }

    public void Click1()
    {
        teleport(anchor1);
    }

    public void Click2()
    {
        teleport(anchor2);
    }

    public void teleport(Transform anchor)
    {
        Networking.LocalPlayer.TeleportTo(anchor.position,
                                          anchor.rotation,
                                          VRC_SceneDescriptor.SpawnOrientation.Default,
                                          false);
    }
}
