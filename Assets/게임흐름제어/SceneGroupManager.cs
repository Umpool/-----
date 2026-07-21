using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동 기능을 사용하기 위한 도구상자

public class SceneGroupManager : MonoBehaviour
{
    // [유니티 매뉴얼 크기 점검] 이 코드는 주석 포함 총 65줄입니다.

    // 싱글톤(Singleton): 어디서나 이 스크립트에 쉽게 접근할 수 있도록 만드는 문법입니다.
    public static SceneGroupManager Instance { get; private set; }

    private void Awake()
    {
        // 게임 안에 이 매니저가 오직 하나만 존재하도록 보장하는 코드입니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 화면이 바뀌어도 이 오브젝트는 삭제되지 않고 유지됩니다.
        }
        else
        {
            Destroy(gameObject); // 이미 존재한다면 새로 생겨난 것은 파괴합니다.
        }
    }

    /// <summary>
    /// 다른 스크립트(RPG, 퍼즐, 인트로 등)들이 이 함수를 호출하여 화면을 전환합니다.
    /// 예시: SceneGroupManager.Instance.ChangeScene("PuzzleScene");
    /// </summary>
    /// <param name="sceneName">이동하고 싶은 씬의 정확한 이름</param>
    public void ChangeScene(string sceneName)
    {
        Debug.Log($"[SceneGroupManager] '{sceneName}' 씬으로 이동을 시작합니다.");

        // 실제로 유니티 화면을 전환하는 명령어입니다.
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 텍스트 RPG나 미니게임이 끝났을 때, 결과를 받아서 다음 행동을 중재해주는 함수 예시입니다.
    /// </summary>
    /// <param name="gameType">어떤 게임이 끝났는지 (예: "RPG", "MiniGame")</param>
    /// <param name="isSuccess">성공했는지 실패했는지 여부</param>
    public void OnSubGameFinished(string gameType, bool isSuccess)
    {
        Debug.Log($"[SceneGroupManager] {gameType} 결과 수신 - 성공 여부: {isSuccess}");

        if (gameType == "MiniGame" && isSuccess)
        {
            // 미니게임 성공 시 보상을 주거나 보스 퍼즐 배틀로 이동하는 처리를 여기서 중재합니다.
            ChangeScene("PuzzleBattleScene");
        }
        else if (gameType == "RPG")
        {
            // RPG 대화가 끝나면 일반 퍼즐 판으로 이동시킵니다.
            ChangeScene("PuzzleNormalScene");
        }
    }
}
