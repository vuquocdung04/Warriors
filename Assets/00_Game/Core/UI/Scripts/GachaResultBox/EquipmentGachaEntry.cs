using UnityEngine;

public class EquipmentGachaEntry : IGachaResultEntry
{
    private readonly GachaService.GachaResult _res;
    public EquipmentGachaEntry(GachaService.GachaResult res) => _res = res;

    public bool IsNew => _res.isFirstOwn;

    public GameObject Spawn(Transform holder)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        EquipmentItem prefab = _res.type switch
        {
            EquipType.Melee => db.GetMeleeItemPrefab(),
            EquipType.Range => db.GetRangeItemPrefab(),
            _ => db.GetShieldItemPrefab(),
        };
        Sprite icon = _res.type switch
        {
            EquipType.Melee => db.GetMeleeIcon(_res.equip.id),
            EquipType.Range => db.GetRangeIcon(_res.equip.id),
            _ => db.GetShieldIcon(_res.equip.id),
        };

        var item = Object.Instantiate(prefab, holder);
        item.Init(_res.equip, _res.type, icon, null);
        item.SetEquipped(false);
        item.SetViewProgress(false);
        item.SetButtonEnabled(false);
        item.SetNew(_res.isFirstOwn);
        return item.gameObject;
    }
}