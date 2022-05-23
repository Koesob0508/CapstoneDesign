using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FindingAgent : Agent
{
    public Rigidbody carRigidbody;
    public GameObject goal;
    public List<GameObject> parkingBlock;
    public CarController carController;

    // 8 방향을 커스텀하기 위한 지점
    public GameObject stoneFront;
    public GameObject stoneFR;
    public GameObject stoneRight;
    public GameObject stoneBR;
    public GameObject stoneBack;
    public GameObject stoneBL;
    public GameObject stoneLeft;
    public GameObject stoneFL;

    // 관리하기 편하기 위한 List 형태
    public List<GameObject> stoneList;
    public List<Vector3> parkingList;

    // 8 방향에 대한 Ray
    private Ray rayFront;
    private Ray rayFR;
    private Ray rayRight;
    private Ray rayBR;
    private Ray rayBack;
    private Ray rayBL;
    private Ray rayLeft;
    private Ray rayFL;

    // Ray 최대 거리
    public float rayDistance;
    public float accel;
    public Vector2 toTarget;
    public Vector2 fromObstacle;

    // Obstacle 리스트
    public List<Vector3> sortedObjects;
    // RayCast를 위한 변수들
    private RaycastHit raycastHit;

    public float goalRadius;

    public float obstacleCount;

    private void Start()
    {
        carRigidbody = this.GetComponent<Rigidbody>();
        carController = this.GetComponent<CarController>();

        // CarController carController = this.gameObject.GetComponent<CarController>();
        stoneList = new List<GameObject>();
        // Stone들을 stonList에 추가
        stoneList.Add(stoneFront);
        stoneList.Add(stoneFR);
        stoneList.Add(stoneRight);
        stoneList.Add(stoneBR);
        stoneList.Add(stoneBack);
        stoneList.Add(stoneBL);
        stoneList.Add(stoneLeft);
        stoneList.Add(stoneFL);

        sortedObjects = new List<Vector3>();
    }

    //private void FixedUpdate()
    //{
    //    DisplayRay();

    //}

    public override void OnEpisodeBegin()
    {
        this.gameObject.transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5, 5f));
        this.gameObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        carRigidbody.velocity = Vector3.zero;

        int parkIndex = Random.Range(0, parkingList.Count);
        int blocIndex = 0;

        for(int index=0; index<parkingList.Count; index++)
        {
            if(index == parkIndex)
            {
                goal.transform.localPosition = parkingList[parkIndex];
            }
            else
            {
                parkingBlock[blocIndex].transform.localPosition = parkingList[index] - new Vector3(1, 0, 0);
                blocIndex += 1;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        CastRay();

        Vector2 goalXZ = new Vector2(goal.transform.position.x, goal.transform.position.z);
        Vector2 positionXZ = new Vector2(this.transform.position.x, this.transform.position.z);
        toTarget = goalXZ - positionXZ;
        sensor.AddObservation(toTarget.normalized); // 2

        float relativeDistance = toTarget.magnitude / goalRadius;
        sensor.AddObservation(relativeDistance); // 1

        sensor.AddObservation(carRigidbody.velocity.x); // 1
        sensor.AddObservation(carRigidbody.velocity.z); // 1

        Vector2 rotationNomal = new Vector2(this.transform.rotation.normalized.y, this.transform.rotation.normalized.w);
        sensor.AddObservation(rotationNomal); // 2

        fromObstacle = Vector2.zero;

        obstacleCount = sortedObjects.Count;

        foreach(Vector3 sorted in sortedObjects)
        {
            fromObstacle.x += rayDistance - (positionXZ.x - sorted.x);
            fromObstacle.y += rayDistance - (positionXZ.y - sorted.z);
        }

        if(sortedObjects.Count > 0)
        {
            fromObstacle /= sortedObjects.Count;
        }

        sensor.AddObservation(fromObstacle); // 2
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.horizontalInput = actions.ContinuousActions[0];
        carController.verticalInput = actions.ContinuousActions[1];
        if (actions.ContinuousActions[2] > 0)
        {
            carController.currentBrake = actions.ContinuousActions[2];
            carController.isBraking = true;
        }
        else
        {
            carController.isBraking = false;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionsOut = actionsOut.ContinuousActions;
        continousActionsOut[0] = Input.GetAxis("Horizontal");
        continousActionsOut[1] = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Space))
        {
            continousActionsOut[2] = 1;
        }
        else
        {
            continousActionsOut[2] = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("교통 사고");
            SetReward(-1);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Goal"))
        {
            Debug.Log("골인");
            SetReward(1);
            EndEpisode();
        }
    }

    /// <summary>
    /// ray를 시각화
    /// </summary>
    private void DisplayRay()
    {
        Debug.DrawRay(rayFront.origin, rayFront.direction * rayDistance, Color.red);
        Debug.DrawRay(rayFR.origin, rayFR.direction * rayDistance, Color.red);
        Debug.DrawRay(rayRight.origin, rayRight.direction * rayDistance, Color.red);
        Debug.DrawRay(rayBR.origin, rayBR.direction * rayDistance, Color.red);
        Debug.DrawRay(rayBack.origin, rayBack.direction * rayDistance, Color.red);
        Debug.DrawRay(rayBL.origin, rayBL.direction * rayDistance, Color.red);
        Debug.DrawRay(rayLeft.origin, rayLeft.direction * rayDistance, Color.red);
        Debug.DrawRay(rayFL.origin, rayFL.direction * rayDistance, Color.red);
    }

    private void UpdateRay()
    {
        LinkStoneToRay(stoneFront, out rayFront);
        LinkStoneToRay(stoneFR, out rayFR);
        LinkStoneToRay(stoneRight, out rayRight);
        LinkStoneToRay(stoneBR, out rayBR);
        LinkStoneToRay(stoneBack, out rayBack);
        LinkStoneToRay(stoneBL, out rayBL);
        LinkStoneToRay(stoneLeft, out rayLeft);
        LinkStoneToRay(stoneFL, out rayFL);
    }

    /// <summary>
    /// stone의 위치 정보 및 forward를 활용하여 ray를 초기화한다.
    /// </summary>
    /// <param name="_stone"></param>
    /// <param name="_ray"></param>
    private void LinkStoneToRay(GameObject _stone, out Ray _ray)
    {
        _ray = new Ray(_stone.transform.position, _stone.transform.forward);
    }

    /// <summary>
    /// Ray Cast 후 거리 관측, 아무것도 관측되지 않은 경우, 편의상 0으로
    /// </summary>
    private void CastRay()
    {
        UpdateRay();
        sortedObjects.Clear();

        if (Physics.Raycast(rayFront, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayFR, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayRight, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayBR, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayBack, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayBL, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayLeft, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        if (Physics.Raycast(rayFL, out raycastHit, rayDistance))
        {
            sortedObjects.Add(raycastHit.point);
        }

        DisplayRay();
    }
}
