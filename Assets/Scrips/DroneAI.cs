using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DroneController))]
public class DroneAI : MonoBehaviour
{

    private DroneController m_Drone; // the car controller we want to use
    public GameObject[] friends;
    public GameObject my_goal_object;
    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    public List<Node> my_path = new List<Node>();

    public int waypoint = 0;

    public int DroneID;

    //Constraints
    Vector2[] A;
    double[] b;
    double[] h;
    float Ds;
    double gamma;

    float Umax;
    float Vmax;
    float Kv;
    float Ka;

    //stuckRecovery
    bool stuck = false;
    Vector3 LastPos;
    int stuckCounter = 0;
    int recovery = 0;

    private void Start()
    {
        // get the drone controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        //friends = GameObject.FindGameObjectsWithTag("Drone");

        Vector3 start_pos = terrain_manager.myInfo.start_pos;
        Vector3 goal_pos = terrain_manager.myInfo.goal_pos;

        Ds= 4 + 2f * (float)DroneID / (float)(friends.Length - 1);
        gamma = 6000f - (float)DroneID * 100f;
        Vmax = 15f - 10f * (float)DroneID / (float)(friends.Length-1);
        Kv = 2f - (float)DroneID / (float)(friends.Length - 1);
        Ka = 1f;
    }


    private void FixedUpdate()
    {
        Umax = m_Drone.max_acceleration;
        Vector3 acc;

        if (Vector3.Distance(LastPos, m_Drone.transform.position) < 0.01f && Vector3.Distance(my_path[my_path.Count - 1].point, m_Drone.transform.position) > 5f && recovery == 0) stuckCounter++;
        stuck = stuckCounter > 10;
        if (stuck)
        {
            Debug.Log("Drone" + DroneID + " Stucked");
            acc = AccRecovery();
            m_Drone.Move_vect(acc);
            recovery++;
            if (recovery > 50)
            {
                stuckCounter = 0;
                recovery = 0;
            }
        }
        else
        {

            ComputeConstraints();
            float treshold = 7;

            Vector3 DesiredSpeed = Kv * (my_path[waypoint].point - m_Drone.transform.position);
            if (DesiredSpeed.magnitude > Vmax) DesiredSpeed = DesiredSpeed.normalized * Vmax;

            acc = Ka * (DesiredSpeed - m_Drone.velocity);
            if (acc.magnitude > Umax)
            {
                acc = acc.normalized * Umax;
            }


            acc = two2three(RandSearc(three2two(acc)));

            m_Drone.Move_vect(acc / Umax);

            if (waypoint < (my_path.Count - 1) && Vector3.Distance(my_path[waypoint].point, m_Drone.transform.position) < treshold)
            {
                waypoint++;
            }
        }
        LastPos = m_Drone.transform.position;

    }

    public Vector3 two2three(Vector2 vec)
    {
        Vector3 vecNew = new Vector3(vec.x, 0, vec.y);
        return vecNew;
    }
    public Vector2 three2two(Vector3 vec)
    {
        Vector2 vecNew = new Vector2(vec.x, vec.z);
        return vecNew;
    }

    public void ComputeConstraints()
    {
        A = new Vector2[friends.Length];
        b = new double[friends.Length];
        h = new double[friends.Length];



        for (int j=0; j<A.Length; j++)
        {
            Vector2 deltaP = three2two(m_Drone.transform.position - friends[j].transform.position);
            Vector2 deltaV = three2two(m_Drone.velocity - friends[j].GetComponent<DroneController>().velocity);

            h[j] = Mathf.Sqrt(2 * Umax * (deltaP.magnitude-Ds))+ Vector2.Dot(deltaP,deltaV) / deltaP.magnitude;


            A[j] = -deltaP;

            b[j] = -Vector2.Dot(deltaP, deltaV) * Vector2.Dot(deltaP, three2two(m_Drone.velocity)) / Mathf.Pow(deltaP.magnitude, 2);
            b[j] += Vector2.Dot(deltaV, three2two(m_Drone.velocity));
            b[j] += 0.5 * (gamma * Mathf.Pow((float)h[j], 3) * deltaP.magnitude + Mathf.Sqrt(2 * Umax) * Vector2.Dot(deltaP, deltaV) / Mathf.Sqrt(2 * (deltaP.magnitude - Ds)));
        }
    }


    public bool CheckConstraints(Vector2 u)
    {
        bool ConstraintRespected = true;

        ConstraintRespected = u.magnitude <= Umax;

        int j = 0;
        while(ConstraintRespected && j < friends.Length)
        {
            if(j != DroneID)
            {
                ConstraintRespected = Vector2.Dot(A[j], u) <= b[j];
            }

            if (!ConstraintRespected) Debug.DrawLine(m_Drone.transform.position, friends[j].transform.position, Color.magenta);

            j++;
        }

        return ConstraintRespected;
    }



    public Vector2 RandSearc(Vector2 uHat)
    {
        float step = 0f;
        int count = 0;
        Vector2 uRand = uHat;

        while (!CheckConstraints(uRand) && step < 30)
        {

            switch (count)
            {
                case 0:
                    uRand = uHat + new Vector2(0, step);
                    break;
                case 1:
                    uRand = uHat + new Vector2(step, step);
                    break;
                case 2:
                    uRand = uHat + new Vector2(step, 0);
                    break;
                case 3:
                    uRand = uHat + new Vector2(step, -step);
                    break;
                case 4:
                    uRand = uHat + new Vector2(0, -step);
                    break;
                case 5:
                    uRand = uHat + new Vector2(-step, -step);
                    break;
                case 6:
                    uRand = uHat + new Vector2(-step, 0);
                    break;
                case 7:
                    uRand = uHat + new Vector2(-step, step);
                    break;
                case 8:
                    uRand = uHat * 0.1f * step;
                    break;
                case 9:
                    uRand = - uHat * 0.1f * step;
                    step += 1;
                    break;
            }

            count = (count + 1) % 10;
        }


        if (step == 0)
        {
            Debug.Log("uscito e basta");
        }
        else if (step >= 30)
        {
            Debug.Log("uscito con antiimpallo");
            uRand = three2two(AccRecovery());
        }
        else
        {
            Debug.Log("uscito con step="+step);
        }

        return uRand;//4*uRand;
    }

    public Vector3 closestFriend()
    {
        Vector3 goal = Vector3.zero;
        float dist = float.PositiveInfinity;
        float actualDist;

        for (int i = 0;i< friends.Length; i++)
        {
            actualDist = Vector3.Distance(m_Drone.transform.position, friends[i].transform.position);
            if (actualDist != 0 && dist > actualDist)
            {
                dist = actualDist;
                goal = friends[i].transform.position;
            }
        }
        return goal;
    }
    public Vector3 AccRecovery()
    {
        Vector3 acc = - 1f * (closestFriend() - m_Drone.transform.position);
        acc = Quaternion.EulerAngles(0, 3.14f/9.0f, 0) * acc; 
        return acc;
    }

    public Vector3 StuckRecovery(Vector3 Pos)
    {
        Vector3 acc = -0.5f * (Pos - m_Drone.transform.position);
        acc = Quaternion.EulerAngles(0, 3.14f / 9.0f, 0) * acc;
        return acc;
    }
}
