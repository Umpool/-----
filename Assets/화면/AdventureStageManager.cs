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
                // 주사위 시드값을 현실 시간(유니크한 값) 기반으로 초기화하여 매번 다른 첫 숫자가 나오게 만듭니다.
        Random.InitState((int)System.DateTime.Now.Ticks);
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

        // ----------------------------------------------------------------------
        // 1. [공통 대사 단계] 이벤트에 적혀있는 기본 스토리 대사들을 출력합니다.
        // ----------------------------------------------------------------------
        for (int i = 0; i < data.eventDialogues.Length; i++)
        {
            uiManager.SetTextTyping(data.eventDialogues[i]);

            // 💡 [★ 타이밍 자동화 계산기 가동!]
            // 대사 전체 글자 수와 인스펙터의 타자 속도를 곱해서 "정확히 타자가 다 쳐지는 시간"을 계산합니다.
            float textDisplayTime = data.eventDialogues[i].Length * uiManager.dialogueSpeed;
            
            // 타자가 다 다다닥 찍히는 동안 정확하게 기다린 뒤, 글을 읽을 수 있게 1.5초만 살짝 숨을 고릅니다.
            yield return new WaitForSeconds(textDisplayTime + 1.5f);
        }

        // ----------------------------------------------------------------------
        // 2. [분기 선택 단계] 기본 대사가 모두 끝나면 좌측/우측 버튼이 즉시 등장합니다!
        // ----------------------------------------------------------------------
        switch (data.eventType)
        {
            case EventType.NothingFound:
                // 아무것도 찾지 못함: 안내문 즉시 출력 후 전진 루프로 자연스럽게 이행
                uiManager.SetTextInstant(data.nextActionPrompt);
                float nothingTime = data.nextActionPrompt.Length * uiManager.dialogueSpeed;
                yield return new WaitForSeconds(nothingTime + 1.5f);
                break;

            case EventType.RewardItem:
            case EventType.MeetMonster:
            case EventType.MeetPerson:
                // 안내문을 띄우고 조사/진입 선택지(좌측=확인, 우측=취소)를 즉시 활성화합니다.
                uiManager.SetTextInstant(data.nextActionPrompt);
                
                if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);
                if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(true);
                if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(true);

                isLeftButtonClicked = false;
                isRightButtonClicked = false;
                while (!isLeftButtonClicked && !isRightButtonClicked) yield return null;

                if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
                if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

                // 유저가 선택한 결과에 따른 연출 진행
                if (isLeftButtonClicked)
                {
                    // [상자 조사 연출] 네가 기획한 2초 고속 순환 온점 연출 발동!
                    if (data.eventType == EventType.RewardItem)
                    {
                        uiManager.SetTextInstant("아이템을 확인하는중."); yield return new WaitForSeconds(0.33f);
                        uiManager.SetTextInstant("아이템을 확인하는중.."); yield return new WaitForSeconds(0.33f);
                        uiManager.SetTextInstant("아이템을 확인하는중..."); yield return new WaitForSeconds(0.34f);
                        uiManager.SetTextInstant("아이템을 확인하는중."); yield return new WaitForSeconds(0.33f);
                        uiManager.SetTextInstant("아이템을 확인하는중.."); yield return new WaitForSeconds(0.33f);
                        uiManager.SetTextInstant("아이템을 확인하는중..."); yield return new WaitForSeconds(0.34f);

                        string rewardResultText = "성공적으로 조사를 마쳤습니다!\n";
                        if (data.giveGold)
                        {
                            int rewardGold = Random.Range(1, 21);
                            rewardResultText += "획득 재화: +" + rewardGold + " 골드\n";
                            if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddGold(rewardGold);
                        }
                        if (data.rewardColor != ColorType.None)
                        {
                            string colorEngName = data.rewardColor.ToString();
                            string colorKorName = GetColorNameKorean(data.rewardColor);
                            rewardResultText += "특수 아이템 획득: " + colorKorName + " 컬러를 얻었습니다!";
                            if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddColor(colorEngName, 1);
                        }

                        // 결과 텍스트를 화면에 즉시 쏩니다!
                        uiManager.SetTextInstant(rewardResultText);
                    }
                    // [몬스터 조우 연출] 네가 살려두라고 한 고마운 2초 워프 대기 시간 유지!
                    else if (data.eventType == EventType.MeetMonster)
                    {
                        uiManager.SetTextInstant("전투에 돌입합니다! 3매치 퍼즐 화면으로 이동 중...");
                        yield return new WaitForSeconds(2.0f);
                        SceneManager.LoadScene("PuzzleBattleScene");
                        yield break;
                    }
                    // [사람 조우 연출] 인스펙터 대사 화면 즉시 출력 및 진짜 컬러 적립
                    else if (data.eventType == EventType.MeetPerson)
                    {
                        uiManager.SetTextInstant(data.nextActionPrompt);

                        ColorType targetColor = ColorType.None;
                        switch (data.npcType)
                        {
                            case NpcType.Warrior:        targetColor = ColorType.Red; break;    
                            case NpcType.Chief:          targetColor = ColorType.Green; break;  
                            case NpcType.Healer:         targetColor = ColorType.Yellow; break; 
                            case NpcType.Merchant:       targetColor = ColorType.Blue; break;   
                            case NpcType.archaeologist:  targetColor = ColorType.Purple; break; 
                        }

                        if (targetColor != ColorType.None && CurrencyManager.Instance != null)
                        {
                            CurrencyManager.Instance.AddColor(targetColor.ToString(), 1);
                        }
                    }

                    // 💡 [★ 핵심 변경 지점] 보상 결과를 화면에 띄우자마자 
                    // 종합 버튼을 숨긴 채 컴퓨터의 강제 시간 지연 없이 즉시 이 조건문을 탈출합니다!
                    if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);
                }
                else if (isRightButtonClicked)
                {
                    // 🏃 [★ 타이핑 효과 추가] 몬스터 회피 혹은 조사 취소 시 대사
                    string escapeText = "위험 요소나 번거로운 상황을 무시하고 조용히 발걸음을 옮깁니다.";
                    uiManager.SetTextTyping(escapeText);

                    // 글자 길이에 맞춰 타이핑이 다 끝날 때까지 정확히 대기합니다.
                    float escapeTime = escapeText.Length * uiManager.dialogueSpeed;
                    yield return new WaitForSeconds(escapeTime + 1.2f);
                }
                break;
        }

        // ----------------------------------------------------------------------
        // 3. [최종 종착지 결정 단계] 이벤트가 마무리되고 버튼이 켜지는 구간
        // ----------------------------------------------------------------------
        // 💬 [★ 타이핑 효과 추가] 이벤트 마무리 안내 대사
        string endChoiceText = "이벤트가 마무리되었습니다.\n계속 전진하시겠습니까, 아니면 마을로 돌아가시겠습니까?";
        uiManager.SetTextTyping(endChoiceText);

        // 글자가 다 다다닥 찍히는 타이밍을 계산해서 기다립니다.
        float endChoiceTime = endChoiceText.Length * uiManager.dialogueSpeed;
        yield return new WaitForSeconds(endChoiceTime);

        // 💡 타자가 완전히 끝나는 그 0.001초의 순간에 좌측/우측 버튼을 즉시 활성화합니다!
        if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);
        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(true);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(true);

        isLeftButtonClicked = false;
        isRightButtonClicked = false;
        while (!isLeftButtonClicked && !isRightButtonClicked) yield return null;

        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

        if (isRightButtonClicked)
        {
            // 🏡 [★ 타이핑 효과 추가] 마을 귀환 시작 대사
            string townReturnText = "모험을 마치고 마을로 발걸음을 돌립니다...";
            uiManager.SetTextTyping(townReturnText);

            float returnTime = townReturnText.Length * uiManager.dialogueSpeed;
            yield return new WaitForSeconds(returnTime + 0.8f);

            SceneManager.LoadScene("게임초반에서마을까지"); 
        }
        else if (isLeftButtonClicked)
        {
            // 🏃 [★ 타이핑 효과 추가] 모험 지속 선택 대사
            string continueText = "마음을 다잡고 모험을 계속 이어 나갑니다.";
            uiManager.SetTextTyping(continueText);

            float continueTime = continueText.Length * uiManager.dialogueSpeed;
            yield return new WaitForSeconds(continueTime + 0.8f);

            if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(true);
        }
    }



    // 컬러 이름을 한국어로 변환해주는 편리한 함수
    private string GetColorNameKorean(ColorType type)
    {
        switch (type)
        {
            case ColorType.Red:    return "레드";
            case ColorType.Yellow: return "옐로";
            case ColorType.Green:  return "그린";
            case ColorType.Blue:   return "블루";
            case ColorType.Purple: return "퍼플";
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

