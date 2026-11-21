using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] public GameObject _menuPanel;
    [SerializeField] public GameObject _creditsPanel;

    private void Start()
    {
        _menuPanel.SetActive(true);
        _creditsPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Credits()
    {
        _menuPanel.SetActive(false);
        _creditsPanel.SetActive(true);
    }

    public void Menu()
    {
        _menuPanel.SetActive(true);
        _creditsPanel.SetActive(false);
    }
}
