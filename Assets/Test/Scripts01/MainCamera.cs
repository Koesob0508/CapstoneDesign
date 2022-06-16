using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform target;        // ����ٴ� Ÿ�� ������Ʈ�� Transform

    private Transform tr;                // ī�޶� �ڽ��� Transform

    public Vector3 edit;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void LateUpdate()
    {
        tr.position = new Vector3(target.transform.position.x + edit.x, target.transform.position.y + edit.y, target.transform.position.z + edit.z);

        tr.LookAt(target);
    }
}