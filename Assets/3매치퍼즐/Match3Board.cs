using UnityEngine;
using System.Collections;

public class Match3Board : MonoBehaviour
{
    [Header("--- Board Settings ---")]
    public int width = 8; 
    public int height = 8;
    public float cellSize = 1.0f;
    public Transform boardParent;

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

    public void InitializeBoard()
    {
        ClearBoardObjects();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SpawnBlockAtPosition(x, y);
            }
        }
    }

    public void SpawnBlockAtPosition(int x, int y)
    {
        int randomIndex = Random.Range(0, blockPrefabs.Length);
        Vector3 spawnPos = GetWorldPosition(x, y);
        
        GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPos, Quaternion.identity, boardParent);
        newBlock.name = blockPrefabs[randomIndex].name; 
        
        boardArray[y * width + x] = newBlock;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 startPos = boardParent != null ? boardParent.position : Vector3.zero;
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

}
