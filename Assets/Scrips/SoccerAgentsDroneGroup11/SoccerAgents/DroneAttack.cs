using System.Collections;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAttack : MonoBehaviour
{
    private DroneController m_Drone;
    // public GameObject own_goal;
    public GameObject other_goal;
    public GameObject ball;
    public string team_tag;

    // 0 : waiting, 1 : gotoball, 2 : shoot
    public int state;



    // Start is called before the first frame update
    private void Start()
    {
        m_Drone = GetComponent<DroneController>();
        // terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();


        // note that both arrays will have holes when objects are destroyed
        // but for initial planning they should work
        // friend_tag = gameObject.tag;
        // if (friend_tag == "Blue")
        //     enemy_tag = "Red";
        // else
        //     enemy_tag = "Blue";

        // friends = GameObject.FindGameObjectsWithTag(friend_tag);
        // enemies = GameObject.FindGameObjectsWithTag(enemy_tag);
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
        // if((ball.transform.position.x < 117f && team_tag=="Blue") || (ball.transform.position.x > 183f && team_tag == "Red"))
        // {
        //     state = 0;
        // }
        // else
        // {
            bool goodway = team_tag == "Blue" ? transform.position.x - ball.transform.position.x < 0 : transform.position.x - ball.transform.position.x > 0;
            if ((transform.position - ball.transform.position).magnitude < 10 && goodway)
            {
                state = 2;
            }
            else
            {
                state = 1;
            }
        // }

        if(state == 0)
        {
            Vector3 preferedPos = OptimalWaitingPosition();
            Vector3 realVector = (preferedPos - transform.position).normalized * m_Drone.max_acceleration / 3f;
            m_Drone.Move_vect(realVector);
        }
        else if(state == 1)
        {
            Vector3 preferedPos = OptimalShootPosition();
            Vector3 realVector = (preferedPos - transform.position).normalized * m_Drone.max_acceleration / 1f;
            m_Drone.Move_vect(realVector);
        }
        else
        {
            m_Drone.Move_vect((ball.transform.position - transform.position).normalized * m_Drone.max_acceleration);
        }
        if(team_tag=="Red")
        {
            // Debug.Log("State :" + state);
        }

    }

    Vector3 OptimalShootPosition()
    {
        Vector3 direction = (other_goal.transform.position - ball.transform.position).normalized;
        return ball.transform.position - 5f*direction;
    }

    Vector3 OptimalWaitingPosition()
    {
        float preferedX, preferedZ;
        if(team_tag=="Blue")
        {
            preferedX = 117f;
            preferedZ = 105f;
        }
        else
        {
            preferedX = 183f;
            preferedZ = 95f;
        }
        // float preferedZ = 100f;
        return new Vector3(preferedX, 0f, preferedZ);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ball")
        {
            state = 1;
        }
        if (collision.gameObject.name == "Drone")
        {
            Vector3 dirCol = (collision.gameObject.transform.position - transform.position).normalized;
            m_Drone.Move_vect(-dirCol * m_Drone.max_acceleration / 3);
        }
    }
}
