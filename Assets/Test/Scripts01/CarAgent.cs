using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class CarAgent : Agent
{
    public List<GameObject> EnvList;
    public GameObject currentEnv;
    public Rigidbody carRigidbody;
    public float currentTime;
    public float shortestDistance;
    public float tempDist;

    public GameObject Target;

    // 8 방향을 커스텀하기 위한 지점
    public GameObject stoneFront;
    public GameObject stoneFR;
    public GameObject stoneRight;
    public GameObject stoneBR;
    public GameObject stoneBack;
    public GameObject stoneBL;
    public GameObject stoneLeft;
    public GameObject stoneFL;

    public float distance;
    public float[] distanceList;

    // 8 방향에 대한 Ray
    private Ray rayFront;
    private Ray rayFR;
    private Ray rayRight;
    private Ray rayBR;
    private Ray rayBack;
    private Ray rayBL;
    private Ray rayLeft;
    private Ray rayFL;

    // 관리하기 편하기 위한 List 형태
    public List<GameObject> stoneList;
    public CarController carController;

    private void Start()
    {
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

        distanceList = new float[stoneList.Count];

        carRigidbody = this.gameObject.GetComponent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        AddReward(-0.1f * Time.deltaTime);
        DebugRay();
        currentTime += Time.deltaTime;
        if(currentTime >= 300f)
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// 초기화 담당
    /// </summary>
    public override void OnEpisodeBegin()
    {
        carRigidbody.velocity = new Vector3(0f, 0f, 0f);
        carRigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
        currentTime = 0f;
        shortestDistance = Vector3.Distance(this.gameObject.transform.position, Target.transform.position);
        // Ray를 stone의 위치로 초기화
        UpdateRay();

        //if (currentEnv)
        //{
        //    int index = UnityEngine.Random.Range(0, EnvList.Count);
        //    currentEnv.SetActive(false);
        //    currentEnv = EnvList[index];
        //    currentEnv.SetActive(true);

        //    this.Target = GameObject.Find("Goal");
        //}
        //else
        //{
        //    int index = UnityEngine.Random.Range(0, EnvList.Count);
        //    currentEnv = EnvList[index];
        //    currentEnv.SetActive(true);

        //    this.Target = GameObject.Find("Goal");
        //}

        Target.transform.localPosition = new Vector3(UnityEngine.Random.Range(-4f, 4f), -0.2f, UnityEngine.Random.Range(-4f, 4f));

        this.gameObject.transform.localPosition = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
        this.gameObject.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);
    }

    /// <summary>
    /// 관측
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        CastRay();
        UpdateRay();

        sensor.AddObservation(this.gameObject.transform.position);
        sensor.AddObservation(this.gameObject.transform.rotation);
        sensor.AddObservation(this.carRigidbody.velocity);
        sensor.AddObservation(this.carRigidbody.angularVelocity);

        // 현재 8
        for (int index = 0; index < distanceList.Length; index++)
        {
            sensor.AddObservation(distanceList[index]);
        }

        sensor.AddObservation(Target.transform.position);
    }

    /// <summary>
    /// 실행
    /// </summary>
    /// <param name="actions"></param>
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

        tempDist = Vector3.Distance(this.gameObject.transform.position, Target.transform.position);
        if (tempDist < shortestDistance)
        {
            float delta = shortestDistance - tempDist;
            AddReward(delta);
            shortestDistance = tempDist;
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

    private void CastRay()
    {
        RaycastHit raycastHit;

        if(Physics.Raycast(rayFront, out raycastHit, distance))
        {
            distanceList[0] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[0] = 0;
        }

        if (Physics.Raycast(rayFR, out raycastHit, distance))
        {
            distanceList[1] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[1] = 0;
        }
        if (Physics.Raycast(rayRight, out raycastHit, distance))
        {
            distanceList[2] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[2] = 0;
        }
        if (Physics.Raycast(rayBR, out raycastHit, distance))
        {
            distanceList[3] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[3] = 0;
        }
        if (Physics.Raycast(rayBack, out raycastHit, distance))
        {
            distanceList[4] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[4] = 0;
        }
        if (Physics.Raycast(rayBL, out raycastHit, distance))
        {
            distanceList[5] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[5] = 0;
        }
        if (Physics.Raycast(rayLeft, out raycastHit, distance))
        {
            distanceList[6] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[6] = 0;
        }
        if (Physics.Raycast(rayFL, out raycastHit, distance))
        {
            distanceList[7] = distance - raycastHit.distance;
        }
        else
        {
            distanceList[7] = 0;
        }


        // UpdateDistance(0, raycastHit);
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// ray를 시각화
    /// </summary>
    private void DebugRay()
    {
        Debug.DrawRay(rayFront.origin, rayFront.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayFR.origin, rayFR.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayRight.origin, rayRight.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayBR.origin, rayBR.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayBack.origin, rayBack.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayBL.origin, rayBL.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayLeft.origin, rayLeft.direction * distance, Color.red, Time.deltaTime);
        Debug.DrawRay(rayFL.origin, rayFL.direction * distance, Color.red, Time.deltaTime);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("교통 사고");
            AddReward(-60);
            EndEpisode();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("CenterLine"))
        {
            Debug.Log("중앙선 침범");
            AddReward(-0.1f * Time.deltaTime);
        }

        if (other.gameObject.CompareTag("Goal"))
        {
            Debug.Log("골인");
            AddReward(100);
            Target.transform.localPosition = new Vector3(UnityEngine.Random.Range(-5f, 5f), -0.2f, UnityEngine.Random.Range(-5f, 5f));
            currentTime = 0f;
            // EndEpisode();
        }
    }

    
}
