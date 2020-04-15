using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlannerBlue : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject terrain_manager_game_object;
    public TerrainManager terrain_manager;

    public GameObject[] friends;
    public string friend_tag;
    public GameObject[] enemies;
    public string enemy_tag;

    private void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

        friends = GameObject.FindGameObjectsWithTag("Blue");

        AssignID();
    }

    private void Update()
    {
    }

    public void AssignID()
    {
        int k = 0;
        foreach (GameObject friend in friends)
        {
         //   friend.GetComponent<DroneAISoccer_blue>().DroneID = k++;
        }
    }
}

