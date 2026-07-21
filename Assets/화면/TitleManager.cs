using UnityEngine;
using TMPro;

public class TitleManager : MonoBehaviour
{
    // [유니티 매뉴얼] 이 코드는 주석 포함 총 95줄입니다.

    [Header("버튼 오브젝트 설정")]
    [Tooltip("새로운 모험 버튼")] public GameObject newGameButton;
    [Tooltip("이어하기 버튼")] public GameObject continueButton;

    [Header("팝업 및 텍스트 설정")]
    [Tooltip("설정 창 패널 오브젝트 (팝업창)")] public GameObject settingsPopupPanel;
    [Tooltip("iOS 종료 안내 텍스트")] public TextMeshProUGUI iosNoticeText;

    // [유니티 매뉴얼] 이 코드 조각은 주석 포함 총 10줄입니다. 변수 선언부에 추가하세요.
    [Header("새로운 모험 흐름 설정 (오브젝트 방식)")]
    [Tooltip("타이틀 다음으로 켜줄 첫 이벤트 화면 오브젝트입니다.")]
    public GameObject firstEventPanel;

    [Tooltip("이벤트 다음에 켜줄 메인 캐릭터 선택창 오브젝트입니다.")]
    public GameObject characterSelectPanel;

    [Tooltip("이어하기 시 바로 켜줄 최종 마을 화면 패널 오브젝트입니다.")]
    public GameObject villagePanel;


    // [중요] 유니티 에디터 창이나 코드 상에서 테스트용으로 데이터를 껐다 켰다 할 수 있게 만듭니다.
    private bool hasUserData = false;

    private void Start()
    {
        // ----------------------------------------------------
        // 👑 [수집형 RPG 정석: 캐릭터 창고 복귀 유저 다이렉트 패스 장치]
        // ----------------------------------------------------
        if (PlayerPrefs.HasKey("IsReturningFromStorage") && PlayerPrefs.GetInt("IsReturningFromStorage") == 1)
        {
            Debug.Log("[TitleManager] 정석 복귀 인지 완료: 캐릭터 보관함에서 돌아온 유저입니다! 인트로/타이틀을 올-패스하고 마을 패널을 강제 활성화합니다.");

            // 1. 세이브/복귀 정보에 근거하여, 오직 실물 '마을 패널'과 '상단바'만 보란 듯이 즉시 켭니다.
            if (villagePanel != null) villagePanel.SetActive(true);

            Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
            string currentActiveSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            foreach (Transform obj in allObjects)
            {
                if (obj.gameObject.scene.name == currentActiveSceneName && obj.name == "상단바")
                {
                    obj.gameObject.SetActive(true);
                }
            }

            // 2. 유저의 복귀를 방해할 첫 이벤트, 캐릭터 선택창, 타이틀 본체는 철저하게 OFF 꺼버립니다.
            if (firstEventPanel != null) firstEventPanel.SetActive(false);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(false);

            // 3. 임무를 안전하게 마친 복귀 유저 신호 도장은 다음 플레이를 위해 깔끔하게 0으로 리셋합니다.
            PlayerPrefs.SetInt("IsReturningFromStorage", 0);
            PlayerPrefs.Save();

            gameObject.SetActive(false); // 타이틀 제어 스크립트 본체 퇴장 마감
            return; // ★ 일반 정석 타이틀 오프닝 로직은 가차 없이 스킵(Skip) 차단합니다!
        }

        // ----------------------------------------------------
        // 🌀 [기존 정석 시작 로직 구역 - 변함없이 100% 안전 보존]
        // ----------------------------------------------------
        if (TryGetComponent<Canvas>(out Canvas titleCanvas)) { titleCanvas.overrideSorting = true; titleCanvas.sortingOrder = 99; }
        if (settingsPopupPanel != null) settingsPopupPanel.SetActive(false);
        if (iosNoticeText != null) iosNoticeText.gameObject.SetActive(false);
        if (firstEventPanel != null) firstEventPanel.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        CheckUserDataOnly();
        SetupTitleButtons();
    }



    // [유니티 매뉴얼] TitleManager.cs의 기존 데이터 검사 함수를 지우고, 이 두 개의 독립된 함수로 갈아끼우세요.

    /// <summary>
    /// [독립 기능 1] 순수하게 데이터 유무만 '판단'하여 결과만 기록하는 함수입니다.
    /// </summary>
    private void CheckUserDataOnly()
    {
        Debug.Log("[TitleManager] 1단계: 순수 데이터 유무 검사 가동...");

        // 두 번째 이벤트씬에서 저장한 통합 마스터 열쇠(HasSaveData)를 검사하므로 
        // 앞으로 골드가 추가되든, 진행도가 추가되든 이 한 줄로 타이틀 버튼이 완벽하게 통제됩니다.
        if (PlayerPrefs.HasKey("HasSaveData"))
        {
            hasUserData = true;
            Debug.Log("[TitleManager] 데이터 판단 결과 -> 💾 기존 통합 세이브 데이터가 존재합니다.");
        }
        else
        {
            hasUserData = false;
            Debug.Log("[TitleManager] 데이터 판단 결과 -> 🆕 세이브 데이터가 없습니다.");
        }
    }


    /// <summary>
    /// [독립 기능 2] 기록된 판단 결과 변수(hasUserData)만 보고 실질적으로 버튼을 '켜고 끄는' 따로 제어하는 함수입니다.
    /// </summary>
    private void SetupTitleButtons()
    {
        Debug.Log($"[TitleManager] 2단계: 판단된 결과({hasUserData})를 바탕으로 버튼 개별 제어 시작.");

        // 오직 데이터 유무 상태만 보고 두 버튼의 활성화 상태를 제어합니다.
        if (hasUserData)
        {
            if (continueButton != null) continueButton.SetActive(true);   // 이어하기 ON
            if (newGameButton != null) newGameButton.SetActive(false);    // 새로운 모험 OFF
        }
        else
        {
            if (continueButton != null) continueButton.SetActive(false);  // 이어하기 OFF
            if (newGameButton != null) newGameButton.SetActive(true);     // 새로운 모험 ON
        }
    }



    /// <summary>
    /// 1. [새로운 모험] 버튼을 눌렀을 때
    /// </summary>
    public void OnClickNewGame()
    {
        Debug.Log("[TitleManager] 새로운 모험 시작 - 타이틀을 끄고 첫 이벤트 화면을 켭니다.");

        // [1박자]: 먼저 잠들어 있던 '첫 이벤트 화면' 오브젝트의 네모 체크박스를 확실하게 켭니다!
        if (firstEventPanel != null)
        {
            firstEventPanel.SetActive(true);
        }

        // [2박자]: 그 다음 임무를 마친 타이틀 화면 전체를 깔끔하게 꺼줍니다.
        gameObject.SetActive(false);

        // [3박자]: 화면이 완전히 활성화된 것을 유니티가 인지한 '직후'에 
        // 이벤트 매니저에게 "이제 안전하니 0.5초 뒤에 첫 대사를 틀어라!"라고 바통을 넘깁니다.
        if (firstEventPanel != null && firstEventPanel.TryGetComponent<EventScreenManager>(out EventScreenManager eventMgr))
        {
            eventMgr.StartEventWithDelay();
        }
    }

    /// 2. [이어하기] 버튼을 눌렀을 때 실행되는 함수입니다.
    /// </summary>
    public void OnClickContinue()
    {
        Debug.Log("[Title] 이어하기 가동: 프로젝트 창고의 프리팹을 배제하고, 오직 현재 씬의 실물 이벤트창들을 완전히 파괴하여 끕니다.");

        CancelInvoke();

        // 1. 현재 게임 월드(씬)에 살아 숨 쉬는 모든 오브젝트들을 스캔합니다.
        Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();

        // 현재 열려있는 씬의 고유 이름을 가져옵니다.
        string currentActiveSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        foreach (Transform obj in allObjects)
        {
            // [검증 장치 핵심] 프로젝트 폴더 안에 잠들어 있는 원본 프리팹 파일들은 모조리 거릅니다!
            // 오직 현재 눈앞에 열려있는 진짜 게임 씬 소속의 실물 오브젝트만 통제합니다.
            if (obj.gameObject.scene.name == currentActiveSceneName)
            {
                // 실물 첫 이벤트 화면 발견 시 강제 종료
                if (obj.name == "첫 이벤트 화면")
                {
                    obj.gameObject.SetActive(false);
                }

                // 실물 두 번째 이벤트 화면 발견 시 강제 종료
                if (obj.name == "두번째 이벤트 화면")
                {
                    obj.gameObject.SetActive(false);
                }

                // [진짜 실물 마을 화면 발견] 오직 게임 화면 속에 배치된 진짜 마을만 활성화합니다!
                if (obj.name == "마을")
                {
                    obj.gameObject.SetActive(true);
                    Debug.Log("[Title] 프로젝트 폴더 원본이 아닌, 진짜 눈앞의 실물 '마을' 패널을 성공적으로 켰습니다.");
                }
            }
        }

        // 2. 만약의 사태를 대비해 인스펙터 칸에 직통 연결해 둔 마을 오브젝트도 세트로 한 번 더 확실하게 켭니다.
        if (villagePanel != null)
        {
            villagePanel.SetActive(true);
        }

        gameObject.SetActive(false);
    }




    /// <summary>
    /// 3. [설정] 버튼을 눌렀을 때 - 화면 전환 없이 팝업창을 켭니다.
    /// </summary>
    public void OnClickOpenSettings()
    {
        Debug.Log("[Title] 설정 창 버튼 클릭 - 설정 팝업창을 띄웁니다.");
        if (settingsPopupPanel != null)
        {
            settingsPopupPanel.SetActive(true); // 숨겨뒀던 설정창 ON!
        }
    }

    /// <summary>
    /// 3-1. 설정 창 내부에 만들 [닫기(X)] 버튼에 연결할 함수입니다.
    /// </summary>
    public void OnClickCloseSettings()
    {
        Debug.Log("[Title] 설정 창 닫기 버튼 클릭 - 설정 팝업창을 끕니다.");
        if (settingsPopupPanel != null)
        {
            settingsPopupPanel.SetActive(false); // 설정창 OFF!
        }
    }

    /// <summary>
    /// 4. [휴식하기] 버튼을 눌렀을 때 (iOS 및 타 OS 분기)
    /// </summary>
    // [유니티 매뉴얼] 이 수정본 조각은 코루틴을 포함하여 주석 포함 총 55줄입니다.

    /// <summary>
    /// 4. [휴식하기] 버튼을 눌렀을 때 실행되는 함수 (OS별 처리 + 아이폰 페이드아웃 연출)
    /// </summary>
    public void OnClickQuitGame()
    {
        Debug.Log("[Title] '휴식하기' 선택 - 운영체제(OS) 환경을 검사합니다.");

        // 현재 게임이 실행 중인 플랫폼이 아이폰/아이패드(iOS)인지 판별합니다.
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("[Title] iOS 환경 감지: 안내 문구 출력 후 페이드아웃 효과를 시작합니다.");

            // 이미 글자가 사라지는 중일 수도 있으므로, 혹시 돌고 있을지 모르는 연출을 초기화합니다.
            StopAllCoroutines();

            // 글자가 서서히 사라지는 마법(코루틴)을 실행합니다.
            StartCoroutine(FadeOutIosNotice());
        }
        // [유니티 매뉴얼] 기존 주석과 로그를 완벽히 살리면서 수정하는 부분입니다.
        else
        {
            Debug.Log("[Title] PC / 안드로이드 환경 감지: 게임을 완전히 종료합니다.");
            // 실제로 빌드된 게임(PC, 모바일 앱)을 종료하는 유니티 명령어입니다.
            Application.Quit();

            // ---------------- [여기서부터 새로 추가하는 한 줄!] ----------------
            // 유니티 에디터로 테스트 중일 때는 상단의 재생(▶) 버튼을 자동으로 꺼줍니다.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            // ---------------- [여기까지 새로 추가하는 한 줄!] ----------------
        }
    }


    /// <summary>
    /// 아이폰용 안내 텍스트를 띄우고 서서히 투명하게 만들어 사라지게 하는 컴퓨터 전용 타이머 기능입니다.
    /// </summary>
    private System.Collections.IEnumerator FadeOutIosNotice()
    {
        if (iosNoticeText == null) yield break;

        // 1. 먼저 글자를 화면에 확실하게 켜고 원래 선명한 흰색(투명도 1)으로 채워줍니다.
        iosNoticeText.gameObject.SetActive(true);
        Color txtColor = iosNoticeText.color;
        txtColor.a = 1f;
        iosNoticeText.color = txtColor;
        iosNoticeText.text = "홈 버튼을 눌러 주십시오.";

        // 2. 글자가 선명하게 뜬 상태로 잠시 유저가 읽을 시간(예: 1.5초) 동안 가만히 기다립니다.
        yield return new WaitForSeconds(1.5f);

        // 3. 이제 1초 동안 실시간으로 투명도를 낮춰서 흐릿하게 만듭니다.
        float duration = 1.0f; // 사라지는 데 걸릴 시간
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            // 시간에 따라 투명도(Alpha)를 1에서 0으로 수학적으로 부드럽게 깎아내립니다.
            txtColor.a = Mathf.Lerp(1f, 0f, currentTime / duration);
            iosNoticeText.color = txtColor;

            // 다음 프레임(화면 갱신)까지 잠시 대기했다가 다시 연산합니다.
            yield return null;
        }

        // 4. 완전히 투명해졌으므로 오브젝트를 깔끔하게 꺼서 마무리합니다.
        iosNoticeText.gameObject.SetActive(false);
        Debug.Log("[Title] iOS 안내 문구 페이드아웃 연출이 안전하게 끝났습니다.");
    }
    // [유니티 매뉴얼] 이 코드 조각은 주석 포함 총 25줄입니다. 스크립트 맨 아래에 붙여넣으세요.

    /// <summary>
    /// 3-1. [타이틀 화면으로] (취소) 버튼을 눌렀을 때 실행될 함수
    /// 어떤 변동사항이 있어도 무시하고 저장하지 않은 채 팝업창을 끕니다.
    /// </summary>
    public void OnClickCancelSettings()
    {
        Debug.Log("[TitleManager] 취소 버튼 클릭 - 변경 사항을 저장하지 않고 팝업을 닫습니다.");

        if (settingsPopupPanel != null)
        {
            settingsPopupPanel.SetActive(false); // 팝업창 OFF
        }
    }

    /// <summary>
    /// 3-2. [저장] 버튼을 눌렀을 때 실행될 함수
    /// 변경값이 있다면 기기에 안전하게 기록(저장)하고 팝업창을 닫습니다.
    /// </summary>
    public void OnClickSaveSettings()
    {
        Debug.Log("[TitleManager] 저장 버튼 클릭 - 변경된 설정값을 기기에 영구 저장합니다.");

        // [선배의 팁] 나중에 슬라이더나 볼륨 조절을 넣으면 여기에 저장 명령을 적을 예정입니다.
        PlayerPrefs.Save(); // 디스크에 즉시 쓰기 완료 명령

        if (settingsPopupPanel != null)
        {
            settingsPopupPanel.SetActive(false); // 팝업창 OFF
        }
    }
    // [유니티 매뉴얼] 이 코드 조각은 주석 포함 총 50줄입니다. 스크립트 맨 아래에 붙여넣으세요.

    [Header("타이틀 글자 색상 변경 설정")]
    [Tooltip("색상을 변경할 COLOR MIXER 타이틀 텍스트 오브젝트를 넣어주세요.")]
    public TextMeshProUGUI titleMainText;

    [Tooltip("글자가 다음 색상으로 변하는 데 걸리는 시간(초)입니다.")]
    public float colorChangeSpeed = 2.0f;

    // 기획하신 6가지 색상 명단 (적, 황, 녹, 청, 자, 흑)
    private Color[] targetColors = new Color[]
    {
        Color.red,                                      // 적
        new Color(1f, 0.92f, 0.016f),                   // 황 (유니티 기본 노란색)
        Color.green,                                    // 녹
        Color.blue,                                     // 청
        new Color(0.5f, 0f, 0.5f),                      // 자 (보라색)
        Color.white                                     // 백
    };

    private int currentColorIndex = 0;                  // 현재 색상 번호
    private int nextColorIndex = 1;                     // 다음에 변할 색상 번호
    private float colorTransitionProgress = 0f;         // 색상 변환 진행도 (0.0 ~ 1.0)

    // 타이틀 화면이 켜져 있는 동안 매 프레임 실시간으로 실행되는 유니티 기본 함수입니다.
    private void Update()
    {
        // 연결된 글자가 없다면 계산하지 않고 넘어갑니다.
        if (titleMainText == null) return;

        // 시간에 따라 진행도를 누적합니다.
        colorTransitionProgress += Time.deltaTime / colorChangeSpeed;

        // 현재 색상과 다음 색상을 비율(progress)에 맞춰 스르륵 섞어줍니다.
        titleMainText.color = Color.Lerp(targetColors[currentColorIndex], targetColors[nextColorIndex], colorTransitionProgress);

        // 다음 색상에 완전히 도달했다면(1.0 이상) 목표 색상을 다음 단계로 업데이트합니다.
        if (colorTransitionProgress >= 1f)
        {
            colorTransitionProgress = 0f;               // 진행도 리셋
            currentColorIndex = nextColorIndex;          // 현재 색상을 방금 도달한 색상으로 바꿈
            nextColorIndex = (nextColorIndex + 1) % targetColors.Length; // 다음 색상 번호 결정 (마지막 흑색 다음엔 다시 0번 적색으로!)
        }
    }


}


