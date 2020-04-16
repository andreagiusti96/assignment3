using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MLAgents;
using MLAgents.Sensors;



[RequireComponent(typeof(DroneController))]
public class DroneAISoccer_blue : Agent
{
    private DroneController m_Drone; // the drone controller we want to use

    public GameObject own_goal;
    public GameObject other_goal;
    public GameObject ball;

    Rigidbody rigidB;

    int bScore=0;
    int rScore = 0;
    public int side;// = 1;

    public int DroneID;

    public GameObject[] friends;
    public GameObject[] enemies;

    string friend_tag = "Blue";
    string enemy_tag = "Red";

    float minX = 60f;
    float maxX = 240f;
    float minZ = 55f;
    float maxZ = 145f;

    System.Random rand;


    private void Start()
    {
        m_Drone = GetComponent<DroneController>();
        rigidB = GetComponent<Rigidbody>();

        friends = GameObject.FindGameObjectsWithTag(friend_tag);
        enemies = GameObject.FindGameObjectsWithTag(enemy_tag);

        rand = new System.Random();
    }

    public override void OnEpisodeBegin()
    {
        rigidB.angularVelocity = Vector3.zero;
        rigidB.velocity = Vector3.zero;
        m_Drone.velocity = Vector3.zero;

        //changeTeam();

        float bx = rand.Next(100, 200);
        float bz = rand.Next(80, 120);

        ball.transform.position = new Vector3(bx, 0, bz);
        ball.GetComponent<Rigidbody>().velocity=Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        moveDrones(enemies);
        moveDrones(friends);
        //Debug.DrawLine(transform.position, other_goal.transform.position, Color.white, 10f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions x5
        sensor.AddObservation((transform.position.x - minX) / (maxX - minX));
        sensor.AddObservation((transform.position.z - minZ) / (maxZ - minZ));
        sensor.AddObservation((ball.transform.position.x - transform.position.x) / (maxX - minX));
        sensor.AddObservation((ball.transform.position.z - transform.position.z) / (maxZ - minZ));
        sensor.AddObservation(ball.transform.position.y / 30f);

        // Agent and ball velocity x5
        sensor.AddObservation(m_Drone.velocity.x / m_Drone.max_speed);
        sensor.AddObservation(m_Drone.velocity.z / m_Drone.max_speed);
        sensor.AddObservation((ball.GetComponent<Rigidbody>().velocity-m_Drone.velocity) / m_Drone.max_speed); // x3

        // team info
        sensor.AddObservation(side);

        // other players x8
        GameObject closestFriend = getClosest(friends);
        GameObject closestEnemy = getClosest(enemies);

        sensor.AddObservation((closestFriend.transform.position.x - transform.position.x) / (maxX - minX));
        sensor.AddObservation((closestFriend.transform.position.z - transform.position.z) / (maxZ - minZ));
        sensor.AddObservation((closestFriend.GetComponent<Rigidbody>().velocity.x - m_Drone.velocity.x) / m_Drone.max_speed);
        sensor.AddObservation((closestFriend.GetComponent<Rigidbody>().velocity.z - m_Drone.velocity.z) / m_Drone.max_speed);

        sensor.AddObservation((closestEnemy.transform.position.x - transform.position.x) / (maxX - minX));
        sensor.AddObservation((closestEnemy.transform.position.z - transform.position.z) / (maxZ - minZ));
        sensor.AddObservation((closestEnemy.GetComponent<Rigidbody>().velocity.x - m_Drone.velocity.x) / m_Drone.max_speed);
        sensor.AddObservation((closestEnemy.GetComponent<Rigidbody>().velocity.z - m_Drone.velocity.z) / m_Drone.max_speed);

        //Debug.Log("drone "+ DroneID+" x=" + (transform.position.x - minX) / (maxX - minX) + " z=" + (transform.position.z - minZ) / (maxZ - minZ) +
        //    " dx=" + (ball.transform.position.x - transform.position.x) / (maxX - minX) + " dz=" + (ball.transform.position.z - transform.position.z) / (maxZ - minZ) +
        //    " dv=" + ((ball.GetComponent<Rigidbody>().velocity - m_Drone.velocity) / m_Drone.max_speed).magnitude + 
        //    " side=" + side +
        //    " closestFriend=" + (closestFriend.transform.position.x - transform.position.x) / (maxX - minX) + " dvx=" + (closestFriend.GetComponent<Rigidbody>().velocity.x - m_Drone.velocity.x) / m_Drone.max_speed+
        //    " closest enemy=" + (closestEnemy.transform.position.x - transform.position.x) / (maxX - minX) + " dvx="+ (closestEnemy.GetComponent<Rigidbody>().velocity.x - m_Drone.velocity.x) / m_Drone.max_speed
        //    ) ;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 force = Vector3.zero;
        force.x = vectorAction[0];
        force.z = vectorAction[1];
        m_Drone.Move_vect(force);

        //if (Mathf.Abs(ball.GetComponent<Rigidbody>().velocity.x) + Mathf.Abs(ball.GetComponent<Rigidbody>().velocity.z) > 0.01)
        //{
        //    SetReward(1f);
        //    EndEpisode();
        //}

        //if (side * ball.GetComponent<Rigidbody>().velocity.x > 0)
        //{
        //    SetReward(0.01f);
        //}

        if (ball.GetComponent<GoalCheck>().blue_score > bScore)
        {
            bScore = ball.GetComponent<GoalCheck>().blue_score;
            SetReward(1f * side);
            //Debug.Log("ricompensa=" + 1f * side);
            EndEpisode();
        }
        else if (ball.GetComponent<GoalCheck>().red_score > rScore)
        {
            rScore = ball.GetComponent<GoalCheck>().red_score;
            SetReward(-1f * side);
            //Debug.Log("ricompensa=" + (-1f) * side);
            EndEpisode();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = ball.transform.position.x - transform.position.x;
        action[1] = ball.transform.position.z - transform.position.z;
        return action;
    }

    void changeTeam()
    {
        side *= -1;

        GameObject temp = own_goal;
        own_goal = other_goal;
        other_goal = temp;

        string temps = friend_tag;
        friend_tag = enemy_tag;
        enemy_tag = temps;
    }

    GameObject getClosest(GameObject[] list)
    {
        float min_dist = float.PositiveInfinity;
        GameObject closest= new GameObject();

        foreach(GameObject drone in list)
        {
            float dist = Vector3.Distance(transform.position, drone.transform.position);
            if ( dist < min_dist && dist!=0f )
            {
                closest = drone;
                min_dist = Vector3.Distance(transform.position, closest.transform.position);
            }
        }

        return closest;
    }

    void moveDrones(GameObject[] list)
    {
        foreach (GameObject drone in list)
        {
            float x = rand.Next(70, 230);
            float z = rand.Next(70, 130);

            drone.transform.position = new Vector3(x, 0, z);
        }
    }
}

