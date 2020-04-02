using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Stare insieme a me è poco produttivo

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends; // use these to avoid collisions

        public GameObject my_goal_object;

        //Constraints
        Vector2[] A;
        float[] b;
        float[] h;
        float Ds = 1;

        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            // Plan your path here
            // Replace the code below that makes a random path
            // ...

            Vector3 start_pos = transform.position; // terrain_manager.myInfo.start_pos;
            Vector3 goal_pos = terrain_manager.myInfo.goal_pos;

            friends = GameObject.FindGameObjectsWithTag("Player");

            List<Vector3> my_path = new List<Vector3>();

            my_path.Add(start_pos);

            for (int i = 0; i < 3; i++)
            {
                Vector3 waypoint = start_pos + new Vector3(UnityEngine.Random.Range(-50.0f, 50.0f), 0, UnityEngine.Random.Range(-30.0f, 30.0f));
                my_path.Add(waypoint);
            }
            my_path.Add(goal_pos);


            // Plot your path to see if it makes sense
            // Note that path can only be seen in "Scene" window, not "Game" window
            Vector3 old_wp = start_pos;
            foreach (var wp in my_path)
            {
                //Debug.DrawLine(old_wp, wp, Color.red, 100f);
                old_wp = wp;
            }
            ComputeConstraints();
            
        }


        private void FixedUpdate()
        {
            // Execute your path here
            // ...
 
            // this is how you access information about the terrain
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            //Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z));


            Vector3 relVect = my_goal_object.transform.position - transform.position;
            bool is_in_front = Vector3.Dot(transform.forward, relVect) > 0f;
            bool is_to_right = Vector3.Dot(transform.right, relVect) > 0f;

            if(is_in_front && is_to_right)
                m_Car.Move(1f, 1f, 0f, 0f);
            if(is_in_front && !is_to_right)
                m_Car.Move(-1f, 1f, 0f, 0f);
            if(!is_in_front && is_to_right)
                m_Car.Move(-1f, -1f, -1f, 0f);
            if(!is_in_front && !is_to_right)
                m_Car.Move(1f, -1f, -1f, 0f);


            // this is how you control the car
            //m_Car.Move(1f, 1f, 1f, 0f);

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
            Vector2 currPos = new Vector2(m_Car.transform.position.x, m_Car.transform.position.z);
            float angle = Vector2.SignedAngle(currPos, m_Car.transform.forward);//to check
            Vector2 currSpeed = new Vector2(m_Car.CurrentSpeed * Mathf.Cos(angle), m_Car.CurrentSpeed * Mathf.Sin(angle));
            Vector3 speed;
            speed.x = currSpeed.x;
            speed.y = 0;
            speed.z = currSpeed.y;
            //Debug.DrawLine(m_Car.transform.position, m_Car.transform.position + speed, Color.black);
            int i = 0, j = 0;
            while (i < friends.Length)
            {
                if (currPos.x != friends[i].transform.position.x && currPos.y != friends[i].transform.position.z)
                {
                    A[j].x = -(currPos.x - friends[i].transform.position.x);
                    A[j].y = -(currPos.y - friends[i].transform.position.z);
                    Debug.Log("A_ij:" + A[j].x + ", " + A[j].y);
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
            float Umaxi = 1;
            float gamma = 1;
            i = 0;
            j = 0;
            while (i < friends.Length)
            {
                if (currPos.x != friends[i].transform.position.x && currPos.y != friends[i].transform.position.z)
                {
                    DeltaP = new Vector2(currPos.x - friends[i].transform.position.x, currPos.y - friends[i].transform.position.z);
                    angle = Vector2.SignedAngle(friends[i].transform.position, friends[i].transform.forward);
                    DeltaV = new Vector2(currSpeed.x - (friends[i].GetComponent<CarController>().CurrentSpeed * Mathf.Cos(angle)), currSpeed.y - (friends[i].GetComponent<CarController>().CurrentSpeed * Mathf.Sin(angle)));
                    float x = -(DeltaP.x * DeltaV.x + DeltaP.y * DeltaV.y) * (DeltaP.x * currSpeed.x + DeltaP.y * currSpeed.y) / Mathf.Pow(DeltaP.magnitude, 2);
                    float y = (DeltaV.x * currSpeed.x + DeltaV.y * currSpeed.y);
                    h[j] = Mathf.Sqrt(4 * (Umaxi) * (DeltaP.magnitude - Ds)) - x;
                    float z = (1 / 2) * ((gamma * Mathf.Pow(h[i], 3) * DeltaP.magnitude) + (Mathf.Sqrt(2) * (DeltaP.x * DeltaV.x + DeltaP.y * DeltaV.y) / (Mathf.Sqrt(2 * (DeltaP.magnitude - Ds)))));
                    b[j] = x + y + z;
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
            while (!CheckConstraints(A, uRand, b, Umax))
            {
                if (count == 5)
                {
                    count = 0;
                    step += 0.1f;
                }
                if (count == 0)
                {
                    uRand.x = uHat.x + step;
                }
                if (count == 1)
                {
                    uRand.y = uHat.y + step;
                }
                if (count == 3)
                {
                    uRand.x = uHat.x - step;
                }
                if (count == 4)
                {
                    uRand.y = uHat.y - step;
                }
                count++;
            }
            return uRand;
        }
    }
}
