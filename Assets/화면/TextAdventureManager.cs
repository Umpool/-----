using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextAdventureManager : MonoBehaviour
{
    [Header("[RPG 출력용 UI 세팅]")]
    public TextMeshProUGUI logText;       // 대사가 출력될 '대사' 오브젝트 (TMP)

    [Header("[모험에 필요한 3개 버튼]")]
    public Button leftButton;             // 좌측 버튼
    public Button rightButton;            // 우측 버튼 (하단 버튼을 우측으로 쓰신다면 그것을 연결)
    public Button topButton;              // 상단 버튼

    [Header("[보상용 캐릭터 데이터베이스]")]
    public CharacterData[] poolCompanions; // 획득 가능한 동료 데이터 (ScriptableObject) 배열

    void Start()
    {
        // 씬이 처음 시작될 때 안내 문구를 띄웁니다.
        if (logText != null)
        {
            logText.text = "새로운 모험이 시작되었습니다.\n이동할 방향을 선택하세요.";
        }

        // 3개의 버튼에 각각 클릭했을 때 무작위 이벤트가 터지도록 기능을 연결합니다.
        if (leftButton != null) leftButton.onClick.AddListener(() => TriggerRandomEvent("좌측"));
        if (rightButton != null) rightButton.onClick.AddListener(() => TriggerRandomEvent("우측"));
        if (topButton != null) topButton.onClick.AddListener(() => TriggerRandomEvent("상단"));
    }

    // 어떤 버튼을 누르든 이 함수가 실행되며 랜덤한 사건이 발생합니다.
    private void TriggerRandomEvent(string direction)
    {
        // 0부터 2까지의 숫자를 무작위로 뽑습니다 (0: 배틀, 1: 아이템, 2: 동료)
        int randomResult = Random.Range(0, 3); 

        switch (randomResult)
        {
            case 0:
                StartTextBattle(direction);
                break;
            case 1:
                GainAdventureItem(direction);
                break;
            case 2:
                RecruitNewCompanion(direction);
                break;
        }
    }

    // 1. 배틀 이벤트 발생시 실행되는 함수
    private void StartTextBattle(string direction)
    {
        int dmg = Random.Range(10, 30);
        logText.text = $"⚔️ [{direction} 탐색] 거대 몬스터가 나타났습니다!\n치열한 전투 끝에 승리하여 캐릭터들이 {dmg}의 경험치를 획득했습니다.";
    }

    // 2. 아이템(재화) 획득시 실행되는 함수
    private void GainAdventureItem(string direction)
    {
        int goldAmount = Random.Range(50, 200);
        
        // 기존 프로젝트에 CurrencyManager(돈 관리자)가 있다면 연동 가능
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(goldAmount);
        }

        logText.text = $"💎 [{direction} 탐색] 숨겨진 상자를 발견했습니다!\n상자 안에서 <color=yellow>{goldAmount} 골드</color>와 유용한 모험 아이템을 얻었습니다.";
    }

    // 3. 동료 획득시 실행되는 함수 (유저님의 CharacterData 활용)
    private void RecruitNewCompanion(string direction)
    {
        // 유니티 인스펙터 창에 등록한 캐릭터가 없을 때의 예외 처리입니다.
        if (poolCompanions == null || poolCompanions.Length == 0)
        {
            logText.text = $"🏃 [{direction} 탐색] 누군가 지나간 흔적이 보이지만, 지금은 아무도 만날 수 없었습니다.";
            return;
        }

        // 준비된 동료 데이터 풀에서 랜덤으로 1명을 추첨합니다.
        CharacterData newFriend = poolCompanions[Random.Range(0, poolCompanions.Length)];

        // 기존 인벤토리 시스템(UserCharacterInventory)이 살아있다면 획득 처리 연동
        if (UserCharacterInventory.Instance != null)
        {
            UserCharacterInventory.Instance.AddCharacter(newFriend.characterID);
        }

        logText.text = $"🤝 [{direction} 탐색] 길을 헤매던 <color=cyan>[{newFriend.characterName}]</color>(이)가 파티에 합류하고 싶어 합니다!\n새로운 동료를 획득했습니다.";
    }
}