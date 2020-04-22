using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneIntercep : MonoBehaviour
{
    private DroneController m_Drone;
    // public GameObject own_goal;
    public GameObject other_goal;
    public GameObject ball;
    public string team_tag;

    // 0 : intercept, 1 : shoot
    public int state;

    // Start is called before the first frame update
    void Start()
    {
        m_Drone = GetComponent<DroneController>();
        team_tag = "Red";
        ball = GameObject.FindGameObjectWithTag("Ball");
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        bool goodway = team_tag == "Blue" ? transform.position.x - ball.transform.position.x < 0 : transform.position.x - ball.transform.position.x > 0;
        bool speedBallGoodWay = team_tag == "Blue" ? ball.GetComponent<Rigidbody>().velocity.x < 0 : ball.GetComponent<Rigidbody>().velocity.x > 0;
        if((ball.GetComponent<Rigidbody>().velocity).magnitude < 20f && goodway && speedBallGoodWay)
        {
            state = 1;
        }
        else 
        {
            state = 0;
        }

        if(state == 1)
        {
            m_Drone.Move_vect((ball.transform.position - transform.position).normalized * m_Drone.max_acceleration);
        }
        Vector3 preferedPosition;
        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
        if(state == 0)
        {
            // if(team_tag == "Blue")
            // {
            //     if(ballVelocity.x > 0)
            //     {
            //         Vector3 ballDirection = ballVelocity.normalized;
            //         preferedPosition = new Vector3(125f,0f,100f);
            //     }
            //     else
            //     {
            //         Vector3 direction = ballVelocity.normalized;
            //         preferedPosition = ball.transform.position + 20f * direction;
            //     }
            // }
            // else
            // {
            //     if (ballVelocity.x < 0)
            //     {
            //         Vector3 ballDirection = ballVelocity.normalized;
            //         preferedPosition = new Vector3(175f, 0f, 100f);
            //     }
            //     else
            //     {
            //         Vector3 direction = ballVelocity.normalized;
            //         preferedPosition = ball.transform.position + 20f * direction;
            //     }
            // }
            preferedPosition = OptimalIntercepPosition();
            Vector3 realVector = (preferedPosition - transform.position).normalized * m_Drone.max_acceleration / 1f;
            m_Drone.Move_vect(realVector);
        }
    }

    Vector3 OptimalIntercepPosition()
    {
        Vector3 direction = (ball.transform.position - ball.GetComponent<Rigidbody>().velocity).normalized;
        if(ball.GetComponent<Rigidbody>().velocity.x < 0)
        {
            return ball.transform.position - 10f * direction;
        }
        else
        {
            return ball.transform.position + 10f * direction;
        }
            
    }

}
