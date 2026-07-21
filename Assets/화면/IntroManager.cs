using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroLoadingManager : MonoBehaviour
{
    // [유니티 매뉴얼] 이 코드는 주석 포함 총 95줄입니다.
    public Slider loadingSlider;
    public TextMeshProUGUI statusText;
    public GameObject clickPanel;

    [Header("인트로 터치 제어 설정 (직접 연결 방식)")]
    [Tooltip("인스펙터 체크박스를 제어할 인트로 클릭 감지 버튼 컴포넌트를 직접 넣어주세요.")]
    public Button introClickButton;


    [Header("로딩 속도 설정")]
    public float loadingSpeed = 0.5f;

    private float currentProgress = 0f;
    private bool isLoadingComplete = false;
    private bool hasUserData = false;

    // 어떤 문구 세트를 사용할지 결정하는 변수 (0, 1, 2 중 하나)
    private int textGroupIndex = 0;

    // ⬇️ [27번째 줄 void Start() 구역을 아래 코드로 통째로 복사 붙여넣기 하세요!] ⬇️
    void Start()
    {
        // 👑 [창고 복귀 유저 인트로 가동 절대 차단막 엔진 탑재]
        // 캐릭터 저장고 방에서 뒤로가기를 누르고 방금 막 돌아온 복귀 유저인지 검사합니다.
        if (PlayerPrefs.HasKey("IsReturningFromStorage") && PlayerPrefs.GetInt("IsReturningFromStorage") == 1)
        {
            Debug.Log("[IntroManager] 창고 복귀 유저 상태가 확인되어 인트로 로딩 가동을 원천 차단하고 셀프 종료합니다.");

            // 인트로 패널 본체 오브젝트를 눈치 챙겨서 즉시 강제로 꺼버리고 연산을 탈출합니다!
            gameObject.SetActive(false);
            return; // ★ 복귀 처리가 끝났으므로 아래쪽에 적힌 원래 첫 인트로 실행 코드들은 완전히 무시하고 패스합니다!
        }

        // ----------------------------------------------------
        // 🌀 [기존에 적혀있던 인트로 시작 순정 로직 구역 - 오차 없이 안전 보존]
        // ----------------------------------------------------
        // [서열 정리] 인트로 캔버스의 그리기 순서를 '100층'으로 높여 무조건 화면 맨 앞에 오게 만듭니다! 구조송
        if (TryGetComponent<Canvas>(out Canvas introCanvas)) { introCanvas.overrideSorting = true; introCanvas.sortingOrder = 100; }

        // [강제 활성화 스위치] 게임이 시작되면 이 스크립트가 붙은 오브젝트를 무조건 활성화합니다!
        gameObject.SetActive(true);
        loadingSlider.value = 0f;

        // [당신의 설계] 연결된 버튼의 Interactable 네모 박스 체크를 시작하자마자 완전히 해제(OFF)합니다!
        if (introClickButton != null) introClickButton.interactable = false;

        // [새로운 기능] 게임이 켜질 때 0, 1, 2 중 하나의 숫자를 무작위로 뽑아줍니다.
        textGroupIndex = Random.Range(0, 3);
        Debug.Log($"[IntroLoading] 이번 판에 선택된 로딩 문구 세트 번호: {textGroupIndex}번");

        CheckUserDataOnly();
        if (hasUserData) { /* 나중에 저장된 유저 정보를 불러올 때 활용할 예정입니다 */ }
    } // void Start() 함수가 끝나는 닫는 중괄호

    // [에러 박멸 핵심] 컴퓨터가 애타게 찾고 있던 바로 그 함수 방을 안전하게 개설했습니다!
    private void CheckUserDataOnly()
    {
        // 미래의 유저 데이터 로딩 기획을 위해 미리 비워둔 빈 방입니다.
    }

    // [유니티 매뉴얼] 이 수정본 조각은 총 31줄입니다.
    void Update()
    {
        // 1. 로딩이 아직 안 끝났다면 계속 로딩바를 채웁니다.
        if (!isLoadingComplete)
        {
            currentProgress += Time.deltaTime * loadingSpeed;
            loadingSlider.value = Mathf.Clamp01(currentProgress);

            // ---------------- [여기서부터 새로 끼워 넣는 코드] ----------------
            // 0%일 땐 흰색, 100%일 땐 녹색이 되도록 비율을 계산합니다.
            Color barColor = Color.Lerp(Color.white, Color.green, loadingSlider.value);

            // 로딩바 내부의 채워지는 이미지(fillRect)를 찾아서 색상을 입혀줍니다.
            if (loadingSlider.fillRect != null && loadingSlider.fillRect.TryGetComponent<Image>(out Image fillImage))
            {
                fillImage.color = barColor;
            }
            // ---------------- [여기까지 새로 끼워 넣는 코드] ----------------

            // 로딩 수치에 따라 무작위로 선택된 세트의 텍스트를 보여줍니다.
            UpdateStatusText(loadingSlider.value);

            if (currentProgress >= 1.0f)
            {
                CompleteLoading();
            }
        }
        // 2. [새로 추가된 기능] 로딩이 끝났다면 글자를 깜빡깜빡하게 만듭니다!
        else
        {
            if (statusText != null)
            {
                // 시간에 따라 0.3에서 1.0 사이를 부드럽게 오가는 수학 공식입니다.
                // 뒤의 '5f' 숫자를 높이면 더 빠르게 깜빡거립니다.
                float alpha = Mathf.PingPong(Time.time * 2f, 0.7f) + 0.3f;

                // 텍스트의 색상에서 투명도(a)만 실시간으로 조절합니다.
                Color textColor = statusText.color;
                textColor.a = alpha;
                statusText.color = textColor;
            }
        }
    }


    // [핵심 변경 구간] 3가지 루트 중 하나를 선택해 텍스트를 출력합니다.
    private void UpdateStatusText(float progress)
    {
        // 1번 루트: 커피 세트
        if (textGroupIndex == 0)
        {
            if (progress < 0.33f) statusText.text = "당신을 위해 물 끓이는 중...";
            else if (progress < 0.66f) statusText.text = "컵에 물을 붓는 중...";
            else if (progress < 1.0f) statusText.text = "커피를 타는 중...";
        }
        // 2번 루트: 데이터 및 시스템 세트 (요청하신 문구)
        else if (textGroupIndex == 1)
        {
            if (progress < 0.25f) statusText.text = "데이터를 불러오는 중...";
            else if (progress < 0.50f) statusText.text = "리소스 검사 중...";
            else if (progress < 0.75f) statusText.text = "세계관 생성 중...";
            else if (progress < 1.0f) statusText.text = "모험을 준비하는 중...";
        }
        // 3번 루트: 선배 추천 판타지 세트
        else if (textGroupIndex == 2)
        {
            if (progress < 0.33f) statusText.text = "동굴 속 괴물에게 밥 주는 중...";
            else if (progress < 0.66f) statusText.text = "보물 상자에 독니 덫 설치하는 중...";
            else if (progress < 1.0f) statusText.text = "물감을 섞어 새로운 색을 만드는 중...";
        }
    }

    private void CompleteLoading()
    {
        isLoadingComplete = true;
        statusText.text = "화면을 클릭해주세요.";

        // [최종 마감] '로딩 완료!' 로그가 찍히기 직전에, 꺼두었던 버튼 컴포넌트의 체크박스를 다시 켭니다(ON)!
        if (clickPanel != null && clickPanel.TryGetComponent<Button>(out Button introBtn))
        {
            introBtn.interactable = true;
        }

        Debug.Log("[IntroLoading] 로딩 완료!");
        // 2. [당신의 설계] 메시지가 나오자마자 독립된 방에 연결된 버튼의 체크박스를 다시 탁! 체크(ON)합니다.
        if (introClickButton != null)
        {
            introClickButton.interactable = true;
        }
    }



    private void CheckUserData()
    {
        Debug.Log("[IntroLoading] 데이터 검사 중...");
        if (PlayerPrefs.HasKey("HasSaveData"))
        {
            hasUserData = true;
            Debug.Log("[IntroLoading] 💾 기존 유저 데이터 발견!");
        }
        else
        {
            hasUserData = false;
            Debug.Log("[IntroLoading] 🆕 새로운 유저로 판단!");
        }
    }
    // [유니티 매뉴얼] 이 코드 조각은 주석 포함 총 20줄입니다.

    [Header("타이틀 전환 설정")]
    [Tooltip("인트로가 끝나고 켜질 타이틀 화면 오브젝트를 넣어주세요.")]
    public GameObject titleScreenObject;

    /// 로딩이 완수된 후 화면 전체(ClickPanel)를 마우스로 터치했을 때 발동하는 함수
    /// </summary>
    public void OnClickToTitle()
    {
        // [방어벽 발동] 유저가 마을에서 캐릭터 창고 씬을 갔다가 뒤로가기로 정식 복귀한 순간이라면!
        if (PlayerPrefs.HasKey("IsReturningFromStorage") && PlayerPrefs.GetInt("IsReturningFromStorage") == 0)
        {
            // 타이틀 매니저가 이미 2단계에서 모든 정리를 끝내고 세이브 값을 0으로 안전 리셋해둔 상태입니다.
            Debug.Log("[IntroManager] 복귀 정착 성공 감지 -> 화면 터치 시 타이틀이 강제로 무덤에서 부활하는 오작동을 철저히 봉쇄합니다.");

            // 타이틀을 절대 깨우지 않고, 화면을 가리고 있던 인트로 로딩 패널 자기 자신만 투명하게 싹 꺼줍니다!
            gameObject.SetActive(false);
            return; // 연산 즉시 탈출
        }

        // ----------------------------------------------------
        // 🌀 [기존 일반 유저 시작 로직 구역 - 안전 보존]
        // ----------------------------------------------------
        Debug.Log("[IntroManager] 일반 새 게임 유저 터치 감지 -> 타이틀 본체를 활성화합니다.");
        if (titleScreenObject != null)
        {
            titleScreenObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }





}
