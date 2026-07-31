using UnityEngine;
using System.Collections;

public class Match3Referee : MonoBehaviour
{
    [Header("--- Block Damage Settings ---")]
    public string[] blockNames;        // 인스펙터에서 RED, BLUE, GREEN 등 명칭 작성
    public int[] blockBaseDamages;     // 기획서 반영: 색상마다 부여할 고유 대미지 수치

    private Match3Board board;

    void Awake()
    {
        board = GetComponent<Match3Board>();
    }

    // [판정] 현재 퍼즐보드 전체를 검사하여 가로/세로 3개 이상 일치하는 블록 맵을 반환
    public bool[,] EvaluateBoardMatches()
    {
        int w = board.Width;
        int h = board.Height;
        GameObject[] boardArray = board.BoardArray;
        bool[,] matchMap = new bool[w, h];

        // 1) 가로 방향 3매치 판정 검사
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w - 2; x++)
            {
                string b1 = boardArray[y * w + x]?.name;
                string b2 = boardArray[y * w + (x + 1)]?.name;
                string b3 = boardArray[y * w + (x + 2)]?.name;

                if (!string.IsNullOrEmpty(b1) && b1 == b2 && b2 == b3)
                {
                    matchMap[x, y] = true;
                    matchMap[x + 1, y] = true;
                    matchMap[x + 2, y] = true;
                }
            }
        }

        // 2) 세로 방향 3매치 판정 검사
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h - 2; y++)
            {
                string b1 = boardArray[y * w + x]?.name;
                string b2 = boardArray[(y + 1) * w + x]?.name;
                string b3 = boardArray[(y + 2) * w + x]?.name;

                if (!string.IsNullOrEmpty(b1) && b1 == b2 && b2 == b3)
                {
                    matchMap[x, y] = true;
                    matchMap[x, y + 1] = true;
                    matchMap[x, y + 2] = true;
                }
            }
        }

        return matchMap;
    }

    // 보드판 위에 터뜨려야 할 매치가 하나라도 존재하는지 확인하는 기능
    public bool HasAnyMatch(bool[,] matchMap)
    {
        int w = board.Width;
        int h = board.Height;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (matchMap[x, y]) return true;
            }
        }
        return false;
    }

    // [3매치 성공] 파괴되는 블록의 수와 고유 색상, 콤보에 따른 대미지 계산
    public float CalculateTotalDamage(bool[,] matchMap, int comboCount)
    {
        int w = board.Width;
        int h = board.Height;
        GameObject[] boardArray = board.BoardArray;
        float totalDamage = 0;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (!matchMap[x, y]) continue;
                int index = y * w + x;
                if (boardArray[index] == null) continue;

                // [블록] 기획서 내용 반영: 블록 이름(색상)마다 지정된 고유 대미지 서치
                string currentBlockName = boardArray[index].name;
                int baseDamage = 10; // 인스펙터 세팅 누락 시 작동할 기본 데미지 수치

                if (blockNames != null && blockBaseDamages != null)
                {
                    for (int i = 0; i < blockNames.Length; i++)
                    {
                        if (blockNames[i] == currentBlockName && i < blockBaseDamages.Length)
                        {
                            baseDamage = blockBaseDamages[i];
                            break;
                        }
                    }
                }
                totalDamage += baseDamage;
            }
        }

        // [콤보] 기획서 내용 반영: 콤보수에 따른 대미지 배율 가산 규칙 구현
        // 계산 공식 예시: 1콤보(기본), 2콤보부터 10%씩 추가 가산 (1.1배, 1.2배...)
        float comboMultiplier = 1f + (comboCount - 1) * 0.1f;
        totalDamage *= comboMultiplier;

        // [시너지 확장 영역]
        // 추후 이 자리에 유저의 캐릭터 시너지 효과에 따른 가산 공식을 추가해 얹어줄 수 있습니다!
        
        return totalDamage;
    }

    // 매치 판정을 받은 블록 오브젝트들을 하이어라키에서 파괴하고 비우는 연출 코루틴
    public IEnumerator ClearMatchedBlocks(bool[,] matchMap)
    {
        int w = board.Width;
        int h = board.Height;
        GameObject[] boardArray = board.BoardArray;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (matchMap[x, y])
                {
                    int index = y * w + x;
                    if (boardArray[index] != null)
                    {
                        Destroy(boardArray[index]);
                        boardArray[index] = null;
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f); // 파괴 연출 대기 시간
    }

    // 1-2) 블록이 파괴된 빈칸은 위에 있는 블록이 아래로 내려와 빈칸을 채운다
    public IEnumerator GravityDropBlocks()
    {
        int w = board.Width;
        int h = board.Height;
        GameObject[] boardArray = board.BoardArray;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int index = y * w + x;
                if (boardArray[index] == null)
                {
                    // 해당 빈칸 바로 위칸부터 꼭대기까지 탐색하여 살아있는 블록을 아래로 당김
                    for (int nextY = y + 1; nextY < h; nextY++)
                    {
                        int nextIndex = nextY * w + x;
                        if (boardArray[nextIndex] != null)
                        {
                            boardArray[index] = boardArray[nextIndex];
                            boardArray[nextIndex] = null;
                            
                            // 아래로 미끄러지는 연출 시작
                            StartCoroutine(board.MoveBlockAnimation(boardArray[index], new Vector2Int(x, y)));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f); // 낙하 연출 완료 대기
    }

    // 화면 12시 방향(최상단)에서 새로운 블록들이 생성되어 내려와 모든 빈칸을 마저 채운다
    public IEnumerator RefreshNewTopBlocks()
    {
        int w = board.Width;
        int h = board.Height;
        GameObject[] boardArray = board.BoardArray;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int index = y * w + x;
                if (boardArray[index] == null)
                {
                    // 12시 가상 위치(최상단 너머 h 좌표)에서 생성되어 자연스럽게 안착하도록 세팅
                    int randomIndex = Random.Range(0, board.blockPrefabs.Length);
                    Vector3 spawnPos = board.GetWorldPosition(x, h);
                    
                    GameObject newBlock = Instantiate(board.blockPrefabs[randomIndex], spawnPos, Quaternion.identity, board.boardParent);
                    newBlock.name = board.blockPrefabs[randomIndex].name;
                    
                    boardArray[index] = newBlock;
                    StartCoroutine(board.MoveBlockAnimation(boardArray[index], new Vector2Int(x, y)));
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
    }
}
