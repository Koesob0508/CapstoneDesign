using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float RewardSum;

    private void Awake()
    {
        RewardSum = 0f;
    }

    private void Update()
    {
        RewardSum -= 0.01f * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Goal"))
        {
            RewardSum += 1f;
            Debug.Log("골인");
        }
        
        if(other.gameObject.CompareTag("Obstacle"))
        {
            RewardSum -= 1f;
            Debug.Log("교통 사고");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("CenterLine"))
        {
            RewardSum -= 1f * Time.deltaTime;
            Debug.Log("중앙선 침범");
        }
    }
}
