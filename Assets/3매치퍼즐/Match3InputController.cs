using UnityEngine;

public class Match3InputController : MonoBehaviour
{
    private Match3Board board;
    private Match3GameManager gameManager;

    private GameObject selectedBlock;
    private Vector2Int selectedGridPos;

    void Awake()
    {
        board = GetComponent<Match3Board>();
        gameManager = GetComponent<Match3GameManager>();
    }

    void Update()
    {
        // 게임 매니저가 보드를 정산 중이거나 블록이 떨어지는 중에는 유저 입력 감시 차단
        if (gameManager != null && gameManager.GetIsProcessing()) return;

        // [흐름] 블록은 클릭 후 인접한 블록 1칸 범위 안쪽으로 드래그하여 이동시킨다
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        else if (Input.GetMouseButtonUp(0) && selectedBlock != null)
        {
            HandleMouseRelease();
        }
    }

    void HandleMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            int w = board.Width;
            int h = board.Height;
            GameObject[] boardArray = board.BoardArray;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int index = y * w + x;
                    if (boardArray[index] == hit.collider.gameObject)
                    {
                        selectedBlock = boardArray[index];
                        selectedGridPos = new Vector2Int(x, y);
                        return;
                    }
                }
            }
        }
    }

    void HandleMouseRelease()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject != selectedBlock)
        {
            Vector2Int targetGridPos = Vector2Int.zero;
            bool isTargetFound = false;

            int w = board.Width;
            int h = board.Height;
            GameObject[] boardArray = board.BoardArray;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int index = y * w + x;
                    if (boardArray[index] == hit.collider.gameObject)
                    {
                        targetGridPos = new Vector2Int(x, y);
                        isTargetFound = true;
                        break;
                    }
                }
            }

            // 상하좌우 인접 블록 1칸 범위 수학적 거리 검사 (거리가 딱 1인 경우만 교환 조건 성립)
            if (isTargetFound && Mathf.Approximately(Vector2Int.Distance(selectedGridPos, targetGridPos), 1.0f))
            {
                // 게임 매니저에게 정렬 및 스와이프 연산 요청 전달
                StartCoroutine(gameManager.RequestBlockSwapProcess(selectedGridPos, targetGridPos));
            }
        }
        
        // 조작 데이터 초기화
        selectedBlock = null;
    }
}
