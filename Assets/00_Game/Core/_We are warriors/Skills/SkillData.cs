[System.Serializable]
public class SkillData
{
    public string id;
    public string name;
    public int levelMax;
    public float baseValue;
    public float valuePerLevel;
    public string description;

    public float ValueAt(int level) => baseValue + valuePerLevel * (level - 1);

    public string DescAt(int level)
    {
        float v = ValueAt(level);
        string vStr = v % 1 == 0 ? ((int)v).ToString() : v.ToString("0.#");
        return string.IsNullOrEmpty(description) ? "" : description.Replace("{value}", vStr);
    }
    public string DescPreview(int level, bool showNext)
    {
        float cur = ValueAt(level);
        string curStr = Fmt(cur);

        string vStr;
        if (showNext && level < levelMax)
        {
            float delta = valuePerLevel;          
             vStr = $"{curStr} <color=#2ECC40>(+{delta})</color>";
        }
        else
        {
            vStr = curStr;                         
        }

        return string.IsNullOrEmpty(description) ? "" : description.Replace("{value}", vStr);
    }

    static string Fmt(float v) => v % 1 == 0 ? ((int)v).ToString() : v.ToString("0.#");
}