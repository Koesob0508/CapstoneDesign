using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingNavigator : Navigator
{
    public List<Vector3> startPoints; // ���� ��ġ List
    public Vector3 assistStartPoint;
    public List<Vector3> parkingPoints; // ���� ������ ��ġ List
    public Vector3 assistParkingPoint;
    public List<Vector3> entrancePoints; // ������ �Ա� ��ġ List
    public Vector3 assistEntrancePoint;
    public Vector3 crossPoint;
    public List<int> restCarIndices; // Target ��ġ ���� �� ������ ������ ��ġ List
    public List<GameObject> carPrefabs; // ������ ĭ�� �� ���� Prefabs
    public List<GameObject> restCarList; // ������ ������ ������ List

    public int targetIndex; // Target�� �� ���� ��ġ Index
    public int restCarNumber; // ������ ������ �� ���� ���� ������
    public int startIndex;
    public float[,] dijkstraMatrix;
    public List<Vector3> routeList;


    private void Start()
    {
        restCarIndices = new List<int>();
        restCarList = new List<GameObject>();
        dijkstraMatrix = new float[entrancePoints.Count, parkingPoints.Count];
        routeList = new List<Vector3>();
        dijkstraMatrix = SetDijkstra();
    }

    /// <summary>
    /// Episode ���۽�, ���� ��ġ ���� �� ��ǥ ��ġ ���� �� ������ ������ ��ġ ����
    /// </summary>
    public override void OnNavigator()
    {
        restCarNumber = Random.Range(parkingPoints.Count / 2, parkingPoints.Count - 1);


        SetStartPosition();
        targetIndex = Random.Range(0, parkingPoints.Count);
        routeList = StartAStar(targetIndex);
        SetRestPosition(targetIndex, restCarNumber);
        SetTargetPosition(routeList);
    }

    public override void MoveNextPosition(bool _isGoal)
    {
        testAgent.GetComponent<TestAgent>().SetReward(0.1f);

        if(routeList.Count > 0)
        //if (routeList.Count > 0 && !_isGoal)
        {
            Debug.Log("���� ���� : " + routeList.Count);

            SetTargetPosition(routeList);
        }
        //else if (routeList.Count==0 && _isGoal)
        else if(routeList.Count==0)
        {
            Debug.Log("����");
            testAgent.GetComponent<TestAgent>().EndEpisode();
        }
    }

    private void SetStartPosition()
    {
        startIndex = Random.Range(0, startPoints.Count);
        //Vector3 startPosition = routeList[0];

        // testAgent.transform.localPosition = startPosition + new Vector3(3f, 0, 0);
        if(startIndex == 0)
        {
            testAgent.transform.localPosition = startPoints[0];
            testAgent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            testAgent.transform.localPosition = startPoints[1];
            testAgent.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }



    }

    /// <summary>
    /// Target�� ��ġ ����
    /// </summary>
    /// <returns>target�� ������ ��ġ Index</returns>
    private void SetTargetPosition(List<Vector3> _routeList)
    {
        Vector3 targetPosition = _routeList[0];
        _routeList.Remove(targetPosition);

        target.transform.localPosition = targetPosition;

        int isForward = Random.Range(0, 2);

        if (isForward == 1)
        {
            target.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else
        {
            target.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
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
        while (restCarIndices.Count < _restCarNumber)
        {
            int index = Random.Range(0, parkingPoints.Count);

            if (index == _targetIndex)
            {
                continue;
            }
            else
            {
                if (restCarIndices.Contains(index))
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
        foreach (int index in restCarIndices)
        {
            int prefabIndex = Random.Range(0, carPrefabs.Count);
            Vector3 prefabPosition = parkingPoints[index];
            Quaternion prefabRotation;

            int isForward = Random.Range(0, 2);

            if (isForward == 1)
            {
                prefabRotation = Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                prefabRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            GameObject restCar = Instantiate(carPrefabs[prefabIndex], this.transform.parent);
            restCar.transform.localPosition = prefabPosition;
            restCar.transform.rotation = prefabRotation;
            restCarList.Add(restCar);
        }
    }

    private float[,] SetDijkstra()
    {
        float[,] resultMatrix = new float[entrancePoints.Count, parkingPoints.Count];

        for (int i = 0; i < entrancePoints.Count; i++)
        {
            for (int j = 0; j < parkingPoints.Count; j++)
            {
                Vector3 norm = parkingPoints[j] - entrancePoints[i];
                resultMatrix[i, j] = Mathf.Abs(norm.x) + Mathf.Abs(norm.z);
                // Debug.Log("i : " + i + "j : " + j + "result : " + resultMatrix[i, j]);
            }
        }

        return resultMatrix;
    }

    private List<Vector3> StartAStar(int _targetIndex)
    {
        List<Vector3> resultRoute = new List<Vector3>();

        // �̹����� �Ա� Vector3�� ���� ���̴�.

        float[] entranceDistance = new float[entrancePoints.Count];
        float distance = float.PositiveInfinity;
        float currentDistance;
        Vector3 selectedEntrance = Vector3.zero;

        for (int index = 0; index < entrancePoints.Count; index++)
        {
            entranceDistance[index] = Vector3.Distance(entrancePoints[index], testAgent.transform.position);
        }

        for (int index = 0; index < entranceDistance.Length; index++)
        {
            currentDistance = entranceDistance[index] + dijkstraMatrix[index, targetIndex];

            if (distance > currentDistance)
            {
                distance = currentDistance;
                selectedEntrance = entrancePoints[index];
            }
        }

        //if(startIndex == 0)
        //{
        //    assistStartPoint = startPoints[0] + new Vector3(0, 0, 14);
        //    resultRoute.Add(assistStartPoint);
        //}
        //else
        //{
        //    assistStartPoint = startPoints[1] + new Vector3(0, 0, -14);
        //    resultRoute.Add(assistStartPoint);
        //}

        assistEntrancePoint = selectedEntrance + new Vector3(2, 0, 0);
        resultRoute.Add(assistEntrancePoint);
        resultRoute.Add(selectedEntrance);
        crossPoint = selectedEntrance + new Vector3(-7, 0, 0);
        resultRoute.Add(crossPoint);
        if(parkingPoints[targetIndex].x > 0)
        {
            assistParkingPoint = parkingPoints[targetIndex] + new Vector3(-4, 0, 0);
            resultRoute.Add(assistParkingPoint);
            resultRoute.Add(parkingPoints[targetIndex] + new Vector3(2, 0, 0));
        }
        else
        {
            assistParkingPoint = parkingPoints[targetIndex] + new Vector3(4, 0, 0);
            resultRoute.Add(assistParkingPoint);
            resultRoute.Add(parkingPoints[targetIndex] + new Vector3(-2, 0, 0));
        }
        

        return resultRoute;
    }
}
