using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Ball")]
    public GameObject ball;

    [Header("Player 1")]
    public GameObject player1;

    [Header("Player 2")]
    public GameObject player2;

    [Header("Score UI")]
    public TextMeshProUGUI Player1Text;
    public TextMeshProUGUI Player2Text;

    int Player1Score;
    int Player2Score;

    public void Player1Scored()
    {
        Player1Score++;
        Player1Text.GetComponent<TextMeshProUGUI>().text = Player1Score.ToString();
        ResetPosition();
    }

    public void Player2Scored()
    {
        Player2Score++;
        Player2Text.GetComponent<TextMeshProUGUI>().text = Player2Score.ToString();
        ResetPosition();
    }

    void ResetPosition()
    {
        ball.GetComponent<Ball>().Reset();
        player1.GetComponent<PlayersMovement>().Reset();
        player2.GetComponent<PlayersMovement>().Reset();
    }

}
