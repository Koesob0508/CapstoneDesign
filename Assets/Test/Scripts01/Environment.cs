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

    
}
