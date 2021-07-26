using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boid.Classic
{
    public class Boid : MonoBehaviour
    {
        private BoidsManager boidsManager;

        private Vector3 separationForce;
        private Vector3 cohesionForce;
        private Vector3 alignmentForces;
        private Vector3 avoidWallsForce;

        private void Start()
        {
            boidsManager = BoidsManager.Instance;
        }

        private void Update()
        {
            CalculateForce();
            MoveForward();
        }

        private void CalculateForce()
        {
            var seperationSum = Vector3.zero;
            var positionSum = Vector3.zero;
            var headingSum = Vector3.zero;

            int boidsNearby = 0;

            for (int i = 0; i < boidsManager.Boids.Count; ++i)
            {
                var otherBoidsPosition = boidsManager.Boids[i].transform.position;
                float distToOtherBoid = (transform.position - otherBoidsPosition).magnitude;

                if (distToOtherBoid < boidsManager.BoidDetectRadius)
                {
                    seperationSum += -(otherBoidsPosition - transform.position) * (1f / Mathf.Max(distToOtherBoid, .0001f));
                    positionSum += otherBoidsPosition;
                    headingSum += boidsManager.Boids[i].transform.forward;
                    boidsNearby++;
                }
            }

            if (boidsNearby > 0)
            {
                separationForce = seperationSum / boidsNearby;
                cohesionForce = (positionSum / boidsNearby) - transform.position;
                alignmentForces = headingSum / boidsNearby;
            }
            else
            {
                separationForce = Vector3.zero;
                cohesionForce = Vector3.zero;
                alignmentForces = Vector3.zero;
            }

            if (MinDistanceToBorder(transform.position, boidsManager.CageSize) < boidsManager.AvoidWallsTurnDist)
            {
                avoidWallsForce = -transform.position.normalized;
            }
            else
            {
                avoidWallsForce = Vector3.zero;
            }

        }

        private void MoveForward()
        {
            var force = separationForce * boidsManager.SeparationWeight +
                cohesionForce * boidsManager.CohesionWeight +
                alignmentForces * boidsManager.AlignmentWeight +
                avoidWallsForce * boidsManager.AvoidWallsWeight;

            var velocity = transform.forward * boidsManager.BoidSpeed;
            velocity += force * Time.deltaTime;
            velocity = velocity.normalized * boidsManager.BoidSpeed;

            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity);

        }

        private float MinDistanceToBorder(Vector3 postion, float cageSize)
        {
            var halfCageSize = cageSize / 2f;
            return Mathf.Min(Mathf.Min(halfCageSize - Mathf.Abs(postion.x), halfCageSize - Mathf.Abs(postion.y)), halfCageSize - Mathf.Abs(postion.z));
        }
    }
}