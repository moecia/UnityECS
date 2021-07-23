using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoBanditsStudio.LowPolyBird
{
    /// <summary>
    /// Flock manager class.
    /// </summary>
    public class FlockManager : MonoBehaviour
    {
        [Header ("Spawm Settings")]
        public List<Bird> birdsPrefab = new List<Bird> (); //Birds prefabs
        public int birdsNumber = 10; //Birds number
        public Vector3 spawnMiddlePoint = Vector3.zero; //Spawn middle point
        public Vector3 spawnBoundaries = Vector3.one; //Span boundaries
        public List<Bird> spawnedBirds; //A list with the flock

        public bool enableRandomScale = true; //Enable random scale
        public float minScale = 0.8f; //Min scale
        public float maxScale = 1.2f; //Max scale

        private GameObject flockParent; //Just to keep the the birds in a single parent

        [Space (15)]
        [Header ("Flock Target Settings")]
        public Vector3 flockTargetPosition = Vector3.zero; //target of flock
        public float minDistanceToTarget = 2f; //the min distance between target and flock birds
        private float initialDistance; //Initial distance
        public Vector3 flockTargetBoxMiddlePoint = Vector3.zero; //the middle point of the flock target might go
        public Vector3 flockTargetBoxSize = new Vector3 (25, 25, 25); //the box where the flock target might be

        /// <summary>
        /// Unity Start function.
        /// </summary>
        private void Start ()
        {
            //Make sure that the birds prefab list is not null
            for (int i=0; i<birdsPrefab.Count; i++)
            {
                if (birdsPrefab[i] == null)
                    birdsPrefab.RemoveAt (i);
            }
            
            //Clear the spawned birds list
            spawnedBirds.Clear ();

            //Flock parent.
            //An empty gameobject that contains all our birds in the flock
            flockParent = new GameObject ("Flock");
            //Spawn the birds
            for (int i = 0; i < birdsNumber; i++)
            {
                //Get a random position
                Vector3 randomPosition = GetRandomPosition (spawnMiddlePoint, spawnBoundaries);

                //Spawn a new bird at a random position
                Bird newBird = (Bird) Instantiate (birdsPrefab[(int) Random.Range (0, birdsPrefab.Count)]);

                //If we enabled the scale, randomize the bird's scale
                if (enableRandomScale)
                {
                    newBird.transform.localScale = Vector3.one * Random.Range (minScale, maxScale);
                }

                //Set the bird's flock manager and set it as a part of the flock
                newBird.flockManager = this;
                newBird.isPartOfFlock = true;

                //Set the position and the parent
                newBird.transform.position = randomPosition;
                newBird.transform.SetParent (flockParent.transform, false);

                //Add the spawned birds to our spawned birds list
                spawnedBirds.Add (newBird);
            }

            //Initial min distance to the target. We will modify this value during update
            initialDistance = minDistanceToTarget;
            //Change the target position
            ChangeTargetPosition ();
        }

        /// <summary>
        /// Get a random position based on the boundaries set.
        /// </summary>
        /// <returns>Random position in boundaries.</returns>
        private Vector3 GetRandomPosition (Vector3 center, Vector3 boundaries)
        {
            //Set a random position.
            //We are spawning based on the middle point set in inspector and the boundaries of spawning.
            return center + new Vector3 (Random.Range (-boundaries.x, boundaries.x), Random.Range (-boundaries.y, boundaries.y), Random.Range (-boundaries.z, boundaries.z)) / 2;
        }

        /// <summary>
        /// Modifies target position
        /// </summary>
        private void ChangeTargetPosition ()
        {
            //Get a random position in our flock target box
            flockTargetPosition = GetRandomPosition (flockTargetBoxMiddlePoint, flockTargetBoxSize);
            //Set a random distance
            minDistanceToTarget = Random.Range (initialDistance - initialDistance / 2, initialDistance + initialDistance / 2);
            //Invoke this function again.
            Invoke ("ChangeTargetPosition", Random.Range (10, 15));
        }

        /// <summary>
        /// On draw gizmos.
        /// </summary>
        private void OnDrawGizmos ()
        {
#if UNITY_EDITOR
            //Spawn settings visualised in the Editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube (spawnMiddlePoint, spawnBoundaries);

            //Target box visualised in the Editor
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube (flockTargetBoxMiddlePoint, flockTargetBoxSize);

            if (UnityEditor.EditorApplication.isPlaying == true)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere (flockTargetPosition, 0.1f);
            }
#endif
        }
    }
}
