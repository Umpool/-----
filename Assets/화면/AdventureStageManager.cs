using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가!

public class AdventureStageManager : MonoBehaviour
{
    [Header("[연결할 매니저 및 데이터 창고]")]
    public TextRPGUIManager uiManager;
    public List<AdventureScenarioData> allEvents = new List<AdventureScenarioData>();

    [Header("[모험 최초 진입 튜토리얼 설정]")]
    [TextArea(2, 5)]
    public string[] introductionDialogues; 

    private bool isNextButtonClicked = false; 
    private bool isLeftButtonClicked = false;
    private bool isRightButtonClicked = false;

    void Start()
    {
        // 버튼 이벤트 연결
        if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.onClick.AddListener(() => isNextButtonClicked = true);
        if (uiManager.leftButton != null) uiManager.leftButton.onClick.AddListener(() => isLeftButtonClicked = true);
        if (uiManager.rightButton != null) uiManager.rightButton.onClick.AddListener(() => isRightButtonClicked = true);

        StartCoroutine(AdventureLoop());
    }

        private IEnumerator AdventureLoop()
    {
        // ----------------------------------------------------------------------
        // [★ 완전 개조된 모험 씬 진입 연출 단계]
        // ----------------------------------------------------------------------
        // 1. 모험 씬에 오자마자 좌/우/상단 방향 버튼은 확실하게 숨겨둡니다.
        uiManager.SetDirectionButtonsActive(false);

        // 2. 대신 유저가 화면 어디든 누를 수 있게 '종합 버튼'을 뿅 켜줍니다!
        if (uiManager.nextDialogueButton != null) 
            uiManager.nextDialogueButton.gameObject.SetActive(true);

        // 3. 네가 기획한 자연스러운 모험 시작 알림 문구를 부드럽게 출력합니다.
        uiManager.SetTextTyping("새로운 모험이 눈앞에 기다리고 있습니다!\n(화면을 터치하여 모험 시작)");

        // 4. 유저가 화면(종합 버튼)을 딸깍 누를 때까지 코드를 완전히 멈추고 기다립니다.
        isNextButtonClicked = false;
        while (!isNextButtonClicked) yield return null;

        // 5. 유저가 터치했으므로, 이제 주사위를 굴리기 직전에 종합 버튼을 깔끔하게 꺼줍니다!
        if (uiManager.nextDialogueButton != null) 
            uiManager.nextDialogueButton.gameObject.SetActive(false);

        // 첫 시작 알림을 보고 터치했으니 맛깔나게 0.3초만 숨을 고르고 본격적인 루프로 진입합니다.
        yield return new WaitForSeconds(0.3f);


        // 🔄 이 아래부터는 언제나 정상적으로 전진할 때마다 작동하는 무한 모험 루프입니다.
        while (true)
        {
            // 🎲 1. 주사위를 굴려 30/30/30/10 확률 변수 판단
            int dice = Random.Range(1, 101);
            EventType chosenType;

            if (dice <= 30)          chosenType = EventType.NothingFound;
            else if (dice <= 60)     chosenType = EventType.RewardItem;  
            else if (dice <= 90)     chosenType = EventType.MeetMonster; 
            else                     chosenType = EventType.MeetPerson;  

            List<AdventureScenarioData> matchedEvents = allEvents.FindAll(e => e.eventType == chosenType);

            if (matchedEvents.Count > 0)
            {
                AdventureScenarioData currentEvent = matchedEvents[Random.Range(0, matchedEvents.Count)];
                // 🎬 뽑힌 이벤트를 실행합니다. (이벤트 내부에서 대사창/버튼들을 제어하게 됨)
                yield return StartCoroutine(PlayEvent(currentEvent));
            }
            else
            {
                Debug.LogWarning($"[경고] {chosenType} 타입의 이벤트 파일이 All Events 리스트에 등록되지 않았습니다.");
                yield return new WaitForSeconds(1f);
            }

            // 하나의 이벤트가 완벽히 끝나고 유저가 '계속 전진'을 골랐다면, 1.5초 대기 후 다음 전진 유도
            yield return new WaitForSeconds(1.5f);

            // ----------------------------------------------------------------------
            // [정상 전진 대기 단계] 다음 턴으로 넘어가기 위한 연출 세팅
            // ----------------------------------------------------------------------
            // 2. 방향 버튼(상단 버튼 포함)은 모두 확실하게 꺼줍니다.
            uiManager.SetDirectionButtonsActive(false);
            
            // 3. 대신 다음 칸으로 전진하기 위해 '종합 버튼'을 다시 켜줍니다.
            if (uiManager.nextDialogueButton != null) 
                uiManager.nextDialogueButton.gameObject.SetActive(true);
            
            // 4. 안내 문구를 화면에 출력합니다.
            uiManager.SetTextInstant("앞으로 전진하려면 화면을 터치하십시오.");

            // 5. 유저가 전진하기 위해 화면을 누를 때까지 기다립니다.
            isNextButtonClicked = false; 
            while (!isNextButtonClicked) yield return null;

            // 6. 클릭이 완료되어 다음 전진을 시작하므로, 다시 종합 버튼을 꺼줍니다.
            if (uiManager.nextDialogueButton != null) 
                uiManager.nextDialogueButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayEvent(AdventureScenarioData data)
    {
        uiManager.SetDirectionButtonsActive(false);

        // [공통] 해당 이벤트에 적혀있는 기본 스토리 대사들을 3초 간격으로 순서대로 출력합니다.
        for (int i = 0; i < data.eventDialogues.Length; i++)
        {
            uiManager.SetTextTyping(data.eventDialogues[i]);
            yield return new WaitForSeconds(3.0f);
        }

        // [분기 처리] 4가지 확률 변수별 개별 행동 기믹
        switch (data.eventType)
        {
            case EventType.NothingFound:
                uiManager.SetTextInstant(data.nextActionPrompt);
                yield return new WaitForSeconds(2.5f);
                break;

            case EventType.RewardItem:
            case EventType.MeetMonster:
            case EventType.MeetPerson:
                uiManager.SetTextInstant(data.nextActionPrompt);
                
                if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);
                if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(true);
                if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(true);

                isLeftButtonClicked = false;
                isRightButtonClicked = false;
                while (!isLeftButtonClicked && !isRightButtonClicked) yield return null;

                if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
                if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

                if (isLeftButtonClicked)
                {
                    // 🔴 [2번: 일반 재화/상자 조사 기믹]
                    // 🔴 [2번: 재화 및 컬러 아이템 진짜 지갑에 적립하기]
                    if (data.eventType == EventType.RewardItem)
                    {
                        uiManager.SetTextInstant("아이템을 확인하는중."); yield return new WaitForSeconds(1.0f);
                        uiManager.SetTextInstant("아이템을 확인하는중.."); yield return new WaitForSeconds(1.0f);
                        uiManager.SetTextInstant("아이템을 확인하는중..."); yield return new WaitForSeconds(1.0f);

                        string rewardResultText = "성공적으로 조사를 마쳤습니다!\n";
                        
                        // 1. 진짜 골드 재화 지급 및 하드디스크 영구 적립
                        if (data.giveGold)
                        {
                            int rewardGold = Random.Range(1, 21); // 1~20 랜덤 골드 계산
                            rewardResultText += $"획득 재화: +{rewardGold} 골드\n";

                            // 💡 현재 하드디스크에 저장되어 있는 유저의 기존 골드 총량을 꺼내옵니다.
                            // (마을 스크립트에서 쓰는 진짜 골드 키 이름이 "Gold"라면 "Gold"로, "PlayerGold"라면 바꿔주면 돼!)
                            int currentGold = PlayerPrefs.GetInt("Gold", 0); 
                            
                            // 기존 돈에 모험에서 방금 번 돈을 더해줍니다.
                            int updatedGold = currentGold + rewardGold;

                            // 쾅! 더해진 최종 돈을 하드디스크 서랍에 안전하게 저장합니다.
                            PlayerPrefs.SetInt("Gold", updatedGold);
                            PlayerPrefs.Save();

                            Debug.Log($"[모험 보상] 골드 적립 완료! 기존: {currentGold}G -> 현재: {updatedGold}G");
                        }

                        // 2. 진짜 5색 컬러 특수 재화 지급 및 하드디스크 영구 적립
                        if (data.rewardColor != ColorType.None)
                        {
                            string colorName = GetColorNameKorean(data.rewardColor);
                            rewardResultText += $"특수 아이템 획득: [{colorName} 컬러]를 얻었습니다!";

                            // 💡 컬러 재화도 똑같이 영구 저장소에 종류별로 개수를 누적합니다.
                            // 예: 서랍 이름은 "Color_Red", "Color_Blue" 형태로 저장됩니다.
                            string colorSaveKey = "Color_" + data.rewardColor.ToString();
                            int currentColorCount = PlayerPrefs.GetInt(colorSaveKey, 0);
                            
                            PlayerPrefs.SetInt(colorSaveKey, currentColorCount + 1); // 개수 1개 누적
                            PlayerPrefs.Save();

                            Debug.Log($"[모험 보상] {colorName} 컬러 적립 완료! 현재 개수: {currentColorCount + 1}개");
                        }

                        uiManager.SetTextInstant(rewardResultText);

                        if (uiManager.nextDialogueButton != null) 
                            uiManager.nextDialogueButton.gameObject.SetActive(false);
                    }

                    // ⚔️ [3번: 몬스터 조우 기믹]
                    else if (data.eventType == EventType.MeetMonster)
                    {
                        uiManager.SetTextInstant("전투에 돌입합니다! 3매치 퍼즐 화면으로 이동 중...");
                        yield return new WaitForSeconds(2.0f);
                        SceneManager.LoadScene("PuzzleBattleScene"); 
                        yield break; 
                    }
                    // 👥 [4번: ★ 대폭 확장된 NPC별 맞춤 대사 및 고유 컬러 지급 기믹]
                    else if (data.eventType == EventType.MeetPerson)
                    {
                        string npcResultText = "";
                        
                        // 각 NPC 직업에 맞는 고유 멘트와 보상 컬러 매칭 [1]
                        switch (data.npcType)
                        {
                            case NpcType.Chief: // 촌장 (그린)
                                npcResultText = "[마을 촌장]: \"이 척박한 땅을 개척하느라 수고가 많네. 생명의 온기가 담긴 색을 주지.\"\n\n🎁 [그린 컬러]를 획득했습니다!";
                                break;
                            case NpcType.Merchant: // 상인 (블루)
                                npcResultText = "[방랑 상인]: \"귀한 손님을 만났군요! 제 비밀 보따리에서 나온 신비한 색입니다.\"\n\n🎁 [블루 컬러]를 획득했습니다!";
                                break;
                            case NpcType.Healer: // 치료사 (옐로)
                                npcResultText = "[약초 치료사]: \"여독이 깊어 보이시네요. 마음을 치유해 주는 따뜻한 색을 나눌게요.\"\n\n🎁 [옐로 컬러]를 획득했습니다!";
                                break;
                            case NpcType.Warrior: // 전사 (레드)
                                npcResultText = "[낙향한 전사]: \"네 눈빛에서 뜨거운 투지가 느껴지는군! 나의 열정을 너에게 전하마.\"\n\n🎁 [레드 컬러]를 획득했습니다!";
                                break;
                            case NpcType.archaeologist: // 학자 (퍼플)
                                npcResultText = "[고고학자]: \"수수께끼 가득한 무채색의 비밀을 풀 실마리... 이 신비로운 색을 보게.\"\n\n🎁 [퍼플 컬러]를 획득했습니다!";
                                break;
                            default:
                                npcResultText = "신비한 나그네와 따뜻한 대화를 나누며 유대를 쌓았습니다.";
                                break;
                        }

                        uiManager.SetTextInstant(npcResultText);
                    }
                    
                    if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);
                    yield return new WaitForSeconds(3.5f); 
                }
                else if (isRightButtonClicked)
                {
                    uiManager.SetTextInstant("위험 요소나 번거로운 상황을 무시하고 조용히 발걸음을 옮깁니다.");
                    yield return new WaitForSeconds(2.5f);
                }
                break;
        }

        // ----------------------------------------------------------------------
        // [모험 종료 / 지속 여부를 묻는 최종 순간 선택 단계]
        // ----------------------------------------------------------------------
        uiManager.SetTextInstant("이벤트가 마무리되었습니다.\n계속 전진하시겠습니까, 아니면 마을로 돌아가시겠습니까?");
        
        // 💡 [★ 수정] 최종 갈림길 선택지가 나올 때도 화면 터치용 종합 버튼을 확실하게 꺼줍니다!
        if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);

        // 다시 좌측(계속 모험)과 우측(마을 귀환) 버튼을 활성화합니다.
        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(true);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(true);

        isLeftButtonClicked = false;
        isRightButtonClicked = false;
        while (!isLeftButtonClicked && !isRightButtonClicked) yield return null;

        // 선택이 끝났으니 방향 버튼을 깔끔하게 숨깁니다.
        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

if (isRightButtonClicked)
{
    uiManager.SetTextInstant("모험을 마치고 마을로 발걸음을 돌립니다...");
    yield return new WaitForSeconds(1.5f);

    // ⭕ [완벽 해결] 마을 씬이 이미 알고 있는 서랍 이름인 "IsReturningFromStorage"에 1을 주입합니다!
    PlayerPrefs.SetInt("IsReturningFromStorage", 1);
    PlayerPrefs.Save();

    SceneManager.LoadScene("게임초반에서마을까지"); 
}


        else if (isLeftButtonClicked)
        {
            uiManager.SetTextInstant("마음을 다잡고 모험을 계속 이어 나갑니다.");
            yield return new WaitForSeconds(1.5f);
            
            // 💡 [★ 중요] 유저가 계속 모험을 하기로 선택했으므로, 
            // 메인 루프 전진 대기 단계(`while(true)`)로 돌아가기 직전에 종합 버튼을 다시 자연스럽게 켜줍니다.
            if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(true);
        }
    }


    // 컬러 이름을 한국어로 변환해주는 편리한 함수
    private string GetColorNameKorean(ColorType type)
    {
        switch (type)
        {
            case ColorType.Red:    return "적색";
            case ColorType.Yellow: return "황색";
            case ColorType.Green:  return "녹색";
            case ColorType.Blue:   return "청색";
            case ColorType.Purple: return "자색";
            default:               return "알 수 없는 색";
        }
    }

    // [★ 버그 수정 완료!] NPC 직업명을 한국어로 변환해주는 편리한 함수
    private string GetNpcNameKorean(NpcType type)
    {
        switch (type)
        {
            case NpcType.Chief:    return "마을 촌장";
            case NpcType.Merchant: return "방랑 상인";
            case NpcType.Healer:   return "약초 치료사";
            case NpcType.Warrior:  return "낙향한 전사";
            case NpcType.archaeologist:  return "고고 학자";
            default:               return "신비한 나그네";
        }
    }
} // 스크립트 맨 마지막 중괄호 (클래스 닫기)

