// Assets/Scripts/Systems/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dayvive
{
    /// <summary>
    /// �� ����/DDOL. Outgame<->Ingame �� ��ȯ�� Day ī���� ���.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Run State")]
        [SerializeField] private int currentDay = 1;   // Outgame ���� ���� �� Day 1
        [SerializeField] private DayResult lastResult; // ���� ���(Outgame���� ���)

        public int CurrentDay => Mathf.Max(1, currentDay);
        public DayResult LastResult => lastResult;

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Ingame���� ȣ��: ��� ���� �� Outgame �ε�</summary>
        public void GoToOutgame(DayResult result)
        {
            lastResult = result;
            SceneManager.LoadScene(SceneNames.Outgame);
        }

        /// <summary>Outgame: "������ ����" �� Day+1 �� Ingame �ε�</summary>
        public void StartNextDay()
        {
            currentDay = Mathf.Max(1, currentDay) + 1;
            SceneManager.LoadScene(SceneNames.Ingame);
        }

        /// <summary>�ʿ� �� �� �� ����</summary>
        public void StartNewRun()
        {
            currentDay = 1;
            lastResult = null;
            SceneManager.LoadScene(SceneNames.Ingame);
        }
    }
}
