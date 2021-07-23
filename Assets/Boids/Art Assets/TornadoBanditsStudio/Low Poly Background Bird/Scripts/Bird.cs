using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoBanditsStudio.LowPolyBird
{
    /// <summary>
    /// Bird class.
    /// </summary>
    public class Bird : MonoBehaviour
    {
        [Header ("Wings Settings")]
        public Transform leftWing; //Left wing
        public Transform rightWing; //Right wing

        [Range (1, 60)]
        public float wingAngle = 30; //Wing angle to reach before going back to 0

        public float currentWingsRotationSpeed = 3; //the current wings rotation speed
        public float minWingsRotationSpeed = 240; //min wings rotation speed
        public float maxWingsRotationSpeed = 320; //max wings rotation speed

        private bool rotateWingsBack; //is rotating wings back from the wing angle to 0

        [Space (15)]
        [Header ("Movement Settings")]
        public float minSpeed = 0.5f; //min speed 
        public float maxSpeed = 1.5f; //max speed
        public float currentSpeed; //The speed chosen

        public float currentRotationSpeed; //current rotation speed, chose between min and max rotation
        public float minRotationSpeed = 0.1f; //min rotation speed
        public float maxRotationSpeed = 0.5f; //max rotation speed

        public bool enableGliding = true; //Would you like to make the bird plan when it goes down?


        [Space (15)]
        [Header ("Flock settings")]
        public bool isPartOfFlock = false;  //Is this bird part of a flock?
        public FlockManager flockManager; //reference to flock manager
        public bool turningToTarget = false; //is the bird rotating to the target
        public bool isGliding = false;



        /// <summary>
        /// Unity Start function.
        /// </summary>
        private void Start ()
        {
            //Set a random movement speed
            ChangeCurrentSpeed ();

            //We are starning with the wings at 0, 0, 0 euler angles.
            rightWing.transform.localEulerAngles = leftWing.transform.localEulerAngles = Vector3.zero;
            //We will start by rotating it to the chosen angle.
            //When we hit the chosen angle we will go back 0 
            rotateWingsBack = false;
            //Get a random rotation speed.
            currentWingsRotationSpeed = Mathf.Abs (Random.Range (minWingsRotationSpeed, maxWingsRotationSpeed));
            //Set a random rotation speed between the chosen values
            currentRotationSpeed = Random.Range (minRotationSpeed, maxRotationSpeed);
        }

        /// <summary>
        /// Unity Update function.
        /// </summary>
        private void Update ()
        {
            //If this is used as a part of a flock
            if (isPartOfFlock)
            {
                //If we are far away to the target then turn to the target
                turningToTarget = (Vector3.Distance (transform.position, flockManager.flockTargetPosition) >= flockManager.minDistanceToTarget) ? true : false;

                //Turn to the target
                if (turningToTarget)
                {
                    Vector3 dir = flockManager.flockTargetPosition - transform.position;
                    transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (dir), currentRotationSpeed * Time.deltaTime);
                }
                else
                {
                    //Trying to avoid some fps spikes, we will apply the rules for the flock randomly
                    //It is not needed to be called once per frame.
                    float randomFlockLogic = Random.Range (0, 30);
                    if (randomFlockLogic == 5)
                        ApplyFlockingRules ();
                }

                //The bird is always moving forward
                this.transform.Translate (this.transform.forward * currentSpeed * Time.deltaTime, Space.World);

            }

            //If we let the bird plan if it goes down we won't rotate the wings anymore.
            if (enableGliding)
            {
                //If the dot product of the bird's forward and the Up vector is less than -0.1f then is planning is true, which means that we won't have to rotate the wings
                isGliding = Vector3.Dot (this.transform.forward, Vector3.up) < -0.1f;

                //Rotate wings
                if (isGliding == false)
                    RotateWings ();
            } else
            {
                RotateWings ();
            }

        }

        /// <summary>
        /// Rotate the wings
        /// </summary>
        private void RotateWings ()
        {
            //Get the current angle for one of the wing.
            //We only need one of them
            float currentAngle = GetCurrentAngle (this.rightWing.transform.localEulerAngles.z);

            if (rotateWingsBack == false)
            {
                if (currentAngle > wingAngle)
                {
                    currentWingsRotationSpeed *= -1;
                    rotateWingsBack = true;
                }
            }
            else
            {
                //If we are rotating back to 0, check if we hit the 0 value.
                //If it is 0 then go rotate to wing angle 
                if (currentAngle < 0f)
                {
                    rotateWingsBack = false;
                    currentWingsRotationSpeed *= -1;
                }
            }

            //Rotate the wings.
            //Don't forget that we are rotating the left wings to -wing angle
            this.rightWing.Rotate (0, 0, currentWingsRotationSpeed * Time.deltaTime);
            this.leftWing.Rotate (0, 0, -currentWingsRotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Get the current local euler angle, with - sign
        /// </summary>
        /// <param name="localEulerAngle"></param>
        /// <returns></returns>
        float GetCurrentAngle (float localEulerAngle)
        {
            return (localEulerAngle > 180 ? localEulerAngle - 360 : localEulerAngle);
        }

        /// <summary>
        /// Change the speed
        /// </summary>
        private void ChangeCurrentSpeed ()
        {
            currentSpeed = Random.Range (minSpeed, maxSpeed);
        }

        /// <summary>
        /// Apply flocking rules.
        /// </summary>
        void ApplyFlockingRules ()
        {
            //We are calculating the center of a group and make the current bird move to the center of the group
            //The bird might be always face the avarage heading of the group
            //The birds must avoid the closer neighbours

            //To get the center we will add all positions of the birds and divede it with the count of birds
            Vector3 groupCenter = Vector3.zero;
            //The avoidance. 
            Vector3 avoidanceVector = Vector3.zero;
            //Distance between 2 birds
            float distance;
            //We will need the list of all the birds existent, that we have in the FlockManager 
            for (int i = 0; i < flockManager.spawnedBirds.Count; i++)
            {
                Bird currentBird = flockManager.spawnedBirds[i];
                //Don't do that for the current bird
                if (currentBird != this)
                {
                    //Get the distance between the 2 birds
                    distance = Vector3.Distance (this.transform.position, currentBird.transform.position);

                    //If we are closer than 1 meter than add the diference between this position and the current bird position
                    if (distance < 1f)
                    {
                        avoidanceVector += (this.transform.position - currentBird.transform.position);
                    }
                    //Add the bird to the group
                    groupCenter += currentBird.transform.position;
                }
            }

            //Divie the group center
            groupCenter /= (flockManager.spawnedBirds.Count - 1);
            //Set the direction based on target position, the group center and the avoidance
            Vector3 direction = (groupCenter + flockManager.flockTargetPosition + avoidanceVector) - this.transform.position;
            //Rotate the bird
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (direction), currentRotationSpeed * Time.deltaTime);
        }
    }
}
