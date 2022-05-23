using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator : MonoBehaviour
{
    public GameObject testAgent;
    public GameObject target;
    public List<Vector3> parkingPositions;
    public List<GameObject> parkingBlock;

    //public void OnNavigator()
    //{
    //    target.transform.localPosition = new Vector3(Random.Range(-7f, 7f), 0f, Random.Range(-7f, 7f));
    //    target.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    //}

    //public void MoveNextPosition()
    //{
    //    Debug.Log("∞Ò¿Œ");
    //    target.transform.localPosition = new Vector3(Random.Range(-7f, 7f), 0f, Random.Range(-7f, 7f));
    //    target.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    //}

    public virtual void OnNavigator()
    {
        SetParkingTest();
    }

    public virtual void MoveNextPosition()
    {
        Debug.Log("∞Ò¿Œ");
        SetParkingTest();
    }

    private void SetParkingTest()
    {
        int parkIndex = Random.Range(0, parkingPositions.Count);
        int blocIndex = 0;

        for (int index = 0; index < parkingPositions.Count; index++)
        {
            if (index == parkIndex)
            {
                target.transform.localPosition = parkingPositions[parkIndex];
            }
            else
            {
                parkingBlock[blocIndex].transform.localPosition = parkingPositions[index];
                blocIndex += 1;
            }
        }
    }
}
