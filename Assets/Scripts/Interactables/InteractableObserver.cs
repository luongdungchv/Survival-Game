using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Linq;

public class InteractableObserver : MonoBehaviour
{
    public static InteractableObserver instance;
    [SerializeField] private List<InteractableObject> interactables;
    [SerializeField] private NativeList<Vector3> posList;
    public void SubscribeInteractable(InteractableObject obj){
        this.interactables.Add(obj);
        posList.Add(obj.transform.position);
    }
    
    private void Awake() {
        instance = this;
        this.interactables = new List<InteractableObject>();
        posList = new NativeList<Vector3>(Allocator.Persistent);
    }
    private void Update(){
        var scheduleBuffer = new NativeArray<bool>(interactables.Count, Allocator.TempJob);
        var posBuffer = posList.AsArray();
        var job = new InteractableUpdateJob(){
            stateList = scheduleBuffer,
            posList = posBuffer,
            playerPos = NetworkPlayer.localPlayer.transform.position,
            threshold = 10
        };
        var handle = job.Schedule(interactables.Count, Mathf.CeilToInt(interactables.Count / 8));
        handle.Complete();
        for (int i = 0; i < interactables.Count; i++)
        {
            if(job.stateList[i]){
                interactables[i]?.OnUpdate();
            }
        }
        scheduleBuffer.Dispose();
    }

    [BurstCompile]
    public struct InteractableUpdateJob : IJobParallelFor
    {
        public NativeArray<bool> stateList;
        public NativeArray<Vector3> posList;
        public Vector3 playerPos;
        public float threshold;
        public void Execute(int index)
        {
            var pos = posList[index];
            stateList[index] = (pos - playerPos).sqrMagnitude < threshold;
        }
    }
}
