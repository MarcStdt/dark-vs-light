using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{

    [SerializeField] SceneLoader sceneLoader = default;
    [SerializeField] AudioMixer audioMixer = default;

    [SerializeField] Slider musicSlider = default;
    [SerializeField] Slider effectSlider = default;

    void Start()
    {
        GameObject.Find("Start").GetComponent<Button>().onClick.AddListener(StartGame);
        GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitGame);
        musicSlider.onValueChanged.AddListener(SetMusic);
        effectSlider.onValueChanged.AddListener(SetEffect);
    }


    void ExitGame()
    {
        Application.Quit();
    }
    void StartGame()
    {
        sceneLoader.nextScene();
    }


    void SetMusic(float vol)
    {
        audioMixer.SetFloat("volMusic", vol);
    }

    void SetEffect(float vol)
    {
        audioMixer.SetFloat("volEffect", vol);
    }
}
