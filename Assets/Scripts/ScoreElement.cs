using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreElement : MonoBehaviour
{
    public TMP_Text usernameText, scoreText;

    public void NewScoreElement(string _username, int _score)
    {
        usernameText.text = _username;
        scoreText.text = _score.ToString();
    }
}
