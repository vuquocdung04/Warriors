using System.Collections.Generic;

[System.Serializable]
public class EnemyCivConfig
{
    public string civId;
    public int houseHp;
    public List<EnemyWave> waves = new();
}

[System.Serializable]
public class EnemyWave
{
    public int wave;
    public float delayBetweenWave;
    public List<SpawnEntry> spawns = new();
}

[System.Serializable]
public class SpawnEntry
{
    public float spawnDelay;
    public string units;  
}