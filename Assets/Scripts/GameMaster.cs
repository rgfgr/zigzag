using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public GameObject player, addScore, leaderboard;
    public static GameMaster instance;
    public Button tryAgain, quitGame;
    public TMP_Text timeUi, pointUi;
    public float time = 3;
    public int Points => points;

    private bool started = false;
    private int points = 0;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        addScore.SetActive(false);
        leaderboard.SetActive(false);
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        while (!started)
        {
            time -= 0.1f;
            timeUi.text = $"Time: {time:g2}";
            if (time <= 0)
            {
                timeUi.gameObject.SetActive(false);
                pointUi.gameObject.SetActive(true);
                player.GetComponent<Player>().enabled = true;
                player.GetComponent<Rigidbody>().useGravity = true;
                started = true;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void UpdatePoints(int amount)
    {
        points += amount;
        pointUi.text = $"Points: {points}";
    }

    public void EndGame()
    {
        timeUi.text = "You Died!";

        var alphaChange = timeUi.color;
        alphaChange.a = 0;
        timeUi.color = alphaChange;

        tryAgain.gameObject.SetActive(true);
        quitGame.gameObject.SetActive(true);
        addScore.SetActive(true);
        leaderboard.SetActive(true);

        timeUi.gameObject.SetActive(true);

        StartCoroutine(FadeIn());

        StartCoroutine(LoadBoard());
    }
    IEnumerator LoadBoard()
    {
        while(true)
        {
            FirebaseManager.Instance.LoadBoard();
            yield return new WaitForSecondsRealtime(5);
        }
    }

    IEnumerator FadeIn()
    {
        for (float i = 0; i < 2; i += Time.deltaTime)
        {
            var alphaChange = timeUi.color;
            alphaChange.a += i / 2;
            timeUi.color = alphaChange;
            yield return null;
        }
    }

    void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void LoadGame() => LoadScene("Game Scene");
    public void LoadMain() => LoadScene("Main menu");
}
