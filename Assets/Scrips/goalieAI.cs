using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[RequireComponent(typeof(DroneController))]
public class goalieAI : MonoBehaviour
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

    private void Start()
    {
        // get the car controller
        m_Drone = GetComponent<DroneController>();
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();


        // note that both arrays will have holes when objects are destroyed
        // but for initial planning they should work
        friend_tag = gameObject.tag;
        if (friend_tag == "Blue")
            enemy_tag = "Red";
        else
            enemy_tag = "Blue";

        friends = GameObject.FindGameObjectsWithTag(friend_tag);
        enemies = GameObject.FindGameObjectsWithTag(enemy_tag);
        ball = GameObject.FindGameObjectWithTag("Ball");

    }


    private void FixedUpdate()
    {
        float Umax = m_Drone.max_acceleration;
        float Vmax = m_Drone.max_speed;
        Vector3 acc;
        
        Vector3 DesiredSpeed = Kv * (GetGoalieSet() - m_Drone.transform.position) + Kvd *((GetGoalieSet() - m_Drone.transform.position) - OldErr) / Time.fixedDeltaTime;
        acc = Ka * (DesiredSpeed - m_Drone.velocity);
        m_Drone.Move_vect(acc / Umax);
        OldErr = (GetGoalieSet() - m_Drone.transform.position);
            
        
        Debug.DrawLine(ball.transform.position, ball.transform.position + GetBallSpeed(OldBallPos));
        OldBallPos = ball.transform.position;
    }

    public Vector3 GetGoalieSet()
    {
        //Incentro del triangolo tra pali e palla se la palla è lontana
        
        float a, b, c;
        Vector3 palo1 = new Vector3(own_goal.transform.position.x, 0f, own_goal.transform.position.z - 15f);
        Vector3 palo2 = new Vector3(own_goal.transform.position.x, 0f, own_goal.transform.position.z + 15f);
        a = Vector3.Distance(ball.transform.position, palo1);
        b = Vector3.Distance(ball.transform.position, palo2);
        c = Vector3.Distance(palo1, palo2);
        Vector3 GoalieSet = new Vector3((a * palo2.x + b * palo1.x + c * ball.transform.position.x) / (a + b + c), 0f, (a * palo2.z + b * palo1.z + c * ball.transform.position.z) / (a + b + c));
        Debug.DrawLine(ball.transform.position, GoalieSet, Color.green);
        Debug.DrawLine(ball.transform.position, palo1, Color.magenta);
        Debug.DrawLine(ball.transform.position, palo2, Color.magenta);

        if (Vector3.Distance(ball.transform.position, GoalieSet) < 40f) GoalieSet = ((ball.transform.position + GetBallSpeed(OldBallPos)) + ball.transform.position) / 2f;
        if ((friend_tag == "red" && GoalieSet.x > own_goal.transform.position.x) || (friend_tag == "blue" && GoalieSet.x < own_goal.transform.position.x)) GoalieSet = ball.transform.position;
        Debug.DrawLine(m_Drone.transform.position, GoalieSet, Color.black);
        return GoalieSet;
    }

    public Vector3 GetBallSpeed(Vector3 OldPos)
    {
        Vector3 BallVelocity = (ball.transform.position - OldPos) / Time.fixedDeltaTime;
        return BallVelocity;
    }
}

