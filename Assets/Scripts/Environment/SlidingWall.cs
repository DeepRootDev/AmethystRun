using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingWall : ActiveObject
{
    public Transform pointOne;
    public Transform pointTwo;
    [SerializeField] private bool isOpen = false;
    public float timeToMove = 1f;

    public override void Activate()
    {
        if(isOpen)
        {
            StartCoroutine(MoveWall(pointOne.position));
        }
        else
        {
            StartCoroutine(MoveWall(pointTwo.position));
        }
    }

    IEnumerator MoveWall(Vector3 targetPos)
    {
        float countTime = 0f;

        while (countTime < timeToMove)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, countTime / timeToMove);
            countTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPos;
        isOpen = !isOpen;
        yield return null;
    }
}
