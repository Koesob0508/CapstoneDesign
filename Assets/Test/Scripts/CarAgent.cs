using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class CarAgent : Agent
{
    // 8 ������ Ŀ�����ϱ� ���� ����
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

    // 8 ���⿡ ���� Ray
    private Ray rayFront;
    private Ray rayFR;
    private Ray rayRight;
    private Ray rayBR;
    private Ray rayBack;
    private Ray rayBL;
    private Ray rayLeft;
    private Ray rayFL;

    // �����ϱ� ���ϱ� ���� List ����
    public List<GameObject> stoneList;

    private void Awake()
    {
        stoneList = new List<GameObject>();
    }

    private void Start()
    {
        // Stone���� stonList�� �߰�
        stoneList.Add(stoneFront);
        stoneList.Add(stoneFR);
        stoneList.Add(stoneRight);
        stoneList.Add(stoneBR);
        stoneList.Add(stoneBack);
        stoneList.Add(stoneBL);
        stoneList.Add(stoneLeft);
        stoneList.Add(stoneFL);

        // Ray�� stone�� ��ġ�� �ʱ�ȭ
        UpdateRay();

        distanceList = new float[stoneList.Count];
    }

    private void FixedUpdate()
    {
        CastRay();
        UpdateRay();
        DebugRay();
    }

    private void CastRay()
    {
        RaycastHit[] raycastHit;

        raycastHit = Physics.RaycastAll(rayFront, distance);

        foreach(RaycastHit rayHit in raycastHit)
        {
            Debug.Log(rayFront.origin);
            Debug.Log(this.gameObject.name + " " + rayHit.collider.gameObject.name + " " + rayHit.distance);
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
    /// ray�� �ð�ȭ
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
    /// stone�� ��ġ ���� �� forward�� Ȱ���Ͽ� ray�� �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="_stone"></param>
    /// <param name="_ray"></param>
    private void LinkStoneToRay(GameObject _stone, out Ray _ray)
    {
        _ray = new Ray(_stone.transform.position, _stone.transform.forward);
    }

    private void UpdateDistance(int _index, RaycastHit[] _raycastHits)
    {
        if(_raycastHits.Length != 1)
        {
            Array.Sort(_raycastHits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
            distanceList[_index] = _raycastHits[1].distance;
            Debug.Log(_raycastHits[0].collider.gameObject.name);
            Debug.Log(_raycastHits[1].collider.gameObject.name);
        }
        else
        {
            distanceList[_index] = 0;
            Debug.Log("������");
        }
        
    }

    /// <summary>
    /// �ʱ�ȭ ���
    /// </summary>
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }
}
