using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject titlePage;
    [SerializeField] private Text highscoreNum;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        manager.OnReturnToTitle += ReturnToTitle;
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        manager.OnReturnToTitle -= ReturnToTitle;
        startButton.onClick.RemoveListener(StartGame);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    private void Start()
    {
        BuildStartScene();
    }

    public void BuildStartScene()
    {
        highscoreNum.text = manager.HighScoreNow.ToString();
    }

    public void StartGame()
    {
        titlePage.SetActive(false);
        manager.InitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ReturnToTitle()
    {
        BuildStartScene();
        titlePage.SetActive(true);
    }
}
