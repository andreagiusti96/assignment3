using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[RequireComponent(typeof(DroneController))]
public class StrikerAI : MonoBehaviour
{
    private DroneController m_Drone; // the drone controller we want to use

    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    public GameObject[] friends;
    public string friend_tag;
    public GameObject[] enemies;
    public string enemy_tag;

    public GameObject own_goal;
    public GameObject other_goal;
    public GameObject ball; 

    public int DroneID;
    float Kv = 10f;
    float Kvd = 5f;
    float Ka = 10f;
    Vector3 BallVelocity;
    Vector3 OldBallPos = Vector3.zero;
    Vector3 OldErr = Vector3.zero;
    int bScore = 0;
    int rScore = 0;
    bool attack = true; //se sto perdendo o pareggiando

    private void Start()
    {
        // get the car controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();


        // note that both arrays will have holes when objects are destroyed
        // but for initial planning they should work
        friend_tag = gameObject.tag;
        if (own_goal.CompareTag("blue"))
        {
            enemy_tag = "red";
            friend_tag = "blue";
        }
        else
        {
            enemy_tag = "blue";
            friend_tag = "red";
        }
        friends = GameObject.FindGameObjectsWithTag(friend_tag);
        enemies = GameObject.FindGameObjectsWithTag(enemy_tag);
        ball = GameObject.FindGameObjectWithTag("Ball");
        Debug.Log("own goal" + own_goal);

    }


    private void FixedUpdate()
    {
        DecideBehaviour();
        Vector3 acc = ComputeAcceleration();
        OldBallPos = ball.transform.position;
        m_Drone.Move_vect(acc);
    }

    public Vector3 ComputeAcceleration()
    {
        Vector3 acc = Vector3.zero;
        float Umax = m_Drone.max_acceleration;
        float Vmax = m_Drone.max_speed;
        if (attack || !attack)
        {
            if ((friend_tag == "blue" && m_Drone.transform.position.x > (ball.transform.position.x )) || (friend_tag == "red" && m_Drone.transform.position.x < (ball.transform.position.x)))
            {
                acc = own_goal.transform.position - m_Drone.transform.position;
                //Debug.DrawLine(m_Drone.transform.position, own_goal.transform.position, Color.black);
            }
            else
            {
                acc = (ball.transform.position - m_Drone.transform.position);
                //Debug.DrawLine(m_Drone.transform.position, ball.transform.position, Color.black);
            }
        }
        //else
        //{
        //    Vector3 DesiredSpeed = Kv * (GetGoalieSet() - m_Drone.transform.position) + Kvd * ((GetGoalieSet() - m_Drone.transform.position) - OldErr) / Time.fixedDeltaTime;
        //    acc = Ka * (DesiredSpeed - m_Drone.velocity);
        //    m_Drone.Move_vect(acc / Umax);
        //    OldErr = (GetGoalieSet() - m_Drone.transform.position);
        //}
        return acc;
    }
    public Vector3 GetGoalieSet()
    {
        //Incentro del triangolo tra pali e palla se la palla è lontana

        float a, b, c;
        Vector3 palo1 = new Vector3(own_goal.transform.position.x, 0f, own_goal.transform.position.z - 17f);
        Vector3 palo2 = new Vector3(own_goal.transform.position.x, 0f, own_goal.transform.position.z + 17f);
        a = Vector3.Distance(ball.transform.position, palo1);
        b = Vector3.Distance(ball.transform.position, palo2);
        c = Vector3.Distance(palo1, palo2);
        Vector3 GoalieSet = new Vector3((a * palo2.x + b * palo1.x + c * ball.transform.position.x) / (a + b + c), 0f, (a * palo2.z + b * palo1.z + c * ball.transform.position.z) / (a + b + c));
        //Debug.DrawLine(ball.transform.position, GoalieSet, Color.green);
        //Debug.DrawLine(ball.transform.position, palo1, Color.magenta);
        //Debug.DrawLine(ball.transform.position, palo2, Color.magenta);
        //Debug.DrawLine(m_Drone.transform.position, GoalieSet, Color.black);
        return GoalieSet;
    }
    public void DecideBehaviour()
    {
        bScore = ball.GetComponent<GoalCheck>().blue_score;
        rScore = ball.GetComponent<GoalCheck>().red_score;
        attack = ((friend_tag == "blue" && bScore <= rScore) || (friend_tag == "red" && rScore <= bScore));
        //if (friend_tag == "blue") Debug.Log("blue attack: " + attack);
        //else Debug.Log("red attack: " + attack);

    }

    public Vector3 GetBallSpeed(Vector3 OldPos)
    {
        Vector3 BallVelocity = (ball.transform.position - OldPos) / Time.fixedDeltaTime;
        return BallVelocity;
    }
}

