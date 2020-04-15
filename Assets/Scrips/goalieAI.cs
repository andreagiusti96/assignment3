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
    float Kv = 15f;
    float Kvd = 15f;
    float Ka = 15f;
    Vector3 BallVelocity;
    Vector3 OldBallPos = Vector3.zero;
    Vector3 OldErr = Vector3.zero;
    Vector3 OldErrVel = Vector3.zero;
    bool uscita = false;

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
        Vector3 target = GetGoalieSet();
        Vector3 DesiredSpeed = Kv * (target - m_Drone.transform.position) + Kvd * ((target - m_Drone.transform.position) - OldErr) / Time.fixedDeltaTime;
        if (Vector3.Distance(ball.transform.position, target) < 10f)
        {
            DesiredSpeed = ball.GetComponent<Rigidbody>().velocity + 2 * (ball.transform.position - m_Drone.transform.position); //velocity to have collision with ball
            Debug.DrawLine(m_Drone.transform.position, m_Drone.transform.position + m_Drone.velocity);
        }
        acc = Ka * (DesiredSpeed - m_Drone.velocity);
        m_Drone.Move_vect(acc);
        OldErr = (target - m_Drone.transform.position);
        OldErrVel = DesiredSpeed - m_Drone.velocity;


        Debug.DrawLine(ball.transform.position, ball.transform.position + ball.GetComponent<Rigidbody>().velocity);
        OldBallPos = ball.transform.position;
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
        Debug.DrawLine(ball.transform.position, GoalieSet, Color.green);
        Debug.DrawLine(ball.transform.position, palo1, Color.magenta);
        Debug.DrawLine(ball.transform.position, palo2, Color.magenta);

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

