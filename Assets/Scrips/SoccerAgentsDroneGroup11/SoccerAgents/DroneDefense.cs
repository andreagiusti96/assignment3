using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneDefense : MonoBehaviour
{
    private DroneController m_Drone; // the drone controller we want to use

        // public GameObject terrain_manager_game_object;
        // TerrainManager terrain_manager;

        // public GameObject[] friends;
        // public string friend_tag;
        // public GameObject[] enemies;
        // public string enemy_tag;

    public GameObject own_goal;
        // public GameObject other_goal;
    public GameObject ball;
    private bool shoot;
    public string team_tag;

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
        shoot = false;
    }

    private void Update()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(team_tag=="Blue")
        {
            Debug.Log(shoot);
        }
        // Debug.Log("shoot :" + shoot);
        bool goodway = team_tag=="Blue" ? transform.position.x - ball.transform.position.x < 0 : transform.position.x - ball.transform.position.x > 0;
        if((transform.position - ball.transform.position).magnitude < 13f && goodway && (transform.position - own_goal.transform.position).magnitude < 30f)
        {
            shoot = true;
        }
        else if(team_tag=="Blue" && ball.transform.position.x < 64)
        {
            shoot = true;
        }
        else if(team_tag=="Red" && ball.transform.position.x > 236)
        {
            shoot = true;
        }
        else
        {
            shoot = false;
        }

        if(shoot == true)
        {
            m_Drone.Move_vect((ball.transform.position - transform.position).normalized * m_Drone.max_acceleration);
        }
        else
        {
            Vector3 preferedPos = OptimalPosition();
            float currentVelocity = m_Drone.GetComponent<Rigidbody>().velocity.magnitude;
            if((preferedPos - transform.position).magnitude > currentVelocity*0.7)
            {
                Vector3 realVector = (preferedPos - transform.position).normalized * m_Drone.max_acceleration;
                m_Drone.Move_vect(realVector);
            }
            else
            {
                Vector3 realVector = (preferedPos - transform.position).normalized * currentVelocity/5f;
                realVector.z = -realVector.z;
                // DebugPlus.DrawSphere(preferedPos, 5f);
                m_Drone.Move_vect(realVector);
            }
            if(team_tag=="Blue")
            {
                // DebugPlus.DrawSphere(preferedPos, 5f);
            }
        }
        
    }

    Vector3 OptimalPosition()
    {
        Vector3 ballSpeed = ball.GetComponent<Rigidbody>().velocity;
        Vector3 ballPos = ball.transform.position;
        if(team_tag == "Blue")
        {
            float optimalX = own_goal.transform.position.x + 3f;
            float optimalZ;
            if(ballPos.z + (own_goal.transform.position.x + 4.2f - ballPos.x)*(ballSpeed.z/ballSpeed.x) < 117.2f && ballPos.z + (own_goal.transform.position.x + 4.2f - ballPos.x) * (ballSpeed.z / ballSpeed.x) > 82.8f)
            {
                optimalZ = ballPos.z + (own_goal.transform.position.x + 4.2f - ballPos.x) * (ballSpeed.z / ballSpeed.x);
                return new Vector3(optimalX,1f,optimalZ);
            }

            // Vector3 goalUp = own_goal.transform.position + new Vector3(0f,0f,15f) - ballPos;
            // Vector3 goalDown = own_goal.transform.position - new Vector3(0f,0f,15f) - ballPos;
            // var halfWayVector = (goalUp + goalDown).normalized;
            // optimalZ = ballPos.z - (halfWayVector.z/halfWayVector.x)*(ballPos.x - own_goal.transform.position.x - 2f);
            // optimalZ = Mathf.Clamp(optimalZ, own_goal.transform.position.z - 15f,own_goal.transform.position.z + 15f);
            optimalZ = 100f + 17.2f*(ballPos.z - 100)/35;
            optimalZ = Mathf.Clamp(optimalZ, own_goal.transform.position.z - 17.2f, own_goal.transform.position.z + 17.2f);
            return new Vector3(optimalX, 1f,optimalZ);
        }
        else
        {
            float optimalX = own_goal.transform.position.x - 3f;
            float optimalZ;
            if (ballPos.z + (own_goal.transform.position.x - 4.2f - ballPos.x) * (ballSpeed.z / ballSpeed.x) < 117.2f && ballPos.z + (own_goal.transform.position.x - 4.2f - ballPos.x) * (ballSpeed.z / ballSpeed.x) > 82.8f)
            {
                optimalZ = ballPos.z + (own_goal.transform.position.x - 4.2f - ballPos.x) * (ballSpeed.z / ballSpeed.x);
                return new Vector3(optimalX, 1f, optimalZ);
            }
            // Vector3 goalUp = own_goal.transform.position + new Vector3(0f,0f,15f) - ballPos;
            // Vector3 goalDown = own_goal.transform.position - new Vector3(0f,0f,15f) - ballPos;
            // var halfWayVector = (goalUp + goalDown).normalized;
            // optimalX = own_goal.transform.position.x - 2f;
            // optimalZ = ballPos.z - (halfWayVector.z/halfWayVector.x)*(ballPos.x - own_goal.transform.position.x + 2f);
            // optimalZ = Mathf.Clamp(optimalZ, own_goal.transform.position.z - 15f,own_goal.transform.position.z + 15f);
            optimalZ = 100f + 17.2f * (ballPos.z - 100) / 35;
            optimalZ = Mathf.Clamp(optimalZ, own_goal.transform.position.z - 17.2f, own_goal.transform.position.z + 17.2f);
            return new Vector3(optimalX, 1f,optimalZ);
        }
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Ball")
        {
            shoot = false;
        }
        if(collision.gameObject.tag == team_tag)
        {
            Vector3 dirCol = (collision.gameObject.transform.position - transform.position).normalized;
            m_Drone.Move_vect(-dirCol*m_Drone.max_acceleration/3);
        }
    }
}
