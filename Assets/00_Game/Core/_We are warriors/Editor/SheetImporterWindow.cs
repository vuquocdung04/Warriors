#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class SheetImporterWindow : OdinEditorWindow
{
    const string KEY_URL = "Sheet_Url_";
    const string KEY_NAME = "Sheet_Name_";
    const string KEY_FOLDER = "Sheet_Folder";

    static readonly ISheetImporter[] Importers =
    {
        new UnitImporter(),
    new HouseImporter(),
    new EquipmentImporter("MeleeEquipment"),
    new EquipmentImporter("RangeEquipment"),
    new EquipmentImporter("ShieldEquipment"),
    new RankImporter(),
    new StatImporter(),
    new GachaLevelImporter(),
    new GachaRateImporter(),
    new SkillImporter(),
    new EnemyWaveImporter(),
    new EconomyImporter(),
    };
    [MenuItem("Tools/We Are Warriors/Sheet Importer")]
    static void Open() => GetWindow<SheetImporterWindow>("Sheet Importer");

    [BoxGroup("Loại data"), ValueDropdown(nameof(TypeNames)), LabelText("Loại"), ShowInInspector]
    public string SelectedType
    {
        get => EditorPrefs.GetString("Sheet_Type", Importers[0].Name);
        set => EditorPrefs.SetString("Sheet_Type", value);
    }
    IEnumerable<string> TypeNames => Importers.Select(i => i.Name);

    [BoxGroup("Nguồn"), LabelText("Google Sheet URL"), ShowInInspector]
    public string SheetUrl
    {
        get => EditorPrefs.GetString(KEY_URL + SelectedType, "");
        set => EditorPrefs.SetString(KEY_URL + SelectedType, value);
    }

    [BoxGroup("Xuất"), LabelText("Tên file JSON"), ShowInInspector]
    public string JsonName
    {
        get => EditorPrefs.GetString(KEY_NAME + SelectedType, SelectedType + "DataJson");
        set => EditorPrefs.SetString(KEY_NAME + SelectedType, value);
    }

    [BoxGroup("Xuất"), LabelText("Thư mục lưu"), FolderPath, ShowInInspector]
    public string SaveFolder
    {
        get => EditorPrefs.GetString(KEY_FOLDER, "Assets");
        set => EditorPrefs.SetString(KEY_FOLDER, value);
    }

    [ShowInInspector, ReadOnly, MultiLineProperty(15), LabelText("JSON")]
    private string _json;

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void Fetch()
    {
        var importer = Importers.First(i => i.Name == SelectedType);
        string csvUrl = SheetImportUtil.ToCsvUrl(SheetUrl);
        if (csvUrl == null) { Debug.LogError("[Sheet] URL không hợp lệ"); return; }

        string csv = SheetImportUtil.Download(csvUrl);
        if (csv == null) return;
        if (csv.TrimStart().StartsWith("<"))
        { Debug.LogError("[Sheet] Nhận HTML — sheet chưa share public?"); return; }

        var rows = SheetImportUtil.SplitCsv(csv);
        int headerRow = FindHeaderRow(rows, importer.AnchorColumn);
        if (headerRow > 0) rows = rows.Skip(headerRow).ToList();

        var data = importer.Parse(rows);
        _json = JsonConvert.SerializeObject(data, Formatting.Indented);
        int count = data is System.Collections.ICollection c ? c.Count : 0;
        Debug.Log($"[Sheet] {SelectedType} OK: header ở row {headerRow + 1}, {count} dòng\n{_json}");
    }

    int FindHeaderRow(List<List<string>> rows, string anchor)
    {
        if (string.IsNullOrEmpty(anchor)) return 0;
        string a = anchor.Trim().ToLower();
        for (int i = 0; i < rows.Count; i++)
            if (rows[i].Any(c => c.Trim().ToLower() == a)) return i;
        return 0;
    }
    [Button(ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f), EnableIf("@!string.IsNullOrEmpty(_json)")]
    public void Save()
    {
        if (string.IsNullOrEmpty(_json)) { Debug.LogError("[Sheet] Chưa Fetch"); return; }
        if (!Directory.Exists(SaveFolder)) { Debug.LogError($"[Sheet] Thư mục sai: {SaveFolder}"); return; }

        string path = Path.Combine(SaveFolder, JsonName + ".json");
        File.WriteAllText(path, _json, new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log($"[Sheet] Đã lưu: {path}");
    }
}
#endif