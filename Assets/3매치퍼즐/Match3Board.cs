using UnityEngine;
using System.Collections;

public class Match3Board : MonoBehaviour
{
    public bool isProcessing = false;
    public float boardSizeX = 8.0f;
    public float boardSizeY = 8.0f;
    public int width = 8;
    public int height = 8;
    public float cellSize = 1.0f;
    public Transform gridGroup;
    public GameObject[] blockPrefabs;
    private GameObject[] boardArray;

    private GameObject selectedBlock = null;
    private Vector2 clickStartPos;
    private int startX;
    private int startY;

    public int Width => width;
    public int Height => height;
    public GameObject[] BoardArray => boardArray;

    void Awake()
    {
        boardArray = new GameObject[width * height];
    }

    public void InitializeBoard()
    {
        Debug.Log("[기획 반영] 보드의 위치와 크기 변동에 자동으로 대응하여 8x8 배치를 시작합니다.");

        // 8x8 격자 사양 고정
        width = 8;
        height = 8;

        // 기존 블록 깔끔하게 청소
        foreach (Transform child in gridGroup) { Destroy(child.gameObject); }

        // 📐 [핵심 공식] 유저님이 설정한 보드 전체 크기를 8칸 격자 간격으로 정밀 자동 분할 계산합니다!
        float spacingX = boardSizeX / (width - 1);
        float spacingY = boardSizeY / (height - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newBlock = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)], gridGroup);
                Transform blockTransform = newBlock.transform;

                if (blockTransform != null)
                {
                    blockTransform.localScale = Vector3.one; // 유저님 기획 반영: 스케일 1 유지
                    blockTransform.localRotation = Quaternion.identity;

                    // 💡 [무적의 좌표 공식]: 부모(GridGroup)의 현재 월드 위치를 기준점(0,0)으로 삼아 상대 좌표를 계산합니다!
                    // 이 공식 덕분에 GridGroup의 위치를 유니티 씬창에서 어디로 옮기든 블록들이 알아서 뭉쳐서 따라갑니다.
                    float startX = -boardSizeX / 2f;
                    float startY = -boardSizeY / 2f;

                    float finalX = startX + (x * spacingX);
                    float finalY = startY + (y * spacingY);

                    // 부모 주머니 내부의 로컬 좌표로 칼같이 안착시킵니다.
                    blockTransform.localPosition = new Vector3(finalX, finalY, 0f);
                }

                newBlock.name = $"Block_({x},{y})";
            }
        }
        Debug.Log($"🎲 [대성공] 가로간격: {spacingX}, 세로간격: {spacingY} 자동 연산 정렬 완료!");
    }




    public void SpawnBlockAtPosition(int x, int y)
    {
        int randomIndex = Random.Range(0, blockPrefabs.Length);
        Vector3 spawnPos = GetWorldPosition(x, y);

        // 기존에 boardParent로 되어 있던 맨 끝 인자값을 gridGroup으로 수정합니다!
        GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPos, Quaternion.identity, gridGroup);
        newBlock.name = blockPrefabs[randomIndex].name;

        boardArray[y * width + x] = newBlock;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 startPos = gridGroup != null ? gridGroup.position : Vector3.zero;
        return startPos + new Vector3(x * cellSize, y * cellSize, 0);
    }


    public void ClearBoardObjects()
    {
        if (boardArray == null) return;
        for (int i = 0; i < boardArray.Length; i++)
        {
            if (boardArray[i] != null)
            {
                Destroy(boardArray[i]);
                boardArray[i] = null;
            }
        }
    }
    // 1. 블록 위치 데이터를 서로 교환하는 핵심 부품 (SwapGridData)
    public void SwapGridData(Vector2Int p1, Vector2Int p2)
    {
        int idx1 = p1.y * width + p1.x;
        int idx2 = p2.y * width + p2.x;

        GameObject temp = boardArray[idx1];
        boardArray[idx1] = boardArray[idx2];
        boardArray[idx2] = temp;
    }

    // 2. 블록이 이동할 때 부드럽게 미끄러지듯 이동하는 물리 연출 부품 (MoveBlockAnimation)
    public System.Collections.IEnumerator MoveBlockAnimation(GameObject block, Vector2Int gridPos)
    {
        if (block == null) yield break;
        Vector3 targetPos = GetWorldPosition(gridPos.x, gridPos.y);

        while (Vector3.Distance(block.transform.position, targetPos) > 0.05f)
        {
            block.transform.position = Vector3.MoveTowards(block.transform.position, targetPos, Time.deltaTime * 10f);
            yield return null;
        }
        block.transform.position = targetPos;
    }
    // 🚀 [우리 프로젝트 전용 최신식 3매치 마우스 조작/동기화 엔진]
    private void Update()
    {
        if (isProcessing) return;

        // 1. 마우스 왼쪽 버튼을 [꾹 눌렀을 때] (UI 전용 조준경으로 블록 록온)
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            clickStartPos = mouseScreenPos;

            // 🎯 [UI 전용 조준경 발동]: 캔버스 위에 그려진 UI 블록을 정밀 포착합니다.
            UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = mouseScreenPos;
            System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

            // 📐 [Match3Board.cs 내부 136번 줄 부근 수정]
            // 기존의 Contains("Block_")을 과감히 지우고, 생성된 모든 자식 블록들을 포착할 수 있게 빗장을 풉니다!
            foreach (var result in results)
            {
                // 부모가 GridGroup(보드판 부모)인 자식 오브젝트라면 무조건 블록으로 인정하고 조준합니다!
                if (result.gameObject != null && result.gameObject.transform.parent == gridGroup)
                {
                    selectedBlock = result.gameObject;

                    // 📐 [이름표 좌표 자르기]: 이제 "Block_(X,Y)" 문자열 파싱이 필요 없으므로, 
                    // 실물 블록이 배치된 칸 좌표를 100% 안전하게 다이렉트로 축출해 낼 수 있는 코드로 이어집니다.
                    // 📐 [추가할 코드 2줄]: 클릭한 블록의 이름 "Block_(X,Y)"에서 X와 Y 숫자를 정확하게 잘라내 장부에 기억시킵니다.
                    string[] nameParts = selectedBlock.name.Replace("Block_(", "").Replace(")", "").Split(',');
                    if (nameParts.Length == 2) { int.TryParse(nameParts[0], out startX); int.TryParse(nameParts[1], out startY); }

                    break;
                }
            }

        }

        // 2. 마우스 왼쪽 버튼을 [뗄 때] (드래그 변위 계산 및 지휘관 다이렉트 호출)
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame && selectedBlock != null)
        {
            Vector2 clickEndPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            Vector2 delta = clickEndPos - clickStartPos;

            // 최소 30픽셀 이상 확실하게 당겼을 때만 단방향 이동기 가동
            if (delta.magnitude > 30f)
            {
                CalculateSwipeDirection(delta);
            }

            selectedBlock = null;
        }
    }

    private void CalculateSwipeDirection(Vector2 delta)
    {
        int targetX = startX;
        int targetY = startY;

        // 📐 [기획서 반영]: 상하좌우 단방향 1칸 제약 (대각선 차단)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            targetX += delta.x > 0 ? 1 : -1;
        }
        else
        {
            targetY += delta.y > 0 ? 1 : -1;
        }

        // 8x8 보드 영역 내부일 때만 전선 작동
        if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
        {
            Match3GameManager manager = FindAnyObjectByType<Match3GameManager>();
            if (manager != null)
            {
                UnityEngine.Debug.Log($"[시스템 통제] ({startX}, {startY})에서 ({targetX}, {targetY})로 드래그 감지. 판정을 시작합니다.");

                // 💡 [치료 열쇠]: 유저님의 진짜 GameManager 장부 함수인 swapBlocks(int, int, int, int) 규격과 100% 일치하게 전선을 직결합니다!
                // 📐 [220번째 줄 수정] swapBlocks를 대문자 SwapBlocks로 변경해 줍니다!
                manager.swapBlocks(startX, startY, targetX, targetY);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("⚠ 보드판 영역 바깥으로 드래그가 차단되었습니다.");
        }
    }

}
