using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

namespace JobsSample
{
    public class TestController : MonoBehaviour
    {
        [SerializeField] private bool useJobs = false;
        void Start()
        {

        }

        void Update()
        {
            var startTime = Time.realtimeSinceStartup;
            if (useJobs)
            {
                var jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
                for (int i = 0; i < 10; i++)
                {
                    var jobHandle = DoSomeCalculationsJob();
                    jobHandleList.Add(jobHandle);
                }
                JobHandle.CompleteAll(jobHandleList);
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    DoSomeCalculations();
                }
            }
            Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
        }

        private void DoSomeCalculations()
        {
            var value = 0f;
            for (int i = 0; i < 50000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }

        private JobHandle DoSomeCalculationsJob()
        {
            var job = new DoSomeCalculationsJob();
            return job.Schedule();
        }
    }

    [BurstCompile]
    public struct DoSomeCalculationsJob : IJob
    {
        public void Execute()
        {
            var value = 0f;
            for (int i = 0; i < 50000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }
}
