using UnityEngine;
using System.Collections;

public class Match3GameManager : MonoBehaviour
{
    [Header("--- [핵심] UI 컴포넌트 연결 ---")]
    public TMPro.TMP_Text turnText;
    public TMPro.TMP_Text comboText;

    [Header("전투 동기화 데이터")]
    public string battleMonsterName = ""; // 모험 씬에서 넘겨받은 진짜 몬스터 이름이 저장될 서랍


    [Header("--- [핵심] 몬스터 및 HP바 프리팹 세팅 ---")]
    public GameObject hpSliderPrefab;       // 프로젝트 창의 HP 바 프리팹 (MonsterHPSlider)
    public Transform uiCanvasTransform;     // UI가 스폰될 Canvas 연결
    public GameObject[] monsterPrefabs;     // 3마리의 몬스터 프리팹 배열 (크기 3)
    public Transform monsterSpawnPoint;     // 몬스터가 소환될 월드 공간 (MonsterSpawnPoint)

    // 내부에서 실시간으로 생성하여 제어할 숨은 부품들 (인스펙터에서 숨김)
    private UnityEngine.UI.Slider monsterHPSlider;
    private UnityEngine.UI.Image hpBarFillImage;

    private Match3Board board;
    private Match3Referee referee;

    // 게임 핵심 상태 제어 변수
    private int currentTurn = 20;
    private int currentCombo = 0;
    private float monsterHP = 500f;
    private float maxMonsterHP = 500f;
    private bool isProcessing = false;

    public bool GetIsProcessing() { return isProcessing; }

    void Awake()
    {
        board = GetComponent<Match3Board>();
        referee = GetComponent<Match3Referee>();
    }

    void Start()
    {
        // 🚀 [행동대장 연결 통로 개통!] 내 오브젝트에 같이 붙어 있는 Match3Board 컴포넌트를 자동으로 꽉 잡습니다.
        board = GetComponent<Match3Board>();
        // 💡 [핵심 추가] 모험 씬에서 파괴되지 않고 살아남은 StageManager 서랍을 맵에서 수색합니다.
        AdventureStageManager stageManager = FindAnyObjectByType<AdventureStageManager>();

        if (stageManager != null)
        {
            // 모험 매니저가 가지고 있던 진짜 몬스터 이름(블랙, 그레이, 화이트 등)을 복사해 옵니다.
            battleMonsterName = stageManager.currentTargetMonster;
            Debug.Log($"[3매치 전투 구동] '{battleMonsterName}' 전투 데이터를 성공적으로 불러왔습니다.");
        }
        else
        {
            // 유니티 에디터에서 '3매치' 씬만 단독으로 플레이 버튼을 눌러 테스트할 때를 위한 안전장치입니다.
            battleMonsterName = "테스트용 기본 몬스터";
            Debug.LogWarning("AdventureStageManager를 찾을 수 없어 기본 몬스터 이름으로 대체합니다.");
        }
        // 게임 시작 시 몬스터를 먼저 안전하게 스폰하고 체력 시스템을 엮어줍니다.
        SpawnMonsterAndSetupHPBar();
        InitMatch3Battle();
    }

    // 1) 3매치 게임을 시작한다
    void InitMatch3Battle()
    {
        monsterHP = maxMonsterHP;
        currentTurn = 20;
        currentCombo = 0;
        isProcessing = false;

        UpdateGameUI();
        board.InitializeBoard();
        // 화면 하이어라키에 실시간 스폰된 64개 블록들을 지휘관 장부(BoardArray)에 순서대로 강제 등록합니다.
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                // 하이어라키에서 이름표("Block_(X,Y)")로 블록을 수색하여 찾아냅니다.
                GameObject foundBlock = GameObject.Find($"Block_({x},{y})");
                if (foundBlock != null)
                {
                    int targetIndex = y * board.width + x; // 2차원 좌표를 1차원 인덱스로 변환하는 공식
                    board.BoardArray[targetIndex] = foundBlock;
                }
            }
        }
        Debug.Log("🎲 [대성공] 64개 실물 블록 주소를 지휘관 장부(BoardArray)에 100% 동기화 완료했습니다!");



        if (CheckBoardDeadlock())
        {
            TriggerDeadlockRefresh();
        }
    }

    // 인스펙터의 프리팹을 활용해 실시간으로 전장을 조립하는 핵심 연동 기믹
    void SpawnMonsterAndSetupHPBar()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0) return;

        // 등록된 몬스터 중 무작위로 한 마리를 선택합니다.
        int selectIndex = Random.Range(0, monsterPrefabs.Length);
        GameObject selectedMonsterPrefab = monsterPrefabs[selectIndex];

        if (selectedMonsterPrefab != null && monsterSpawnPoint != null)
        {
            // 1. 몬스터 프리팹을 스폰 포인트 좌표에 생성합니다.
            Instantiate(selectedMonsterPrefab, monsterSpawnPoint.position, Quaternion.identity);

            // 2. HP 바 프리팹을 Canvas의 자식으로 실시간 복제합니다.
            if (hpSliderPrefab != null && uiCanvasTransform != null)
            {
                GameObject spawnedSliderObj = Instantiate(hpSliderPrefab, uiCanvasTransform);

                // 3. 동적으로 태어난 슬라이더와 컬러 Fill 이미지를 코드가 스스로 찾아 매핑합니다.
                monsterHPSlider = spawnedSliderObj.GetComponent<UnityEngine.UI.Slider>();

                Transform fillTransform = spawnedSliderObj.transform.Find("Fill Area/Fill");
                if (fillTransform != null)
                {
                    hpBarFillImage = fillTransform.GetComponent<UnityEngine.UI.Image>();
                }

                // 4. 생성된 HP바 UI의 기본 화면 위치를 정렬합니다.
                UnityEngine.RectTransform rect = spawnedSliderObj.GetComponent<UnityEngine.RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(0, 400);
                }
            }
        }
    }

    public IEnumerator RequestBlockSwapProcess(Vector2Int p1, Vector2Int p2)
    {
        isProcessing = true;
        currentCombo = 0;

        board.SwapGridData(p1, p2);
        GameObject[] boardArray = board.BoardArray;
        int w = board.Width;
        int idx1 = p1.y * w + p1.x;
        int idx2 = p2.y * w + p2.x;

        yield return StartCoroutine(board.MoveBlockAnimation(boardArray[idx1], p1));
        yield return StartCoroutine(board.MoveBlockAnimation(boardArray[idx2], p2));

        bool[,] matchMap = referee.EvaluateBoardMatches();

        if (referee.HasAnyMatch(matchMap))
        {
            currentTurn--;
            currentCombo++;
            UpdateGameUI();

            while (referee.HasAnyMatch(matchMap))
            {
                float calculatedDamage = referee.CalculateTotalDamage(matchMap, currentCombo);
                monsterHP -= calculatedDamage;
                if (monsterHP < 0) monsterHP = 0;
                UpdateGameUI();

                yield return StartCoroutine(referee.ClearMatchedBlocks(matchMap));
                yield return StartCoroutine(referee.GravityDropBlocks());
                yield return StartCoroutine(referee.RefreshNewTopBlocks());

                matchMap = referee.EvaluateBoardMatches();
                if (referee.HasAnyMatch(matchMap))
                {
                    currentCombo++;
                    UpdateGameUI();
                }
            }

            CheckMonsterCounterAttack();
        }
        else
        {
            board.SwapGridData(p1, p2);
            yield return StartCoroutine(board.MoveBlockAnimation(boardArray[idx1], p1));
            yield return StartCoroutine(board.MoveBlockAnimation(boardArray[idx2], p2));

            currentTurn--;
            currentCombo = 0;
            UpdateGameUI();
        }

        if (CheckBoardDeadlock())
        {
            TriggerDeadlockRefresh();
        }

        EvaluateGameOver();
        isProcessing = false;
    }

    bool CheckBoardDeadlock()
    {
        int w = board.Width;
        int h = board.Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < w - 1)
                {
                    board.SwapGridData(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                    bool possibleMatch = referee.HasAnyMatch(referee.EvaluateBoardMatches());
                    board.SwapGridData(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                    if (possibleMatch) return false;
                }
                if (y < h - 1)
                {
                    board.SwapGridData(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                    bool possibleMatch = referee.HasAnyMatch(referee.EvaluateBoardMatches());
                    board.SwapGridData(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                    if (possibleMatch) return false;
                }
            }
        }
        return true;
    }

    void TriggerDeadlockRefresh()
    {
        board.InitializeBoard();
    }

    void CheckMonsterCounterAttack()
    {
        if (currentTurn % 5 == 0 && monsterHP > 0)
        {
            Debug.Log("Monster Counter Attack!");
        }
    }

    void EvaluateGameOver()
    {
        if (monsterHP <= 0)
        {
            Debug.Log("[VICTORY] Monster Defeated!");
        }
        else if (currentTurn <= 0)
        {
            Debug.Log("[DEFEAT] Turn Over.");
        }
    }

    void UpdateGameUI()
    {
        if (turnText != null) turnText.text = "남은 턴: " + currentTurn;
        if (comboText != null) comboText.text = currentCombo + " COMBO";

        // 코드가 실시간 생성한 슬라이더와 컬러 이미지를 쳐다보며 안전하게 UI 업데이트 진행
        if (monsterHPSlider != null)
        {
            float hpRatio = monsterHP / maxMonsterHP;
            monsterHPSlider.value = hpRatio;

            if (hpBarFillImage != null)
            {
                hpBarFillImage.color = Color.Lerp(Color.red, Color.green, hpRatio);
            }
        }
    }
    // 🚀 [우리 프로젝트 전용 3매치 가동 엔진] Match3Board가 던진 드래그 신호를 직통 수신합니다!
    public void SwapBlocks(UnityEngine.Vector4 swipeData)
    {
        int sX = (int)swipeData.x; int sY = (int)swipeData.y;
        int tX = (int)swipeData.z; int tY = (int)swipeData.w;

        if (isProcessing) return; // 중복 조작 잠금장치
        StartCoroutine(SwapAndProcessRoutine(sX, sY, tX, tY));
    }

    private System.Collections.IEnumerator SwapAndProcessRoutine(int sX, int sY, int tX, int tY)
    {
        isProcessing = true;

        // 📐 유저님의 8x8 2차원 배열 격자 구조와 100% 동기화하는 인덱스 연산
        int srcIndex = sY * board.width + sX;
        int dstIndex = tY * board.width + tX;

        UnityEngine.GameObject srcBlock = board.BoardArray[srcIndex];
        UnityEngine.GameObject dstBlock = board.BoardArray[dstIndex];

        if (srcBlock == null || dstBlock == null) { isProcessing = false; yield break; }

        // 1. [기획서 반영]: 화면상의 실제 블록 위치를 슉 교체하는 눈속임 연출
        UnityEngine.Vector3 srcPos = srcBlock.transform.localPosition;
        UnityEngine.Vector3 dstPos = dstBlock.transform.localPosition;

        srcBlock.transform.localPosition = dstPos;
        dstBlock.transform.localPosition = srcPos;

        // 실물 위치 바꿨으니 컴퓨터 내부 데이터 장부도 동기화 스왑
        board.BoardArray[srcIndex] = dstBlock;
        board.BoardArray[dstIndex] = srcBlock;

        yield return new UnityEngine.WaitForSeconds(0.2f); // 부드러운 스왑 이동 대기 시간

        // 2. [3매치 판정 단계]: 우리가 개조한 완성형 Referee 심판기를 호출합니다!
        Match3Referee referee = FindAnyObjectByType<Match3Referee>();
        bool hasMatches = false;
        bool[,] myMatchMap = null;

        if (referee != null)
        {
            myMatchMap = referee.EvaluateBoardMatches(); // 8x8 격자 지도 수색 수신
            if (myMatchMap != null)
            {
                for (int x = 0; x < board.width; x++)
                {
                    for (int y = 0; y < board.height; y++)
                    {
                        if (myMatchMap[x, y]) { hasMatches = true; break; }
                    }
                    if (hasMatches) break;
                }
            }
        }

        if (hasMatches && myMatchMap != null)
        {
            // 🎉 [3매치 성공 조건]: 블록 팡팡 터뜨리고 몬스터 피 깎기 가동!
            UnityEngine.Debug.Log("[판정 완료] 3매치 대성공! 폭발 정산 처리를 시작합니다.");

            float totalDmg = referee.CalculateTotalDamage(myMatchMap, currentCombo);
            monsterHP -= totalDmg; // 인스펙터 속성 대미지 연동 완료
            UpdateGameUI();

            yield return StartCoroutine(referee.ClearMatchedBlocks(myMatchMap)); // 폭발 연출 대기

            // 💡 [추후 확장 영역]: 여기에 빈칸 채우기(리필) 코드를 얹어주시면 됩니다!
            currentCombo++;
        }
        else
        {
            // ❌ [3매치 실패 조건]: "3매치 실패면 이동중인 블록은 제자리로 돌아가야해" 규정 강제 집행!
            UnityEngine.Debug.Log("[판정 완료] 3매치 조건 불만족. 원래 터전으로 복귀 연출 가동.");

            srcBlock.transform.localPosition = srcPos;
            dstBlock.transform.localPosition = dstPos;

            board.BoardArray[srcIndex] = srcBlock;
            board.BoardArray[dstIndex] = dstBlock;
            yield return new UnityEngine.WaitForSeconds(0.2f);
        }

        // 3. [데드락 판정]: 한 턴이 끝났을 때 판이 막혔는지 감시
        if (CheckBoardDeadlock())
        {
            TriggerDeadlockRefresh(); // 데드락 새로고침 실시간 구동!
        }

        currentTurn--;
        UpdateGameUI();
        isProcessing = false; // 조작 잠금 해제
    }

}
