
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using ReimajoBoothAssets;
using UnityEngine.Rendering;

public class MainMenu : UdonSharpBehaviour
{
    public Toggle bgmToggle;
    public Toggle grassToggle;
    public Toggle bloomToggle;
    public Toggle liftToggle;
    public Toggle dayToggle;

    public GameObject terrain;
    public GameObject bgm;
    public GameObject postPrcocess;

    public Material daySkyBox;
    public Color dayFog;
    public GameObject dayLight;
    public Material nightSkyBox;
    public Color nightFog;
    public GameObject nightLight;

    public ReflectionProbe reflectionProbe;

    public PickingAvatarsUp pickingAvatarsUp;

    private Animator terrainAnimator;

    void Start()
    {
        terrainAnimator = terrain.GetComponent<Animator>();
    }

    public void OnToggleChange()
    {
        bgm.SetActive(bgmToggle.isOn);
        terrainAnimator.SetBool("hasGrass", grassToggle.isOn);
        postPrcocess.SetActive(bloomToggle.isOn);
        pickingAvatarsUp.setCanBePickUp(liftToggle.isOn);

        if (dayToggle.isOn)
        {
            RenderSettings.skybox = daySkyBox;
            RenderSettings.fogColor = dayFog;
            dayLight.SetActive(true);
            nightLight.SetActive(false);
            
        }
        else
        {
            RenderSettings.skybox = nightSkyBox;
            RenderSettings.fogColor = nightFog;
            dayLight.SetActive(false);
            nightLight.SetActive(true);
        }
        reflectionProbe.RenderProbe();
    }
}
