// Assets/Scripts/Systems/DayResult.cs
using System.Collections.Generic;

namespace Dayvive
{
    [System.Serializable]
    public class DayResult
    {
        public int dayIndex;                         // Outgame ǥ�ÿ� Day
        public Dictionary<string, int> loot = new(); // RunInventory.All() ������
        public float dayLengthSeconds;               // �Ϸ� ����(�󺧿�, ��� ����)
    }
}
