#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSheetImporter : OdinEditorWindow
{
    const string KEY_URL = "UnitSheetImporter_Url";
    const string KEY_NAME = "UnitSheetImporter_JsonName";
    const string KEY_FOLDER = "UnitSheetImporter_Folder";

    [MenuItem("Tools/We Are Warriors/Unit Sheet Importer")]
    static void Open() => GetWindow<UnitSheetImporter>("Unit Sheet Importer");

    [BoxGroup("Nguồn"), LabelText("Google Sheet URL"), ShowInInspector]
    public string SheetUrl
    {
        get => EditorPrefs.GetString(KEY_URL, "");
        set => EditorPrefs.SetString(KEY_URL, value);
    }

    [BoxGroup("Xuất"), LabelText("Tên file JSON"), ShowInInspector]
    public string JsonName
    {
        get => EditorPrefs.GetString(KEY_NAME, "UnitDataJson");
        set => EditorPrefs.SetString(KEY_NAME, value);
    }

    [BoxGroup("Xuất"), LabelText("Thư mục lưu"), FolderPath, ShowInInspector]
    public string SaveFolder      // EditorPrefs -> không reset khi mở lại
    {
        get => EditorPrefs.GetString(KEY_FOLDER, "Assets");
        set => EditorPrefs.SetString(KEY_FOLDER, value);
    }

    [ShowInInspector, ReadOnly, MultiLineProperty(15), LabelText("JSON")]
    private string _json;

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void Fetch()
    {
        string csvUrl = ToCsvUrl(SheetUrl);
        if (csvUrl == null) { Debug.LogError("[UnitSheet] URL không hợp lệ"); return; }

        string csv = Download(csvUrl);
        if (csv == null) return;
        if (csv.TrimStart().StartsWith("<"))
        { Debug.LogError("[UnitSheet] Nhận về HTML — sheet chưa share 'Anyone with link = Viewer'?"); return; }

        var list = ParseCsv(csv);
        _json = JsonConvert.SerializeObject(list, Formatting.Indented);
        Debug.Log($"[UnitSheet] Fetch OK: {list.Count} unit\n{_json}");
    }

    [Button(ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f), EnableIf("@!string.IsNullOrEmpty(_json)")]
    public void Save()
    {
        if (string.IsNullOrEmpty(_json)) { Debug.LogError("[UnitSheet] Chưa có JSON, Fetch trước"); return; }
        if (!Directory.Exists(SaveFolder)) { Debug.LogError($"[UnitSheet] Thư mục không tồn tại: {SaveFolder}"); return; }

        string path = Path.Combine(SaveFolder, JsonName + ".json");
        File.WriteAllText(path, _json, new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log($"[UnitSheet] Đã lưu: {path}");
    }

    static string ToCsvUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        var id = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!id.Success) return null;
        var gid = Regex.Match(url, @"gid=([0-9]+)");
        string g = gid.Success ? gid.Groups[1].Value : "0";
        return $"https://docs.google.com/spreadsheets/d/{id.Groups[1].Value}/export?format=csv&gid={g}";
    }

    static string Download(string url)
    {
        using (var req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) System.Threading.Thread.Sleep(10);
            if (req.result != UnityWebRequest.Result.Success)
            { Debug.LogError($"[UnitSheet] Tải lỗi: {req.error}"); return null; }
            return req.downloadHandler.text;
        }
    }

    static List<UnitData> ParseCsv(string csv)
    {
        var rows = SplitCsv(csv);
        var list = new List<UnitData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cId = Col("id"), cCiv = Col("civ_id"), cName = Col("name"), cType = Col("atk_type"),
            cHp = Col("hp"), cAtk = Col("atk"), cAs = Col("attack_speed"), cMs = Col("move_speed"),
            cCrit = Col("critical_chance"), cLs = Col("life_steal"), cFood = Col("food_cost"), cBuy = Col("buy_price");

        string lastCiv = "";
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string id = Get(row, cId);
            if (string.IsNullOrEmpty(id)) continue;

            string civ = Get(row, cCiv);
            if (!string.IsNullOrEmpty(civ)) lastCiv = civ; else civ = lastCiv;   // ô merge

            string type = Get(row, cType);
            bool isRanged = type.Trim().ToLower().StartsWith("rang");

            list.Add(new UnitData
            {
                id = id,
                civId = civ,
                displayName = Get(row, cName),
                atkType = type,
                frontPriority = isRanged ? 1 : 2,
                hp = PFloat(Get(row, cHp)),
                atk = PFloat(Get(row, cAtk)),
                attackSpeed = PFloat(Get(row, cAs)),
                moveSpeed = PFloat(Get(row, cMs)),
                attackRangeInCells = isRanged ? 4f : 2f,
                criticalChance = PFloat(Get(row, cCrit)),
                lifeSteal = PFloat(Get(row, cLs)),
                foodCost = (int)PFloat(Get(row, cFood)),
                buyPrice = PMoney(Get(row, cBuy)),
            });
        }
        return list;
    }

    static string Get(List<string> row, int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";

    static float PFloat(string s) =>
        !string.IsNullOrEmpty(s) && float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0f;

    static int PMoney(string s)   // "1.6k" -> 1600, "12.8k" -> 12800, "200" -> 200, "" -> 0
    {
        if (string.IsNullOrEmpty(s)) return 0;
        s = s.Trim().ToLower().Replace(",", "");
        float mul = 1f;
        if (s.EndsWith("k")) { mul = 1000f; s = s[..^1]; }
        else if (s.EndsWith("m")) { mul = 1000000f; s = s[..^1]; }
        return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? Mathf.RoundToInt(v * mul) : 0;
    }

    static List<List<string>> SplitCsv(string text)   // tôn trọng "..."
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var sb = new StringBuilder();
        bool q = false;
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (q)
            {
                if (c == '"') { if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i++; } else q = false; }
                else sb.Append(c);
            }
            else
            {
                if (c == '"') q = true;
                else if (c == ',') { row.Add(sb.ToString()); sb.Clear(); }
                else if (c == '\n') { row.Add(sb.ToString()); sb.Clear(); rows.Add(row); row = new List<string>(); }
                else sb.Append(c);
            }
        }
        if (sb.Length > 0 || row.Count > 0) { row.Add(sb.ToString()); rows.Add(row); }
        return rows;
    }
}
#endif