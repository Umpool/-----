using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    // 어디서나 접근 가능하도록 싱글톤 설정
    public static CurrencyManager Instance { get; private set; }

    // 골드가 변경될 때마다 UI에 알려줄 이벤트 (옵저버 패턴)
    public static event Action<int> OnGoldChanged;

    private const string GoldSaveKey = "User_Gold_Data"; // 저장 키값
    private int currentGold = 0;

    // 외부에서 현재 골드를 읽을 수 있는 프로퍼티
    public int CurrentGold => currentGold;

    void Awake()
    {
        // 싱글톤 중복 생성 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
            LoadGold(); // 게임 시작 시 저장된 골드 로드
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 골드를 추가하는 함수 (몬스터 처치, 퀘스트 완료 등)
    public void AddGold(int amount)
    {
        currentGold += amount;
        SaveGold();
        NotifyGoldChanged();
    }

    // 골드를 사용하는 함수 (아이템 구매 등)
    // 반환값(bool): 골드가 충분해서 소비에 성공했는지 여부
    public bool ConsumeGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            SaveGold();
            NotifyGoldChanged();
            return true;
        }

        Debug.LogWarning("골드가 부족합니다!");
        return false;
    }

    // 데이터를 기기에 저장
    private void SaveGold()
    {
        PlayerPrefs.SetInt(GoldSaveKey, currentGold);
        PlayerPrefs.Save();
    }

    // 저장된 데이터를 불러오기 (처음 게임을 하면 0원부터 시작)
    private void LoadGold()
    {
        currentGold = PlayerPrefs.GetInt(GoldSaveKey, 0);
    }

    // UI 스크립트들에게 골드가 변했다고 신호를 보냄
    public void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(currentGold);
    }
}
