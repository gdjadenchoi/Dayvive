// Assets/Scripts/Systems/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dayvive
{
    /// <summary>
    /// 런 상태/DDOL. Outgame<->Ingame 씬 전환과 Day 카운팅 담당.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Run State")]
        [SerializeField] private int currentDay = 1;   // Outgame 최초 진입 시 Day 1
        [SerializeField] private DayResult lastResult; // 직전 결과(Outgame에서 사용)

        public int CurrentDay => Mathf.Max(1, currentDay);
        public DayResult LastResult => lastResult;

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Ingame에서 호출: 결과 저장 후 Outgame 로드</summary>
        public void GoToOutgame(DayResult result)
        {
            lastResult = result;
            SceneManager.LoadScene(SceneNames.Outgame);
        }

        /// <summary>Outgame: "다음날 시작" → Day+1 후 Ingame 로드</summary>
        public void StartNextDay()
        {
            currentDay = Mathf.Max(1, currentDay) + 1;
            SceneManager.LoadScene(SceneNames.Ingame);
        }

        /// <summary>필요 시 새 런 시작</summary>
        public void StartNewRun()
        {
            currentDay = 1;
            lastResult = null;
            SceneManager.LoadScene(SceneNames.Ingame);
        }
    }
}
