using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Boid.Classic
{
    public class BoidsManager : MonoBehaviour
    {
        public static BoidsManager Instance;

        public float BoidSpeed = 20;
        public float BoidDetectRadius = 50;
        public float CageSize = 500;
        public int IncrementAmount = 50;

        public float SeparationWeight = 25;
        public float CohesionWeight = 5;
        public float AlignmentWeight = 15;

        public float AvoidWallsWeight = 10;
        public float AvoidWallsTurnDist = 20;

        public List<Boid> Boids;

        [SerializeField] private int boidAmount = 10;
        [SerializeField] private GameObject boidPrefab;
        [SerializeField] private Text statsText;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
            Boids.Clear();
            AddBoid(boidAmount);
        }

        private void Update()
        {
            statsText.text = $"FPS: {(int)(1.0f / Time.deltaTime)}\n" +
                $"Total boids: {Boids.Count}";
        }

        public void AddBoid(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var pos = new Vector3(Random.Range(-CageSize / 2f, CageSize / 2f),
                    Random.Range(-CageSize / 2f, CageSize / 2f),
                    Random.Range(-CageSize / 2f, CageSize / 2f));
                var rot = Quaternion.Euler(Random.Range(0, 360),
                    Random.Range(0, 360),
                    Random.Range(0, 360));

                var newBoid = Instantiate(boidPrefab, pos, rot).GetComponent<Boid>();
                Boids.Add(newBoid);
            }
        }

        private void OnDrawGizmos()
        {

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                Vector3.zero,
                new Vector3(
                    CageSize,
                    CageSize,
                    CageSize
                )
            );
        }
    }

    [CustomEditor(typeof(BoidsManager))]
    public class BoidsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var boidsManager = (BoidsManager)target;
            if (GUILayout.Button("Add 50 Boids"))
            {
                boidsManager.AddBoid(boidsManager.IncrementAmount);
            }
        }
    }
}