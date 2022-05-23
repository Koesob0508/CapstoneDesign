using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingNavigator : Navigator
{
    
    public List<Vector3> startPoints; // ���� ��ġ List
    public List<Vector3> parkingPoints; // ���� ������ ��ġ List
    public List<int> restCarIndices; // Target ��ġ ���� �� ������ ������ ��ġ List
    public List<GameObject> carPrefabs; // ������ ĭ�� �� ���� Prefabs
    public List<GameObject> restCarList; // ������ ������ ������ List

    public int targetIndex; // Target�� �� ���� ��ġ Index
    public int restCarNumber; // ������ ������ �� ���� ���� ������

    private void Start()
    {
        restCarIndices = new List<int>();
        restCarList = new List<GameObject>();
    }

    /// <summary>
    /// Episode ���۽�, ���� ��ġ ���� �� ��ǥ ��ġ ���� �� ������ ������ ��ġ ����
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
    /// Target�� ��ġ ����
    /// </summary>
    /// <returns>target�� ������ ��ġ Index</returns>
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
    /// ������ ������ ���� ��ġ ����
    /// </summary>
    /// <param name="_targetIndex">�̹� ������ target�� index�� �����ϱ� ����</param>
    /// <param name="_restCarNumber">���� ���� ���Ƿ� ���� �� ����</param>
    private void SetRestPosition(int _targetIndex, int _restCarNumber)
    {
        restCarIndices.Clear();
        foreach (GameObject preCar in restCarList)
        {
            Destroy(preCar);
        }
        restCarList.Clear();

        // ������ ������ �� ��ġ List ����
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

        // ������ ������ ��ġ List�� ���� Prefab�� �����ϰ� ����
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
