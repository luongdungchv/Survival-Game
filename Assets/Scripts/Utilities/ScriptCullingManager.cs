using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptCullingManager : MonoBehaviour
{
    public static ScriptCullingManager ins;
    private NativeList<CullingProp> cullingList;
    private List<GameObject> objList;
    private void Awake() {
        ins = this;
    }
    void Start()
    {
        cullingList = new NativeList<CullingProp>(Allocator.Persistent);
        objList = new List<GameObject>();
    }
    bool start = true;

    // Update is called once per frame
    void Update()
    {
        // NativeList<CullingProp> result = new NativeList<CullingProp>(Allocator.TempJob);
        // var cullingJob = new CullingJob(){
        //     allProps = cullingList,
        //     resultProps = result,
        //     playerPos = NetworkPlayer.localPlayer.transform.position
        // };
        // result.Capacity = cullingList.Length;
        // var handle = cullingJob.Schedule(cullingList.Length, Mathf.CeilToInt(cullingList.Length / 10f));
        // handle.Complete();
        // foreach(var i in result){
        //     objList[i.index].GetComponent<FixedSizeUI>().UpdateMethod();
        // }
        // start = false;
        
        // result.Dispose();
    }
    public void AddToCullingList(GameObject cullingObj, float distance){
        var prop = new CullingProp(){
            position = cullingObj.transform.position,
            cullingDist = distance,
            index = cullingList.Length
        };
        cullingList.Add(prop);
        objList.Add(cullingObj);
    }
}
public struct CullingProp{
    public int index;
    public float3 position;
    public float cullingDist;
}
[BurstCompile]
public struct CullingJob : IJobParallelFor
{
    [ReadOnly] public NativeList<CullingProp> allProps;
    [NativeDisableParallelForRestriction] public NativeList<CullingProp> resultProps;
    public float3 playerPos;

    public void Execute(int index)
    {
        var prop = allProps[index];
        var distance = math.distance(prop.position, playerPos);
        if(distance > prop.cullingDist) return;
        resultProps.Add(prop);
    }
}
