
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class MainMenu : UdonSharpBehaviour
{
    public Toggle bgmToggle;
    public Toggle grassToggle;
    public Toggle bloomToggle;

    public GameObject terrain;
    public GameObject bgm;
    public GameObject postPrcocess;

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
    }
}
