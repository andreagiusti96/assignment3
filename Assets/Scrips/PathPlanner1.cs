using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityStandardAssets.Vehicles.Car
{
    public class PathPlanner1 : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject terrain_manager_game_object;
        public TerrainManager terrain_manager;

        public GameObject[] drones;

        public List<Node> nodes = new List<Node>();
        //Node[,] nodeMatrix;
        public int scale;
        int[,] mapMatrix;

        private void Start()
        {
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            drones = GameObject.FindGameObjectsWithTag("Player");
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            scale = 2;
            mapMatrix = ExtendMatrix(terrain_manager, scale);

            makeGraph(terrain_manager, mapMatrix);

            initID();

            foreach (GameObject drone in drones)
            {
                Vector3 startPos = drone.transform.position;
                Vector3 goalPos = drone.GetComponent<UnityStandardAssets.Vehicles.Car.CarAI>().my_goal_object.transform.position;
                
                List<Node> path = Astar(startPos, goalPos);

                drone.GetComponent<CarAI>().my_path = path;

                Color c = drone.GetComponent<UnityStandardAssets.Vehicles.Car.CarAI>().my_goal_object.GetComponent<Renderer>().material.color;
                //Debug.DrawLine(startPos, goalPos, Color.black, 100f);
                drawPath(path, c);
            }
        }

        private void Update()
        {


        }

        public int[,] ExtendMatrix(TerrainManager terrainManager, int scale)
        {
            int[,] newMap = new int[terrainManager.myInfo.x_N * scale, terrainManager.myInfo.z_N * scale];

            for (int i = 0; i < terrainManager.myInfo.x_N; i++)
            {
                for (int j = 0; j < terrainManager.myInfo.z_N; j++)
                {
                    for (int k = 0; k < scale; k++)
                    {
                        for (int l = 0; l < scale; l++)
                        {
                            newMap[i * scale + k, j * scale + l] = (int)terrainManager.myInfo.traversability[i, j];
                        }
                    }
                }
            }
            return newMap;
        }


        public void makeGraph(TerrainManager terrain_manager, int[,] traversability)
        {
            float x_size = (terrain_manager.myInfo.x_high - terrain_manager.myInfo.x_low) / traversability.GetLength(0);
            float z_size = (terrain_manager.myInfo.z_high - terrain_manager.myInfo.z_low) / traversability.GetLength(1);

            //nodeMatrix = new Node[traversability.GetLength(0), traversability.GetLength(1)];

            // build nodes matrix

            int nr = 0;
            for (int i = 0; i < traversability.GetLength(0); i++)
            {
                for (int j = 0; j < traversability.GetLength(1); j++)
                {
                    if (traversability[i, j] == 0)
                    {
                        float x = i * x_size + (terrain_manager.myInfo.x_low) + (x_size * 0.5f);
                        float z = j * z_size + (terrain_manager.myInfo.z_low) + (z_size * 0.5f);

                        Vector3 waypoint = new Vector3(x, 0.2f, z);

                        Node node = new Node(nr, waypoint);
                        nodes.Add(node);
                        //nodeMatrix[i, j] = node;
                        nr++;
                    }
                }
            }

            // build edges

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    Node neighbor = nodes[j];
                    if (Vector3.Distance(node.point, neighbor.point) <= 1f * Mathf.Sqrt(x_size * x_size + z_size * z_size) && !Physics.Raycast(node.point, neighbor.point - node.point, Vector3.Distance(node.point, neighbor.point)))
                    {
                        node.addEdge(neighbor.nr, Vector3.Distance(node.point, neighbor.point));
                        neighbor.addEdge(node.nr, Vector3.Distance(node.point, neighbor.point));
                        // Debug.DrawLine(node.point, neighbor.point, Color.cyan, 100f);
                    }
                }
            }

            Debug.Log("graph built with nodes:"+ nodes.Count);
        }

        public List<Node> Astar(Vector3 startPoint, Vector3 goalPoint)
        {
            List<Node> path = new List<Node>();

            if (nodes.Count == 0)
            {
                makeGraph(terrain_manager, mapMatrix);
            }

            Node start = findClosestNodeInSet(startPoint);
            Node goal = findClosestNodeInSet(goalPoint);

            float[] gScores = new float[nodes.Count];
            for (int i = 0; i < gScores.Length; i++) gScores[i] = float.PositiveInfinity;

            float[] fScores = new float[nodes.Count];
            for (int i = 0; i < fScores.Length; i++) fScores[i] = float.PositiveInfinity;

            Edge[] predecessors = new Edge[nodes.Count];

            List<Node> OpenSet = new List<Node>();
            OpenSet.Add(start);
            gScores[start.nr] = 0;
            fScores[start.nr] = Vector3.Distance(start.point, goal.point);

            int iter = 0;

            while (OpenSet.Count > 0)
            {
                Node current = OpenSet[0];
                foreach (Node node in OpenSet)
                {
                    if (fScores[node.nr] < fScores[current.nr]) current = node;
                }

                if (current.point == goal.point)
                {
                    // Debug.Log("goal fund, buildng path...");
                    path.Add(goal);
                    Edge edge = predecessors[goal.nr];
                    while (edge.from != start.nr)
                    {
                        path.Add(nodes[edge.from]);
                        adjustCost(edge.from, edge.destination);
                        edge = predecessors[edge.from];
                    }
                    path.Add(start);
                    path.Reverse();

                    return path;
                }

                OpenSet.Remove(current);

                foreach (Edge edge in current.edges)
                {
                    float tentativeG = gScores[current.nr] + edge.cost;
                    if (tentativeG < gScores[edge.destination])
                    {
                        // Debug.DrawLine(current.point, nodes[edge.destination].point, Color.cyan, 100f);
                        predecessors[edge.destination] = edge;
                        gScores[edge.destination] = tentativeG;
                        fScores[edge.destination] = tentativeG + Vector3.Distance(nodes[edge.destination].point, goal.point);

                        OpenSet.Add(nodes[edge.destination]);
                    }
                }

                // Debug.Log("iteration "+ (iter++) +" expanded edges="+ current.edges.Count +" open set count="+OpenSet.Count);
            }

            Debug.Log("Astar failed !!!!!!!!!");
            return null;
        }

        public void adjustCost(int from, int to)
        {
            Edge forward = nodes[from].edges.Find(edge => edge.destination == to);
            Edge backward = nodes[to].edges.Find(edge => edge.destination == from);
            forward.cost *= 0.9f;
            backward.cost *= 1.5f;
        }

        public void drawPath(List<Node> path, Color color)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Debug.DrawLine(path[i - 1].point, path[i].point, color, 100f);
            }
        }

        public Node findClosestNodeInSet(Vector3 point)
        {
            Node closestNode = nodes[0];
            float distance = float.PositiveInfinity;

            foreach (Node node in nodes)
            {
                if (Vector3.Distance(node.point, point) < distance)
                {
                    distance = Vector3.Distance(node.point, point);
                    closestNode = node;
                }
            }

            // Debug.Log("closest node " + closestNode.point + " to point " + point);
            return closestNode;
        }


        public void initID()
        {
            int i = 0;
            foreach (GameObject drone in drones)
            {
                //car.GetComponent<DroneAI>().carID = i++;
            }
        }

    }

    public class Node
    {
        //public int i;
        //public int j;
        public Vector3 point;
        public int nr;
        public float dangerLevel = 0;
        public List<Edge> edges;

        public Node(int nr, Vector3 point)
        {
            this.point = point;
            this.nr = nr;

            edges = new List<Edge>();
        }

        public void addEdge(int d, float c)
        {
            edges.Add(new Edge(nr, d, c));
        }
    }

    public class Edge
    {
        public int from;
        public int destination;
        public float cost;

        public Edge(int f, int d, float c)
        {
            this.from = f;
            this.destination = d;
            this.cost = c;
        }
    }

}