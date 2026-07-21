using UnityEngine;

// 프로젝트 창에서 마우스 우클릭으로 데이터 파일을 만들 수 있게 해주는 마법의 한 줄입니다.
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/Create Data")]
public class CharacterData : ScriptableObject
{
    [Header("영웅 고유 정보")]
    [Tooltip("캐릭터의 고유 ID (예: 001, 002 등)")]
    public string characterID;

    [Tooltip("출력될 영웅 이름")]
    public string characterName;

    [Tooltip("캐릭터의 종족 (예: 로봇, 인간, 엘프 등)")]
    public string characterRace;

    [TextArea(2, 5)]
    [Tooltip("영웅 설명 및 정보")]
    public string characterInfo;

    [Tooltip("영웅 일러스트 이미지")]
    public Sprite characterSprite;

    [Tooltip("임시 테스트용 영웅 고유 색상")]
    public Color characterColor = Color.white;


    [Header("전투 능력치")]
    [Tooltip("공격력 (Attack Power)")]
    public int attackPower;

    [Tooltip("체력 (HP)")]
    public int hp;

    [Tooltip("방어력 (Defense)")]
    public int defense;


    [Header("특수 능력 및 시스템")]
    [Tooltip("각 캐릭터의 고유 능력 이름")]
    public string uniqueSkillName;

    [TextArea(2, 4)]
    [Tooltip("고유 능력에 대한 상세 설명")]
    public string uniqueSkillDescription;

    [Tooltip("소속 시너지 시스템 이름 (예: 블랙, 레드 등)")]
    public string synergySystem;
}
