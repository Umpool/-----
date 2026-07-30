using UnityEngine;

public enum EventType
{
    NothingFound,   // 1. 아무것도 찾지 못함
    RewardItem,     // 2. 재화 및 아이템(컬러) 획득
    MeetMonster,    // 3. 몬스터 조우 (3매치 전환)
    MeetPerson      // 4. 사람 조우 (5명 바리에이션)
}

// 컬러 재화 종류 정의
public enum ColorType
{
    None,
    Red,    // 적색
    Yellow, // 황색
    Green,  // 녹색
    Blue,   // 청색
    Purple,  // 자색
    Gold   // 골드
}

// 만날 사람 NPC 종류 정의
public enum NpcType
{
    None,
    Chief,     // 촌장
    Merchant,  // 상인
    Healer,    // 치료사
    Warrior,   // 전사
    archaeologist    // 고고학자
}

[CreateAssetMenu(fileName = "NewEventData", menuName = "RPG/Event Data")]
public class AdventureScenarioData : ScriptableObject
{
    [Header("[이벤트 기본 설정]")]
    public string eventName;               
    public EventType eventType;            

    [Header("[텍스트 내용]")]
    [TextArea(3, 6)]
    public string[] eventDialogues;        

    [Header("[이벤트 완료 후 선택지 안내문]")]
    [TextArea(2, 4)]
    public string nextActionPrompt;        

    [Header("[2. 보상 이벤트 전용 설정]")]
    public bool giveGold = true;           // 골드 지급 여부
    public ColorType rewardColor;          // 획득할 컬러 아이템 종류 (None이면 골드만)

    [Header("[4. 사람 조우 이벤트 전용 설정]")]
    public NpcType npcType;                // 만날 NPC 종류 선택
}
