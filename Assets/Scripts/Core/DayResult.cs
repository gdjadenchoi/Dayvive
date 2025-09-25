// Assets/Scripts/Systems/DayResult.cs
using System.Collections.Generic;

namespace Dayvive
{
    [System.Serializable]
    public class DayResult
    {
        public int dayIndex;                         // Outgame 표시용 Day
        public Dictionary<string, int> loot = new(); // RunInventory.All() 스냅샷
        public float dayLengthSeconds;               // 하루 길이(라벨용, 없어도 무방)
    }
}
