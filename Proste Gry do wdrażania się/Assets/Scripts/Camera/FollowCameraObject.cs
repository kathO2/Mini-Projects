using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraObject : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] float flipRotationTime = 0.5f;

    void Update()
    {
        transform.position = playerTransform.position;
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipRotationTime).setEaseInOutSine();
    }

    float DetermineEndRotation()
    {
        if (playerTransform.localScale.x < 0)
        {
            return 180;
        }
        else
        {
            return 0;
        }
    }
}
