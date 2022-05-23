using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingNavigator : Navigator
{
    
    public List<Vector3> startPoints; // 시작 위치 List
    public List<Vector3> parkingPoints; // 주차 가능한 위치 List
    public List<int> restCarIndices; // Target 위치 지정 후 차량들 세워질 위치 List
    public List<GameObject> carPrefabs; // 나머지 칸에 들어갈 차량 Prefabs
    public List<GameObject> restCarList; // 나머지 차량들 관리용 List

    public int targetIndex; // Target이 들어갈 주차 위치 Index
    public int restCarNumber; // 나머지 차량을 몇 개나 세울 것인지

    private void Start()
    {
        restCarIndices = new List<int>();
        restCarList = new List<GameObject>();
    }

    /// <summary>
    /// Episode 시작시, 시작 위치 지정 → 목표 위치 지정 → 나머지 차량들 위치 지정
    /// </summary>
    public override void OnNavigator()
    {
        restCarNumber = Random.Range(parkingPoints.Count / 2, parkingPoints.Count - 1);

        SetStartPosition();
        targetIndex = SetTargetPosition();
        SetRestPosition(targetIndex, restCarNumber);
    }

    public override void MoveNextPosition()
    {
        this.OnNavigator();
    }

    private void SetStartPosition()
    {
        int index = Random.Range(0, startPoints.Count);
        Vector3 startPosition = startPoints[index];

        testAgent.transform.localPosition = startPosition;
        testAgent.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    /// <summary>
    /// Target의 위치 지정
    /// </summary>
    /// <returns>target이 세워질 위치 Index</returns>
    private int SetTargetPosition()
    {
        int index = Random.Range(0, parkingPoints.Count);
        Vector3 targetPosition = parkingPoints[index];

        target.transform.localPosition = targetPosition;

        int isForward = Random.Range(0, 2);

        if(isForward == 1)
        {
            target.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else
        {
            target.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        return index;
    }

    /// <summary>
    /// 나머지 세워질 차량 위치 지정
    /// </summary>
    /// <param name="_targetIndex">이미 세워진 target의 index는 제외하기 위함</param>
    /// <param name="_restCarNumber">차량 갯수 임의로 정할 수 있음</param>
    private void SetRestPosition(int _targetIndex, int _restCarNumber)
    {
        restCarIndices.Clear();
        foreach (GameObject preCar in restCarList)
        {
            Destroy(preCar);
        }
        restCarList.Clear();

        // 나머지 차량이 들어갈 위치 List 생성
        while(restCarIndices.Count < _restCarNumber)
        {
            int index = Random.Range(0, parkingPoints.Count);

            if (index == _targetIndex)
            {
                continue;
            }
            else
            {
                if(restCarIndices.Contains(index))
                {
                    continue;
                }
                else
                {
                    restCarIndices.Add(index);
                }
            }
        }

        // 위에서 생성한 위치 List에 차량 Prefab을 랜덤하게 생성
        foreach(int index in restCarIndices)
        {
            int prefabIndex = Random.Range(0, carPrefabs.Count);
            Vector3 prefabPosition = parkingPoints[index];
            Quaternion prefabRotation;

            int isForward = Random.Range(0, 2);

            if(isForward == 1)
            {
                prefabRotation = Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                prefabRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            GameObject restCar = Instantiate(carPrefabs[prefabIndex], prefabPosition, prefabRotation, this.transform.parent);
            restCarList.Add(restCar);
        }
    }
}
