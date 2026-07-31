using UnityEngine;
using System.Collections;

public class Match3Board : MonoBehaviour
{
    [Header("ㅡ 마우스 및 드래그 제어 변수 ㅡ")]
    private GameObject selectedBlock = null; // 현재 마우스로 꾹 누른 블록
    private Vector2 clickStartPos;            // 처음 마우스를 클릭한 화면 좌표
    private int startX, startY;               // 클릭한 블록의 바둑판 격자 좌표 (X, Y)

    [Header("ㅡ 조작 잠금 안전 스위치 ㅡ")]
    // 블록이 터지거나 리필되는 도중에는 마우스 조작을 일시적으로 차단하는 방어막입니다.
    public bool isProcessing = false;

    [Header("보드판 실물 영역 크기 설정")]
    public float boardSizeX = 8.0f; // 에디터 인스펙터에서 이 숫자를 키우면 퍼즐판이 가로로 넓어집니다!
    public float boardSizeY = 8.0f; // 이 숫자를 키우면 퍼즐판이 세로로 길어집니다!


    [Header("--- Board Settings ---")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1.0f;
    public Transform gridGroup;

    [Header("--- Block Prefabs (6 Colors) ---")]
    public GameObject[] blockPrefabs;

    private GameObject[] boardArray;

    public int Width => width;
    public int Height => height;
    public GameObject[] BoardArray => boardArray;



    void Awake()
    {
        boardArray = new GameObject[width * height];
    }
    // 🎲 [최종 기획 이식] 위치 이동 및 크기 변화에 실시간 100% 자동 대응하는 8x8 배치 엔진
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
    private void Update()
    {
        // // 🛡 [방어막]: 블록 연산 중이거나 게임판 준비 안 되었을 때는 마우스 입력을 원천 차단합니다.
        if (isProcessing) return;

        // 1. 마우스 왼쪽 버튼을 [꾹 눌렀을 때] (클릭 시작 구역)
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

            // 💡 [2D 월드 조준경으로 교체] 마우스 위치에 있는 2D 콜라이더 블록을 정밀 포착합니다!
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

            if (hitCollider != null && hitCollider.gameObject.name.Contains("Block_"))
            {
                selectedBlock = hitCollider.gameObject;
                clickStartPos = mouseScreenPos;

                // 📐 [이름표 좌표 추출] 기존 유저님의 splitName 로직 그대로 유지
                string[] splitName = selectedBlock.name.Replace("Block_(", "").Replace(")", "").Split(',');
                int.TryParse(splitName[0], out startX);
                int.TryParse(splitName[1], out startY);
            }
        }


        // 2. 마우스 왼쪽 버튼을 [뗄 때] (드래그 종료 및 방향 계산) - 최신 패키지 규격 연동
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame && selectedBlock != null)
        {
            // 최신식 마우스 뗀 위치 좌표 실시간 수신
            Vector2 clickEndPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            Vector2 delta = clickEndPos - clickStartPos; // 마우스가 움직인 거리와 방향 변위

            // // 최소 30픽셀 이상은 드래그해야 사용자가 움직인 것으로 인정합니다 (미끄러짐 방지)
            if (delta.magnitude > 30f)
            {
                CalculateSwipeDirection(delta);
            }

            selectedBlock = null; // // 조작 완료 후 선택 해제
        }
    }

    // 🎯 [3매치 종합 판단 및 조작 동기화 엔진] 대각선 차단 및 1칸 이동 판정 구역
    private void CalculateSwipeDirection(Vector2 delta)
    {
        int targetX = startX;
        int targetY = startY;

        // 1. [기획 규칙]: 마우스 움직임 축을 저울질하여 상하좌우 딱 1칸만 허용 (대각선 미끄러짐 원천 차단)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            targetX += delta.x > 0 ? 1 : -1;
        }
        else
        {
            targetY += delta.y > 0 ? 1 : -1;
        }

        // 2. 8x8 보드판 테두리 범위 내부일 때만 진짜 플레이 작동
        // 📐 [Match3Board.cs 내부 CalculateSwipeDirection 최종 전선 직결 교체]
        if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
        {
            Match3GameManager manager = FindAnyObjectByType<Match3GameManager>();
            if (manager != null)
            {
                Debug.Log($"[시스템 통제] ({startX}, {startY})에서 ({targetX}, {targetY})로 드래그 감지. 판정을 시작합니다.");

                // 💡 [치료 열쇠]: 주소를 확실하게 추적해 지휘관 내부의 SwapBlocks를 다이렉트로 관통 호출합니다!
                // Vector4 데이터 단락을 생성해 그대로 전송해 줍니다.
                Vector4 swipeVector = new Vector4(startX, startY, targetX, targetY);
                manager.SwapBlocks(swipeVector); 
            }
        }

        else
        {
            Debug.LogWarning("⚠ [벽 충돌] 보드판 영역 바깥으로 튕겨 나가 조작이 차단되었습니다.");
        }
    }





}
