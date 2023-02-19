using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Base;
using Enemy.Bean;
using System.ComponentModel;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnGroup> enemyGroups;
    [SerializeField] private float castHeight = 100;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Vector2 spawnExtend;
    [SerializeField] private float ticksPerSecond;
    private bool stop;
    
    private float elapsed = 0;
    private float tickDuration;
    private void Start() {
        tickDuration = 1 / ticksPerSecond;
        mask = LayerMask.GetMask("Terrain", "Water");
    }
    public void AttemptToSpawn(Vector2 origin)
    {
        var possibility = Random.Range(1, 2001);
        if (possibility <= 11)
        {
            var enemyGroup = enemyGroups[0];
            var randX = Random.Range(origin.x - spawnExtend.x, origin.x + spawnExtend.x);
            var randZ = Random.Range(origin.y - spawnExtend.y, origin.y + spawnExtend.y);
            var castPos = new Vector3(randX, castHeight, randZ);
            
            if (Physics.Raycast(castPos, Vector3.down, out var hit, castHeight + 10, mask))
            {
                if (hit.collider.tag != "Water")
                {
                    var spawnOrigin = new Vector2(hit.point.x, hit.point.z);
                    SpawnEnemies(enemyGroup, spawnOrigin);
                }
            }

        }
    }
    private void Update() {
        if(!Client.ins.isHost) return;
        //if(stop) return;
        if(DayNightCircle.time < 1.2f) return;
        if(elapsed > tickDuration){
            int numLoop = (int)(elapsed / tickDuration);
            for(int i = 0; i < numLoop; i++){
                var playerList = NetworkManager.ins.GetAllPlayers();
                var randomIndex = Random.Range(0, playerList.Count).ToString();
                var origin = new Vector2(playerList[randomIndex].transform.position.x, playerList[randomIndex].transform.position.z);
                AttemptToSpawn(origin);
            }
            stop = true;
            elapsed -= numLoop * tickDuration;
            return;
        }
        elapsed += Time.deltaTime;
    }
    private void SpawnEnemies(SpawnGroup group, Vector2 origin)
    {
        foreach (var i in group.enemyList)
        {
            for (int j = 0; j < i.number; j++)
            {
                var randX = Random.Range(origin.x - group.groupExtend.x, origin.x + group.groupExtend.x);
                var randZ = Random.Range(origin.y - group.groupExtend.y, origin.y + group.groupExtend.y);
                var castPos = new Vector3(randX, castHeight, randZ);
                if (Physics.Raycast(castPos, Vector3.down, out var hit, castHeight + 10, mask))
                {
                    if (hit.collider.tag == "Water")
                    {
                        j--;
                        continue;
                    }
                    var spawnPosition = hit.point + Vector3.up * 2;
                    var enemy = Instantiate(i.enemy, spawnPosition, Quaternion.identity);
                    var objId = enemy.GetComponent<NetworkSceneObject>().GenerateId();
                    Debug.Log(objId);
                    var spawnPacket = new SpawnEnemyPacket(){
                        spawnPosition =   spawnPosition,
                        enemyNetId = objId
                    };
                    NetworkManager.ins.SpawnRequest(NetworkPlayer.localPlayer.id, i.enemy.GetComponent<NetworkPrefab>(), spawnPosition, enemy.transform.rotation.eulerAngles, objId);
                    
                }
            }
        }
    }
    [System.Serializable]
    class SpawnGroup
    {
        public string name;
        public List<EnemySpawnInfo> enemyList;
        public Vector2 groupExtend;
    }
    [System.Serializable]
    class EnemySpawnInfo
    {
        public EnemyStats enemy;
        public int number;
    }
}
