public enum EventID
{

    NONE,
    //HOME SCRENE
    CHANGE_COIN = 50,
    CHANGE_HEART = 51,
    CHANGE_AVATAR = 52,
    POPUP_OPENED = 53,
    POPUP_CLOSED = 54,
    BOOSTER_USE_REQUEST = 100,
    BOOSTER_CANCEL_REQUEST = 101,
    BOOSTER_BUY_REQUEST = 102,
    
    LEVEL_COMPLETE = 201,
    BOOSTER_DEACTIVATE_REQUEST = 205,

    // WE ARE WARRIORS
    HOUSE_DESTROYED = 206,   // param: Team (phe nhà bị sập) -> BattleManager nghe để EndBattle
    UNIT_DIED = 207,         // param: Unit (vừa chết) -> UnitDrop sẽ nghe để rơi phần thưởng
    FOOD_CHANGED = 208,      // param: int (food hiện tại) -> UnitCard nghe để đổi màu/bật-tắt nút

    APPLY_EFFECT_ALL_ALLIES = 300,   // buff toàn phe ta (button bấm)
    APPLY_EFFECT_ALL_ENEMIES = 301,  // debuff toàn phe địch


    ON_CIV_CHANGED = 400,
    ON_EQUIPMENT_CHANGED = 401,

    ON_SKILL_CHANGED = 402,

}
