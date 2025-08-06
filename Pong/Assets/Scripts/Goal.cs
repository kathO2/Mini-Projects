using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isPlayer1Goal;

    GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            if (!isPlayer1Goal)
            {
                Debug.Log("Player 2 Scored");
                gameManager.Player2Scored();
            }
            else
            {
                Debug.Log("Player 1 Scored");
                gameManager.Player1Scored();
            }
        }
    }
}
