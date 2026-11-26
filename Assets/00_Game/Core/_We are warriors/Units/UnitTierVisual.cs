using System.Collections.Generic;
using UnityEngine;

// Script trên unit cần đổi hành vi theo bậc (vd đạn) thì implement, UnitTierVisual.Apply sẽ gọi
public interface ITierAware
{
    void ApplyTier(int tier);
}

// Đổi art unit theo bậc (gọi trước Unit.Init). tiers[0] = bậc 1, tiers[1] = bậc 2; bậc 0 giữ nguyên prefab.
public class UnitTierVisual : MonoBehaviour
{
    [System.Serializable]
    public class SpriteOverride
    {
        public SpriteRenderer target;
        public bool hide;
        public Sprite sprite;
        public Vector3 localPosition;
        public Vector3 localEuler;
        public Vector3 localScale = Vector3.one;
        public int sortingOrder;
        public bool overrideColor;
        public Color color = Color.white;
    }

    [System.Serializable]
    public class Tier
    {
        public List<SpriteOverride> overrides = new();
        [Tooltip("Món đeo thêm của bậc này (để ẩn sẵn trong prefab)")]
        public List<GameObject> show = new();
    }

    public List<Tier> tiers = new();

    public void Apply(int tier)
    {
        if (tier <= 0 || tier > tiers.Count) return;
        var t = tiers[tier - 1];

        foreach (var o in t.overrides)
        {
            if (o.target == null) continue;
            if (o.hide) { o.target.gameObject.SetActive(false); continue; }

            o.target.gameObject.SetActive(true);
            o.target.sprite = o.sprite;
            o.target.sortingOrder = o.sortingOrder;
            if (o.overrideColor) o.target.color = o.color;
            var tr = o.target.transform;
            tr.localPosition = o.localPosition;
            tr.localEulerAngles = o.localEuler;
            tr.localScale = o.localScale;
        }
        // Món đeo thêm phải theo mask của unit (vd bị nhà che lúc mới spawn như các phần gốc)
        var refSr = t.overrides.Count > 0 ? t.overrides[0].target : null;
        foreach (var go in t.show)
        {
            if (go == null) continue;
            go.SetActive(true);
            if (refSr != null && go.TryGetComponent(out SpriteRenderer sr)) sr.maskInteraction = refSr.maskInteraction;
        }
        foreach (var ta in GetComponentsInChildren<ITierAware>(true)) ta.ApplyTier(tier);
    }
}
