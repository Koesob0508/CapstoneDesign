using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TestAgent : Agent
{
    // Inspector 창에서 미리 할당 필요
    public Navigator navigator;
    public GameObject target;
    public int timeScale;

    public float rayDistance; // ray 최대 거리
    public float accel;
    public float targetRadius;
    public int rayCount;

    // Start 에서 할당 필요
    public Rigidbody carRigidbody;
    public CarController carController;
    public SortedList<double, Vector3> rayVectors; // Obstacle 리스트
    public int obstacleLayer;

    // 코드 상에서 할당할 예정
    public Vector2 toTarget;
    public Vector2 fromObstacle;
    public Vector2 toForward;

    private RaycastHit raycastHit; // RayCast를 위한 변수

    private Vector3 heightVector;

    public override void Initialize()
    {
        carRigidbody = this.GetComponent<Rigidbody>();
        carController = this.GetComponent<CarController>();
        fromObstacle = Vector2.zero;

        rayVectors = new SortedList<double, Vector3>();

        heightVector = new Vector3(0f, 0.5f, 0f);

        obstacleLayer = 1 << 8;

        Time.timeScale = timeScale;
    }

    public override void OnEpisodeBegin()
    {
        //this.gameObject.transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        //this.gameObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        

        // goal.transform.localPosition = goalPositionList[index];
        navigator.OnNavigator();
        carRigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 targetXZ = new Vector2(target.transform.position.x, target.transform.position.z);
        Vector2 positionXZ = new Vector2(this.transform.position.x, this.transform.position.z);
        toTarget = targetXZ - positionXZ;
        sensor.AddObservation(toTarget.normalized); // 2

        float relativeDistance = toTarget.magnitude / targetRadius; // 상대적 거리인데 이게 필요한가 싶긴 하다.
        sensor.AddObservation(relativeDistance); // 1

        sensor.AddObservation(carRigidbody.velocity.x); // 1
        sensor.AddObservation(carRigidbody.velocity.z); // 1

        Vector2 agentForward = new Vector2(this.transform.forward.x + this.transform.position.x, this.transform.forward.z + this.transform.position.z);
        
        // targetForward
        Vector2 targetForward = new Vector2(target.transform.forward.x + target.transform.position.x, target.transform.forward.z + target.transform.position.z);

        toForward = targetForward - agentForward;
        float forwardDistance = toForward.magnitude / targetRadius;

        sensor.AddObservation(toForward); // 2
        sensor.AddObservation(forwardDistance);

        CastRay(rayCount);
        
        int maxCount = 3;
        int rayIndex = 0;

        fromObstacle = Vector2.zero;

        foreach (KeyValuePair<double, Vector3> direction in rayVectors)
        {
            fromObstacle.x += direction.Value.x;
            fromObstacle.y += direction.Value.z;

            if(rayIndex == maxCount-1)
            {
                break;
            }

            rayIndex++;
        }

        if (rayVectors.Count != 0)
        {
            if(rayVectors.Count < maxCount)
            {
                fromObstacle /= rayVectors.Count;
            }
            else
            {
                fromObstacle /= maxCount;
            }
            
        }

        rayVectors.Clear();

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

        if (toTarget.magnitude < targetRadius)
        {
            float reward = 0.3f;
            if(toForward.magnitude < targetRadius)
            {
                reward += 0.3f;
            }
            SetReward(reward);
            navigator.MoveNextPosition();
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

            if (this.StepCount < 300000)
            {
                AddReward(-0.000003f);
            }
            else
            {
                SetReward(-0.9f);
            }
            EndEpisode();
        }
    }

    private void CastRay(int rayCount)
    {
        // 갯수만큼 Ray 만들고
        // ray 표시

        float rotateAngle = 360f / rayCount;

        for (int index = 0; index < rayCount; index++)
        {
            Vector3 normalizedVector = Vector3.forward;
            Vector3 currentVector = Quaternion.AngleAxis(rotateAngle * index, Vector3.up) * normalizedVector;
            Ray ray = new Ray(this.transform.position + heightVector, currentVector);
            if (Physics.Raycast(ray, out raycastHit, rayDistance, obstacleLayer))
            {
                CastReverseRay(raycastHit, this.transform.position);
                // Debug.DrawRay(this.transform.position, currentVector * rayDistance, Color.red, Time.fixedDeltaTime*20);
            }
            else
            {
                Debug.DrawRay(this.transform.position + new Vector3(0f, 0.5f, 0f), currentVector * rayDistance, Color.green, Time.deltaTime);
            }
        }
    }

    private void CastReverseRay(RaycastHit _raycastHit, Vector3 _position)
    {
        Vector3 origin = _raycastHit.point;
        Vector3 direction = (_position + heightVector - origin).normalized;

        Ray reverseRay = new Ray(origin, direction);
        RaycastHit reverseRayHit;

        if (Physics.Raycast(reverseRay, out reverseRayHit, _raycastHit.distance, obstacleLayer))
        {
            Debug.DrawRay(origin, direction * reverseRayHit.distance, Color.red, Time.deltaTime);
            // reverseRayHit으로부터 point 구하기
            // point - origin으로 방향 벡터 구하기 -> 이게 이미 direction 아닌가유
            // reverseRayHit.distance / rayDistance = ratio
            // 1 - ratio = weighted Ratio
            double weightedRatio = 1 - (reverseRayHit.distance / rayDistance);
            // normalized 방향 벡터 * weighted Ratio = Weighted Direction Vector

            Vector3 weightedDirectionVector = direction * (float)weightedRatio;
            // rayVectors.Add(Weighted Direction Vector) -> Vector2로 보내는게 낫지 않을까 -> 불분명할듯
            rayVectors.Add(-weightedRatio, weightedDirectionVector);
        }
    }
}
