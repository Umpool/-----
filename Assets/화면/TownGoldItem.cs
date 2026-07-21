using UnityEngine;
using UnityEngine.UI;

public class TownGoldItem : MonoBehaviour
{
 
    private Button button;
    private TownGoldSpawner spawner;

    // 생성기(Spawner)의 정보를 넘겨받는 초기화 함수
    public void Initialize(TownGoldSpawner manager)
    {
        spawner = manager;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickedGold);
        }
    }

    private void OnClickedGold()
    {
        // [인스펙터 랜덤 동기화] 내 주인 스포너가 정해둔 인스펙터 범위 내에서 주사위를 굴립니다.
        if (spawner != null)
        {
            int min = spawner.MinGoldAmount;
            int max = spawner.MaxGoldAmount;

            // 최소~최대 값 사이의 랜덤한 골드 계산 (max + 1을 해야 최대치까지 포함됩니다)
            int randomGold = Random.Range(min, max + 1);

            // 싱글톤 매니저를 통해 유저 데이터에 최종 랜덤 골드 지급 및 저장
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddGold(randomGold);
                Debug.Log($"[GoldItem] 대박 찬스! 인스펙터 설정 범위({min}~{max}) 내에서 {randomGold} 골드가 획득되었습니다.");
            }
        }


        // 스포너에게 자신이 삭제됨을 알림 (개수 카운트 감소용)
        if (spawner != null)
        {
            spawner.OnGoldCollected(gameObject);
        }

        // 골드 오브젝트 파괴
        Destroy(gameObject);
    }

}
