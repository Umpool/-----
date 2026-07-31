using UnityEngine;
using System.Collections;

public class Match3GameManager : MonoBehaviour
{
    [Header("--- [핵심] UI 컴포넌트 연결 ---")]
    public TMPro.TMP_Text turnText;
    public TMPro.TMP_Text comboText;

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
}
