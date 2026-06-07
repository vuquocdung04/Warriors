#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public static class SheetImportUtil
{
    public static string ToCsvUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        var id = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!id.Success) return null;
        var gid = Regex.Match(url, @"gid=([0-9]+)");
        string g = gid.Success ? gid.Groups[1].Value : "0";
        return $"https://docs.google.com/spreadsheets/d/{id.Groups[1].Value}/export?format=csv&gid={g}";
    }

    public static string Download(string url)
    {
        using (var req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) System.Threading.Thread.Sleep(10);
            if (req.result != UnityWebRequest.Result.Success)
            { Debug.LogError($"[Sheet] Tải lỗi: {req.error}"); return null; }
            return req.downloadHandler.text;
        }
    }

    public static List<List<string>> SplitCsv(string text)
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

    public static float PFloat(string s) =>
        !string.IsNullOrEmpty(s) && float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0f;

    public static int PMoney(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        s = s.Trim().ToLower().Replace(",", "");
        float mul = 1f;
        if (s.EndsWith("k")) { mul = 1000f; s = s[..^1]; }
        else if (s.EndsWith("m")) { mul = 1000000f; s = s[..^1]; }
        return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? Mathf.RoundToInt(v * mul) : 0;
    }

    public static void ParseUnits(string units, System.Action<string, int> onEach)
    {
        if (string.IsNullOrEmpty(units)) return;
        foreach (var part in units.Split(','))
        {
            var p = part.Trim();
            if (p.Length == 0) continue;
            int colon = p.IndexOf(':');
            if (colon < 0) continue;
            string id = p.Substring(0, colon).Trim().ToLower();
            if (int.TryParse(p.Substring(colon + 1).Trim(), out int count) && count > 0)
                onEach(id, count);
        }
    }
}
#endif