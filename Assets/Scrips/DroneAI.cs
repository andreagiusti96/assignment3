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

    int waypoint = 0;

    //Constraints
    Vector2[] A;
    float[] b;
    float[] h;
    float Ds = 10f;

    private void Start()
    {
        // get the drone controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        friends = GameObject.FindGameObjectsWithTag("Drone");

        Vector3 start_pos = terrain_manager.myInfo.start_pos;
        Vector3 goal_pos = terrain_manager.myInfo.goal_pos;
  
    }


    private void FixedUpdate()
    {
        ComputeConstraints();
        float Umax = m_Drone.max_acceleration;
        Vector3 acc;
        float Kv = 1;
        float Ka = 1;
        float treshold = 5;
        Vector3 DesiredSpeed = Kv * (my_path[waypoint].point - m_Drone.transform.position);
        acc = Ka * (DesiredSpeed - m_Drone.velocity);
        acc = two2three(RandSearc(three2two(acc), A, b, Umax));
        m_Drone.Move_vect(acc);
        if (waypoint < (my_path.Count - 1) && Vector3.Distance(my_path[waypoint].point, m_Drone.transform.position) < treshold)
        {
            waypoint++;
        }
        

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

    public float ObjFunction(Vector2 u, Vector2 uHat)
    {
        float J;
        J = Mathf.Pow(Vector2.SqrMagnitude(u - uHat), 2);
        return J;
    }
    public Vector2 Gradient(Vector2 uBar, Vector2 uHat)
    {
        Vector2 grad = Vector2.zero;
        grad.x = 2 * (uBar.x - uHat.x);
        grad.y = 2 * (uBar.y - uHat.y);
        return grad;
    }

    public void ComputeConstraints()
    {
        // A computation
        A = new Vector2[friends.Length - 1];
        Vector2 currPos = new Vector2(m_Drone.transform.position.x, m_Drone.transform.position.z);
        Vector2 currSpeed = new Vector2(m_Drone.velocity.x, m_Drone.velocity.z);
        int i = 0, j = 0;
        while (i < friends.Length)
        {
            if (currPos.x != friends[i].transform.position.x && currPos.y != friends[i].transform.position.z)
            {
                A[j].x = -(currPos.x - friends[i].transform.position.x);
                A[j].y = -(currPos.y - friends[i].transform.position.z);
                //Debug.Log("A_ij:" + A[j].x + ", " + A[j].y);
                i++;
                j++;
            }
            else
            {
                i++;
            }
        }
        //h and b computation
        h = new float[friends.Length - 1];
        b = new float[friends.Length - 1];
        Vector2 DeltaP;
        Vector2 DeltaV;
        float Umax = m_Drone.max_acceleration;
        System.Random rand = new System.Random();
        float gamma = 1 + rand.Next(5);
        i = 0;
        j = 0;
        while (i < friends.Length)
        {
            if (currPos.x != friends[i].transform.position.x && currPos.y != friends[i].transform.position.z)
            {
                DeltaP = new Vector2(currPos.x - friends[i].transform.position.x, currPos.y - friends[i].transform.position.z);
                DeltaV = new Vector2(currSpeed.x - friends[i].GetComponent<DroneController>().velocity.x, currSpeed.y - friends[i].GetComponent<DroneController>().velocity.z);
                float x = -(DeltaP.x * DeltaV.x + DeltaP.y * DeltaV.y) * (DeltaP.x * currSpeed.x + DeltaP.y * currSpeed.y) / Mathf.Pow(DeltaP.magnitude, 2);
                float y = (DeltaV.x * currSpeed.x + DeltaV.y * currSpeed.y);
                h[j] = Mathf.Sqrt(4 * (Umax) * (DeltaP.magnitude - Ds)) + ((DeltaP.x * DeltaV.x + DeltaP.y * DeltaV.y) / DeltaP.magnitude);
                float z = (1 / 2) * ((gamma * Mathf.Pow(h[j], 3) * DeltaP.magnitude) + (Mathf.Sqrt(2*Umax) * (DeltaP.x * DeltaV.x + DeltaP.y * DeltaV.y) / (Mathf.Sqrt(2 * (DeltaP.magnitude - Ds)))));
                if (DeltaP.magnitude < 20)
                {
                    b[j] = x + y + z;
                }
                else
                {
                    b[j] = 10000000; 
                }
         
                if(h[j]<0) Debug.Log("h negativo");

                i++;
                j++;
            }
            else
            {
                i++;
            }
        }
    }
    public bool CheckConstraints(Vector2[] A, Vector2 u, float[] b, float Umax)
    {
        bool ConstraintRespected = true;
        for (int i = 0; i < A.Length; i++)
        {
            if ((A[i].x * u.x + A[i].y * u.y) > b[i] || u.magnitude > Umax)
            {
                ConstraintRespected = false;
                //Debug.Log("Vincoli non rispettati");
                break;
            }
        }
        return ConstraintRespected;
    }

    public Vector2 SteepDesc(Vector2 u0, Vector2 uHat, Vector2[] A, float[] b, float Umax)
    {
        float alfa = 0.1f; //Step
        Vector2 direction; //direction

        Vector2 uOld = u0;
        while (CheckConstraints(A, u0, b, Umax))
        {
            uOld = u0;
            direction = -Gradient(u0, uHat);
            u0 += alfa * direction;
        }

        return uOld;
    }
    public Vector2 RandSearc(Vector2 uHat, Vector2[] A, float[] b, float Umax)
    {
        float step = 0f;
        int count = 0;
        Vector2 uRand = uHat;
        while (!CheckConstraints(A, uRand, b, Umax) && step < 15)
        {
            uRand = uHat;
            if (count == 7)
            {
                count = 0;
                step += 1f;
            }
            if (count == 0)
            {
                uRand.x = uHat.x + step;
            }
            if (count == 1)
            {
                uRand.x = uHat.x + step;
                uRand.y = uHat.y + step;
            }
            if (count == 2)
            {
                uRand.y = uHat.y + step;
            }
            if (count == 3)
            {
                uRand.x = uHat.x - step;
                uRand.y = uHat.y + step;
            }
            if (count == 4)
            {
                uRand.x = uHat.x - step;
            }
            if (count == 5)
            {
                uRand.x = uHat.x - step;
                uRand.y = uHat.y - step;
            }
            if (count == 6)
            {
                uRand.y = uHat.y - step;
            }
            if (count == 7)
            {
                uRand.x = uHat.x + step;
                uRand.y = uHat.y - step;
            }
            count++;
        }
        if (step == 0)
        {
            Debug.Log("uscito e basta");
        }
        else if (step >= 15)
        {
            Debug.Log("uscito con antiimpallo");
        }
        else
        {
            Debug.Log("uscito senza antiimpallo");
        }
        return uRand;
    }
}
