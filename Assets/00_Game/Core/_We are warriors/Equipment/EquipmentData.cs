using System.Collections.Generic;

[System.Serializable]
public class EquipmentStat
{
    public int slot;
    public string statType;
    public int levelUnlock;
    public float value;          // giá trị cố định (thay lv1..lv5)
}
[System.Serializable]
public class EquipmentData
{
    public string id;
    public string name;
    public string rank;
    public int levelMax;
    public List<EquipmentStat> stats;
}

[System.Serializable]
public class StatData
{
    public string id;
    public string displayName;
    public string calcType;          // flat / percent / rate
}

[System.Serializable]
public class RankData
{
    public string rank;
    public int statCount;
    public int cardPerLevel;
    public float cardMultiplier;
}

[System.Serializable]
public class GachaLevelData
{
    public int level;
    public int spinNeeded;
}

[System.Serializable]
public class GachaRateData
{
    public int level;
    public float common, rare, epic, legend;
}