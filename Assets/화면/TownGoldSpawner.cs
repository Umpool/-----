using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TownGoldSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private RectTransform townRectTransform;

    [Header("스폰 규칙 설정")]
    [SerializeField] private float initialDelay = 10f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxGoldCount = 5;

    // [랜덤 범위 추가] 인스펙터에서 최소/최대 골드 획득량을 내 입맛대로 조절합니다.
    [SerializeField] private int minGoldAmount = 100;
    [SerializeField] private int maxGoldAmount = 500;

    // 코인이 이 범위 데이터를 안전하게 훔쳐갈 수 있도록 입구를 열어줍니다.
    public int MinGoldAmount => minGoldAmount;
    public int MaxGoldAmount => maxGoldAmount;

    private List<GameObject> activeGolds = new List<GameObject>();

    [Header("감시할 이벤트 화면")]
    [SerializeField] private GameObject secondEventPanel;


    // 코인 프리팹의 가로/세로 크기를 미리 저장할 변수
    private float coinHalfWidth = 0f;
    private float coinHalfHeight = 0f;

    // ⬇️ [34번째 줄 void Start() 자리를 아래 OnEnable()로 교체합니다] ⬇️
    private void OnEnable()
    {
        // 1. [유령 복제 방어선] 화면이 켜질 때마다 타이머가 중복 가동되어 코인이 2배로 폭발 생산되는 현상을 방지합니다.
        StopAllCoroutines();

        if (townRectTransform == null)
        {
            townRectTransform = GetComponent<RectTransform>();
        }

        // 게임 시작 및 화면 복귀 시 코인 프리팩의 크기를 가져와서 절반 값을 미리 계산합니다.
        if (goldPrefab != null)
        {
            RectTransform prefabRect = goldPrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                coinHalfWidth = prefabRect.rect.width / 2f;
                coinHalfHeight = prefabRect.rect.height / 2f;
            }
        }

        // 2. [실시간 가동 핵심] 유저의 화면이 마을을 보고 있는 "활성화(OnEnable) 시점"부터 코인 생성 딜레이를 새롭게 출발시킵니다!
        StartCoroutine(SpawnGoldRoutine());
        Debug.Log("[TownGoldSpawner] 유저가 마을 화면으로 진입함 -> 코인 생성 자동 루틴 새 출발 가동 완료!");
    }

    private IEnumerator SpawnGoldRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // [위치 변경 및 안전벽 보강] 코인 생성 전 이벤트 패널 활성화 상태를 철저히 검사
            if (secondEventPanel != null && secondEventPanel.activeInHierarchy)
            {
                yield return new WaitForSeconds(0.5f);
                continue; // 이벤트 중에는 코인 생성 차단
            }

            if (activeGolds.Count < maxGoldCount)
            {
                SpawnGold();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    
}

    private void SpawnGold()
    {
        if (goldPrefab == null || townRectTransform == null) return;

        // 1. 마을 해상도(4000x2500)의 중심(0,0) 기준 최대 가동 영역 계산
        float maxHalfWidth = townRectTransform.rect.width / 2f;
        float maxHalfHeight = townRectTransform.rect.height / 2f;

        // 2. [핵심] 마을 최대 크기에서 코인의 반지름(크기의 절반)만큼 안쪽으로 영역을 좁힙니다.
        // 추가로 유저가 커스텀 여백을 더 주고 싶다면 + 20f 처럼 여백을 더 더해줄 수도 있습니다.
        float edgeSafetyMargin = 30f; // 마을 테두리 벽에서 조금 더 떨어지게 만드는 추가 여백

        float safeMinX = -maxHalfWidth + coinHalfWidth + edgeSafetyMargin;
        float safeMaxX = maxHalfWidth - coinHalfWidth - edgeSafetyMargin;

        float safeMinY = -maxHalfHeight + coinHalfHeight + edgeSafetyMargin;
        float safeMaxY = maxHalfHeight - coinHalfHeight - edgeSafetyMargin;

        // 3. 완벽하게 안전한 내부 영역 안에서만 랜덤 좌표 추출
        float randomX = Random.Range(safeMinX, safeMaxX);
        float randomY = Random.Range(safeMinY, safeMaxY);

        Vector2 spawnPosition = new Vector2(randomX, randomY);

        // 자식 오브젝트로 코인 생성
        GameObject newGold = Instantiate(goldPrefab, townRectTransform);

        RectTransform goldRect = newGold.GetComponent<RectTransform>();
        if (goldRect != null)
        {
            goldRect.anchoredPosition = spawnPosition;
        }

        TownGoldItem goldItem = newGold.GetComponent<TownGoldItem>();
        if (goldItem != null)
        {
            goldItem.Initialize(this);
        }

        activeGolds.Add(newGold);
    }

    public void OnGoldCollected(GameObject goldObject)
    {
        if (activeGolds.Contains(goldObject))
        {
            activeGolds.Remove(goldObject);
        }
    }
}
