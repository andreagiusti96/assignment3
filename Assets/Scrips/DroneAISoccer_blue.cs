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
    public int side = 1;

    public int DroneID;

    float minX = 60f;
    float maxX = 240f;
    float minZ = 55f;
    float maxZ = 145f;

    private void Start()
    {
        // get the car controller
        m_Drone = GetComponent<DroneController>();
        rigidB = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rigidB.angularVelocity = Vector3.zero;
        rigidB.velocity = Vector3.zero;
        m_Drone.velocity = Vector3.zero;

        float x = UnityEngine.Random.Range(70, 230);
        float z = UnityEngine.Random.Range(70, 130);
        float bx = UnityEngine.Random.Range(100, 200);
        float bz = UnityEngine.Random.Range(80, 120);
        transform.position = new Vector3(x, 0, z);

        ball.transform.position = new Vector3(bx, 0, bz);
        ball.GetComponent<Rigidbody>().velocity=Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // change team
        GameObject temp = own_goal;
        own_goal = other_goal;
        other_goal = temp;
        side *= -1;
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

        // Agent velocity x5
        sensor.AddObservation(m_Drone.velocity.x / m_Drone.max_speed);
        sensor.AddObservation(m_Drone.velocity.z / m_Drone.max_speed);
        sensor.AddObservation(ball.GetComponent<Rigidbody>().velocity/m_Drone.max_speed);

        // team info
        sensor.AddObservation((other_goal.transform.position.x - minX) / (maxX - minX));
        sensor.AddObservation((own_goal.transform.position.x - minX) / (maxX - minX));

        //Debug.Log("x=" + (transform.position.x - minX) / (maxX - minX) + " z=" + (transform.position.z - minZ) / (maxZ - minZ) +
        //    " dx=" + (ball.transform.position.x - transform.position.x) / (maxX - minX) + " dz="+ (ball.transform.position.z - transform.position.z) / (maxZ - minZ) +
        //    " goal=" + (other_goal.transform.position.x - minX) / (maxX - minX) + " my goal=" + (own_goal.transform.position.x - minX) / (maxX - minX)
        //    );
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 force = Vector3.zero;
        force.x = vectorAction[0];
        force.z = vectorAction[1];
        m_Drone.Move_vect(force);

        //if ( Mathf.Abs(ball.GetComponent<Rigidbody>().velocity.x) + Mathf.Abs(ball.GetComponent<Rigidbody>().velocity.z) > 0.01)
        //{
        //    SetReward(1f);
        //    EndEpisode();
        //}

        if (side * ball.GetComponent<Rigidbody>().velocity.x > 0)
        {
            SetReward(0.01f);
        }

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
}

