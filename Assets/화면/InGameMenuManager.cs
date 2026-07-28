using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    [Header("중앙에서 데드락 물결 연출로 켜고 닫힐 [팝업창] 오브젝트")]
    [SerializeField] private GameObject popupPanel;

    [Header("물결 퍼짐 속도 (초 단위)")]
    [SerializeField] private float animationDuration = 0.15f;

    private Coroutine animationCoroutine;

    private void Start()
    {
        // [기획 흐름] 상단바 UI 자체는 절대 끄지 않고 무조건 켜진 상태를 유지합니다.
        // 오직 유저 눈을 가리고 있을 자식 [팝업창]만 시작할 때 얌전히 숨겨둡니다.
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 1. 톱니바퀴(G) 버튼을 클릭했을 때: 데드락 생성 효과처럼 중앙에서부터 크기를 키우며 여는 함수
    /// </summary>
    public void OnClickOpenMenu()
    {
        if (popupPanel != null)
        {
            if (popupPanel.activeSelf) return; // 이미 열려있다면 중복 작동 방지

            popupPanel.SetActive(true);

            // [데드락 이식] 0(Vector3.zero) 크기에서 시작하여 원래 1(Vector3.one) 크기까지 부드럽게 확대 가동
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateScaleRoutine(Vector3.zero, Vector3.one));

            Debug.Log("[Menu] 데드락 효과 연동: 팝업창이 중앙에서부터 뿅! 하고 커지며 등장합니다.");
        }
    }

    /// <summary>
    /// 2. 팝업창 내부의 [닫기] 버튼을 클릭했을 때: 데드락 소멸 효과처럼 중심점으로 쏙 작아지며 닫히는 함수
    /// </summary>
    public void OnClickCloseMenu()
    {
        if (popupPanel != null)
        {
            if (!popupPanel.activeSelf) return;

            // [데드락 이식] 원래 1(Vector3.one) 크기에서 다시 0(Vector3.zero) 크기로 쏙 축소 가동 (애니메이션 끝난 후 끄기 옵션 장착)
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateScaleRoutine(Vector3.one, Vector3.zero, true));

            Debug.Log("[Menu] 데드락 효과 연동: 팝업창이 중심점으로 쏙 축소되며 퇴장합니다.");
        }
    }

    /// <summary>
    /// Board.cs의 데드락 연출용 픽셀 스케일 계산식(AnimateScaleUpUI)을 100% 동일한 부드러운 Lerp 감성으로 이식한 핵심 엔진
    /// </summary>
    private System.Collections.IEnumerator AnimateScaleRoutine(Vector3 startScale, Vector3 endScale, bool disableOnEnd = false)
    {
        float elapsed = 0f;
        RectTransform panelRect = popupPanel.GetComponent<RectTransform>();

        if (panelRect != null)
        {
            panelRect.localScale = startScale;

            while (elapsed < animationDuration)
            {
                if (panelRect == null) yield break; // 중간에 방이 깨지거나 파괴될 때를 대비한 안전 방어선

                elapsed += Time.deltaTime;

                // Board.cs의 슬라이딩 및 스케일 업 기법과 정확히 동기화된 순수 시간 비례 선형 보간 연산
                panelRect.localScale = Vector3.Lerp(startScale, endScale, elapsed / animationDuration);
                yield return null;
            }

            if (panelRect != null) panelRect.localScale = endScale; // 소수점 오차 칼고정
        }

        // 닫기 연출이 완전히 끝난 시점에만 방 상자를 꺼서 화면에서 소멸시킵니다.
        if (disableOnEnd)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 3. [타이틀화면으로] 버튼 클릭 함수
    /// </summary>
    /// <summary>
    /// 3. 드롭다운 목록 중 [타이틀로 이동] 버튼을 눌렀을 때 실행되는 함수입니다.
    /// </summary>
    public void OnClickReturnToTitle()
    {
        Debug.Log("[Menu] 기획 흐름: 씬 로딩 없이, 코드로 타이틀 화면을 직접 추적하여 강제 활성화합니다.");

        // [버그 박멸 핵심] 메모리에 돌고 있을지 모르는 모든 유령 Invoke 타이머를 완벽하게 포맷합니다.
        CancelInvoke();

        // 현재 게임 월드(씬)에 살아 숨 쉬는 모든 오브젝트들을 스캔하여 진짜 실물 "타이틀"과 "마을"을 찾아냅니다.
        Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
        string currentActiveSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        foreach (Transform obj in allObjects)
        {
            // 프로젝트 창고의 가짜 프리팹 원본들은 가차 없이 필터링하여 제외합니다!
            if (obj.gameObject.scene.name == currentActiveSceneName)
            {
                // [진짜 실물 타이틀 화면 발견] 코드로 확실하게 켜줍니다!
                if (obj.name == "타이틀")
                {
                    obj.gameObject.SetActive(true);
                    Debug.Log("[Menu] 진짜 눈앞의 실물 '타이틀' 패널을 성공적으로 켰습니다.");
                }

                // [진짜 실물 마을 화면 발견] 이제 타이틀로 가니까 마을 화면은 강제로 쾅 닫아줍니다!
                if (obj.name == "마을")
                {
                    obj.gameObject.SetActive(false);
                }

                // 두 번째 이벤트 화면도 혹시 모를 오작동 방지를 위해 철저하게 꺼줍니다.
                if (obj.name == "두번째 이벤트 화면")
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }

        // 임무를 마친 환경설정 팝업창 자기 자신도 알아서 깔끔하게 닫힙니다.
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// 4. [게임종료] 버튼 클릭 함수

    public void OnClickQuitGame()
    {
        Debug.Log("[Menu] 기획 흐름: 유저가 종료 버튼 클릭함 -> 💾 모든 인게임 데이터를 총통틀어 안전하게 영구 저장 후 게임을 종료합니다.");

        // [최종 데이터 철통 저장 1단계] 
        // 실시간 메모리에만 들고 있던 "유저가 획득한 전체 캐릭터 가방 명단"을 문자열로 압축하여 저장 창고에 밀어 넣습니다.
        if (UserCharacterInventory.Instance != null)
        {
            UserCharacterInventory.Instance.SaveInventoryData();
            Debug.Log("[Menu] 캐릭터 인벤토리 가방 데이터 세이브 성공!");
        }

        // [최종 데이터 철통 저장 2단계]
        // 현재까지 쌓인 골드, 이야기 진행도, 닉네임, 캐릭터ID 등 모든 PlayerPrefs 데이터를 컴퓨터 하드디스크에 물리적으로 즉시 쓰기 명령합니다.
        PlayerPrefs.Save();
        Debug.Log("[Menu] 전체 게임 데이터(골드, 닉네임, 진행도 등) 마스터 하드디스크 영구 저장 완료!");


        // ----------------------------------------------------
        // [안전한 프로그램 완전 종료 가동]
        // ----------------------------------------------------
        // 실제 빌드된 PC 프로그램이나 모바일 게임 소프트웨어인 경우 앱을 아예 강제로 꺼버립니다.
        Application.Quit();

#if UNITY_EDITOR
        // 만약 유니티 에디터 창에서 재생(▶) 버튼을 누르고 테스트 중인 상황이라면, 플레이 모드를 강제로 안전하게 멈춰줍니다.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void OnClickGoToCharacterStorage()
    {
        Debug.Log("[Menu] 기획 흐름: 유저가 캐릭터 보관함 입장을 요청함 -> '캐릭터 저장고' 씬을 신선하게 로딩합니다.");
        
        // [안전장치] 혹시라도 메모리에 남아있을 수 있는 유령 지연 타이머들을 포맷합니다.
        CancelInvoke();

        // 💡 [세이브 강제 주입]: 처음 뽑은 메인 캐릭터 데이터가 씬 이동 중 유실되지 않도록 하드디스크에 물리 저장합니다!
        PlayerPrefs.Save();

        // 유니티 공식 화면 전환 기능을 사용해 빌드 프로필에 등록될 씬 이름("캐릭터 저장고")을 호출합니다.
        UnityEngine.SceneManagement.SceneManager.LoadScene("캐릭터 저장고");
    }


}
