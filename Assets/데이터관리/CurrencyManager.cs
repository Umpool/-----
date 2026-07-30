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
        // [컬러 재화 내부 저장 변수]
    private int redColor = 0;
    private int yellowColor = 0;
    private int greenColor = 0;
    private int blueColor = 0;
    private int purpleColor = 0;

    // 외부에서 각각의 컬러 개수를 읽을 수 있도록 통로 열기
    public int RedColor => redColor;
    public int YellowColor => yellowColor;
    public int GreenColor => greenColor;
    public int BlueColor => blueColor;
    public int PurpleColor => purpleColor;


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
        // 모험 및 전투 승리 시 컬러 재화를 적립해주는 함수
    public void AddColor(string colorType, int amount)
    {
        // 들어온 색상 텍스트 문자에 따라 알맞은 서랍에 더해줍니다.
        if (colorType == "Red") redColor += amount;
        else if (colorType == "Yellow") yellowColor += amount;
        else if (colorType == "Green") greenColor += amount;
        else if (colorType == "Blue") blueColor += amount;
        else if (colorType == "Purple") purpleColor += amount;

        // 즉시 하드디스크에 영구 저장을 보냅니다.
        PlayerPrefs.SetInt("User_Color_" + colorType, PlayerPrefs.GetInt("User_Color_" + colorType, 0) + amount);
        PlayerPrefs.Save();

        Debug.Log("[CurrencyManager] " + colorType + " 컬러가 +" + amount + "개 획득 및 영구 저장되었습니다.");
    }

    // 게임 시작 시 하드디스크 서랍에서 예전에 모아둔 컬러 개수를 로드하는 함수
    public void LoadColors()
    {
        redColor = PlayerPrefs.GetInt("User_Color_Red", 0);
        yellowColor = PlayerPrefs.GetInt("User_Color_Yellow", 0);
        greenColor = PlayerPrefs.GetInt("User_Color_Green", 0);
        blueColor = PlayerPrefs.GetInt("User_Color_Blue", 0);
        purpleColor = PlayerPrefs.GetInt("User_Color_Purple", 0);
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
