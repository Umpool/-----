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

    private int selectedPathType = 0; // 0: 최초 기본, 1: 재화 폭등, 2: 동료 폭등, 3: 재료 폭등
    private bool isUpperButtonClicked = false; // 상단 버튼 클릭 신호 감지용 변수
    private int rewardPathCount = 0; // 재화의 길 누적 선택 횟수
    private int currentMaxRewardAmount = 1; // 현재 얻을 수 있는 컬러의 최대 개수 (최대 5)



    private bool isLeftButtonClicked = false;
    private bool isRightButtonClicked = false;

    void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        // 1. 좌측 버튼 클릭 센서 (재화의 길)
        if (uiManager.leftButton != null)
        {
            uiManager.leftButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => isLeftButtonClicked = true);
        }

        // 2. 상단 버튼 클릭 센서 (topButton / 동료의 길)
        if (uiManager.topButton != null)
        {
            uiManager.topButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => isUpperButtonClicked = true);
        }

        // 3. 우측 버튼 클릭 센서 (재료의 길)
        if (uiManager.rightButton != null)
        {
            uiManager.rightButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => isRightButtonClicked = true);
        }

        StartCoroutine(AdventureLoop());
    }



    private IEnumerator AdventureLoop()
    {
        string startPathText = "앞에 세 갈래 길의 표지판이 보입니다.\n어느 길로 모험을 시작하시겠습니까?";
        uiManager.SetTextTyping(startPathText);
        uiManager.SetButtonTexts("재화의 길", "동료의 길", "재료의 길");

        // 1. 버튼 오브젝트를 화면에 켭니다.
        // 💡 [안전장치 추가] 대사가 나오는 동안 유저가 투명 버튼을 누르지 못하도록 일시 잠금 처리합니다.
        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(true);
        if (uiManager.topButton != null) uiManager.topButton.gameObject.SetActive(true);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(true);

        // 💡 [★ 긴급 수정: 투명도 즉시 제로 세팅] 
        // 버튼이 화면에 켜지자마자 유저 눈에 떡하니 보이지 않도록 0.001초 만에 완전히 투명하게 숨겨버립니다.
        UnityEngine.UI.Image initLeftImg = null; UnityEngine.UI.Image initTopImg = null; UnityEngine.UI.Image initRightImg = null;
        TMPro.TMP_Text initLeftTxt = null; TMPro.TMP_Text initTopTxt = null; TMPro.TMP_Text initRightTxt = null;

        if (uiManager.leftButton != null) { initLeftImg = uiManager.leftButton.GetComponent<UnityEngine.UI.Image>(); initLeftTxt = uiManager.leftButton.GetComponentInChildren<TMPro.TMP_Text>(); }
        if (uiManager.topButton != null) { initTopImg = uiManager.topButton.GetComponent<UnityEngine.UI.Image>(); initTopTxt = uiManager.topButton.GetComponentInChildren<TMPro.TMP_Text>(); }
        if (uiManager.rightButton != null) { initRightImg = uiManager.rightButton.GetComponent<UnityEngine.UI.Image>(); initRightTxt = uiManager.rightButton.GetComponentInChildren<TMPro.TMP_Text>(); }

        if (initLeftImg != null) { Color c = initLeftImg.color; c.a = 0f; initLeftImg.color = c; }
        if (initTopImg != null) { Color c = initTopImg.color; c.a = 0f; initTopImg.color = c; }
        if (initRightImg != null) { Color c = initRightImg.color; c.a = 0f; initRightImg.color = c; }
        if (initLeftTxt != null) { Color c = initLeftTxt.color; c.a = 0f; initLeftTxt.color = c; }
        if (initTopTxt != null) { Color c = initTopTxt.color; c.a = 0f; initTopTxt.color = c; }
        if (initRightTxt != null) { Color c = initRightTxt.color; c.a = 0f; initRightTxt.color = c; }

        // 2. 대사 타이핑이 완벽하게 끝날 때까지 컴퓨터가 가만히 대기합니다. (이제 버튼은 투명하게 숨어있습니다!)
        float startTextTime = startPathText.Length * uiManager.dialogueSpeed;
        yield return new WaitForSeconds(startTextTime);

        // 3. ✨ 글자가 딱 종료되자마자 숨어있던 버튼들이 왼쪽부터 차례대로 스르르 페이드인 됩니다!
        yield return StartCoroutine(FadeInButtonsSequentially(0.4f));

        isLeftButtonClicked = false;
        isUpperButtonClicked = false;
        isRightButtonClicked = false;



        while (!isLeftButtonClicked && !isUpperButtonClicked && !isRightButtonClicked)
        {
            yield return null;
        }

        // 유저의 버튼 터치 결과에 따라 확률 제어장치 시스템 번호를 안전하게 기록합니다.
        if (isLeftButtonClicked) selectedPathType = 1;
        else if (isUpperButtonClicked) selectedPathType = 2;
        else if (isRightButtonClicked) selectedPathType = 3;

        // 갈림길 선택이 완료되었으므로 화면 안의 모든 선택지 버튼들을 깨끗하게 꺼줍니다.
        isLeftButtonClicked = false;
        isUpperButtonClicked = false;
        isRightButtonClicked = false;

        if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
        if (uiManager.topButton != null) uiManager.topButton.gameObject.SetActive(false);
        if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

        // -------------------------------------------------------------------------
        // 여기서부터 기존의 while(true) 모험 루프와 주사위 확률 연산이 자연스럽게 이어집니다.
        // -------------------------------------------------------------------------
        while (true)
        {

            int dice = Random.Range(1, 21);
            EventType chosenType;

            if (selectedPathType == 1)
            {
                rewardPathCount++;
                currentMaxRewardAmount = 1 + (rewardPathCount / 10);
                if (currentMaxRewardAmount > 5) currentMaxRewardAmount = 5;

                if (dice <= 14) chosenType = EventType.RewardItem;
                else if (dice <= 17) chosenType = EventType.MeetMonster;
                else chosenType = EventType.NothingFound;
            }
            else if (selectedPathType == 2)
            {
                if (dice <= 14) chosenType = EventType.MeetPerson;
                else if (dice <= 16) chosenType = EventType.MeetMonster;
                else if (dice <= 18) chosenType = EventType.NothingFound;
                else chosenType = EventType.RewardItem;
            }
            else if (selectedPathType == 3)
            {
                if (dice <= 10) chosenType = EventType.MeetMonster;
                else if (dice <= 16) chosenType = EventType.NothingFound;
                else if (dice <= 18) chosenType = EventType.RewardItem;
                else chosenType = EventType.MeetPerson;
            }
            else
            {
                if (dice <= 4) chosenType = EventType.NothingFound;
                else if (dice <= 7) chosenType = EventType.MeetMonster;
                else if (dice <= 10) chosenType = EventType.RewardItem;
                else chosenType = EventType.MeetPerson;
            }



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
            // 🛑 AdventureStageManager.cs 93~108줄 수정
            // 2. 방향 버튼 종료
            uiManager.SetDirectionButtonsActive(false);

            // [안전한 자동 전진] 안내와 터치 대기 단계를 건너뛰고 시스템 서랍을 깨끗이 청소합니다.

            if (uiManager.nextDialogueButton != null)
            {
                uiManager.nextDialogueButton.gameObject.SetActive(false);
                uiManager.nextDialogueButton.interactable = false;
            }
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

            // 🎯 [★ 완벽한 타이밍 체킹 구현]
            // UI 매니저가 한 글자씩 타자 쳐서 글자를 완전히 다 완성할 때까지 코드를 일시정지하고 기다립니다.
            while (!uiManager.isTypingFinished) yield return null;

            // 글자가 완벽하게 한 화면 가득 다 쳐졌으므로, 유저가 눈으로 읽을 수 있게 딱 1.2초만 숨을 고르고 다음 대사로 넘어갑니다!
            yield return new WaitForSeconds(1.2f);
        }


        // ----------------------------------------------------------------------
        // 2. [분기 선택 단계] 기본 대사가 모두 끝나면 좌측/우측 버튼이 즉시 등장합니다!
        // ----------------------------------------------------------------------
        switch (data.eventType)
        {
            case EventType.NothingFound:
                uiManager.SetTextTyping(data.nextActionPrompt);
                float nothingTime = data.nextActionPrompt.Length * uiManager.dialogueSpeed;
                yield return new WaitForSeconds(nothingTime + 1.5f);
                break;


            case EventType.RewardItem:
            case EventType.MeetMonster:
            case EventType.MeetPerson:
                // 💡 [★ 수정] 선택지 안내문도 한 번에 띄우지 않고 타자 효과로 다다닥 출력합니다!
                uiManager.SetTextTyping(data.nextActionPrompt);
                string finalLeftText = "선택";
                string finalRightText = "제외";

                if (data.eventType == EventType.RewardItem)
                {
                    finalLeftText = "확인한다";
                    finalRightText = "지나간다";
                }
                else if (data.eventType == EventType.MeetMonster)
                {
                    finalLeftText = "블록을 전개한다.";
                    finalRightText = "도망친다.";
                }
                else if (data.eventType == EventType.MeetPerson)
                {
                    if (data.npcType == NpcType.Warrior)
                    {
                        finalLeftText = "...?";
                        finalRightText = "지나간다.";
                    }
                    else if (data.npcType == NpcType.Chief)
                    {
                        finalLeftText = "받는다.";
                        finalRightText = "사양한다.";
                    }
                    else if (data.npcType == NpcType.Healer)
                    {
                        finalLeftText = "받는다.";
                        finalRightText = "사양한다.";
                    }
                    else if (data.npcType == NpcType.Merchant)
                    {
                        finalLeftText = "확인한다";
                        finalRightText = "안본다.";
                    }
                    else if (data.npcType == NpcType.archaeologist)
                    {
                        finalLeftText = "같이 찾는다.";
                        finalRightText = "응원한다.";
                    }
                }

                uiManager.SetButtonTexts(finalLeftText, "", finalRightText);


                // 화면 전체를 덮는 종합 버튼은 방해되지 않게 먼저 꺼둡니다.
                if (uiManager.nextDialogueButton != null) uiManager.nextDialogueButton.gameObject.SetActive(false);

                // 좌측/우측 버튼도 글자가 다 타이핑되는 동안에는 유저가 누르지 못하게 잠시 숨겨둡니다.
                if (uiManager.leftButton != null) uiManager.leftButton.gameObject.SetActive(false);
                if (uiManager.rightButton != null) uiManager.rightButton.gameObject.SetActive(false);

                // 💡 [★ 타이밍 계산] 안내문의 글자 수에 맞춰 타이핑이 완벽히 끝날 때까지 정확하게 대기합니다!
                float promptDisplayTime = data.nextActionPrompt.Length * uiManager.dialogueSpeed;
                yield return new WaitForSeconds(promptDisplayTime);

                // 🎯 [★ 칼 타이밍!] 글자가 마지막 글자까지 다다닥 찍히는 순간 0초의 망설임도 없이 좌/우 버튼을 뿅 켭니다!
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
                        if (true)
                        {
                            Dictionary<ColorType, string> hexMap = new Dictionary<ColorType, string>
                    {
                        { ColorType.Red, "#FF3333" },
                        { ColorType.Green, "#33FF33" },
                        { ColorType.Yellow, "#FFFF33" },
                        { ColorType.Blue, "#3333FF" },
                        { ColorType.Purple, "#A64DFF" }
                    };

                            ColorType randomColor = (ColorType)Random.Range(1, 6);
                            string colorEngName = randomColor.ToString();
                            string colorKorName = GetColorNameKorean(randomColor);

                            string targetHex = "#FFFFFF";
                            if (hexMap.ContainsKey(randomColor)) targetHex = hexMap[randomColor];

                            int finalGiveAmount = Random.Range(1, currentMaxRewardAmount + 1);

                            rewardResultText += "특수 아이템 획득: <color=" + targetHex + ">" + colorKorName + "</color> 컬러를 " + finalGiveAmount + "개 얻었습니다!";

                            if (CurrencyManager.Instance != null)
                            {
                                CurrencyManager.Instance.AddColor(colorEngName, finalGiveAmount);
                            }
                        }




                        // 💡 [★ 타이핑 효과 연동] 보상 텍스트가 다다닥 찍히도록 변경!
                        uiManager.SetTextTyping(rewardResultText);

                        // 글자 길이에 맞춰 타이핑이 완벽하게 끝날 때까지 컴퓨터가 자동으로 계산하여 대기합니다.
                        float rewardTime = rewardResultText.Length * uiManager.dialogueSpeed;
                        yield return new WaitForSeconds(rewardTime);
                    }
                    // ⚔️ [2. 몬스터 조우 연출 타이핑화]
                    else if (data.eventType == EventType.MeetMonster)
                    {
                        string monsterWarnText = "전투에 돌입합니다! 블록 전개 중...";
                        uiManager.SetTextTyping(monsterWarnText);

                        float monsterTime = monsterWarnText.Length * uiManager.dialogueSpeed;
                        yield return new WaitForSeconds(monsterTime + 1.2f); // 안내 글자 다 읽고 안전하게 워프!

                        SceneManager.LoadScene("PuzzleBattleScene");
                        yield break;
                    }
                    // [사람 조우 연출] 인스펙터 대사 화면 즉시 출력 및 진짜 컬러 적립
                    else if (data.eventType == EventType.MeetPerson)
                    {
                        Dictionary<ColorType, (string name, string hex)> npcColorMap = new Dictionary<ColorType, (string, string)>
            {
                { ColorType.Red, ("레드", "#FF3333") },
                { ColorType.Green, ("그린", "#33FF33") },
                { ColorType.Yellow, ("옐로", "#FFFF33") },
                { ColorType.Blue, ("블루", "#3333FF") },
                { ColorType.Purple, ("퍼플", "#A64DFF") }
            };

                        ColorType currentTargetColor = ColorType.None;
                        if (data.npcType == NpcType.Warrior) currentTargetColor = ColorType.Red;
                        else if (data.npcType == NpcType.Chief) currentTargetColor = ColorType.Green;
                        else if (data.npcType == NpcType.Healer) currentTargetColor = ColorType.Yellow;
                        else if (data.npcType == NpcType.Merchant) currentTargetColor = ColorType.Blue;
                        else if (data.npcType == NpcType.archaeologist) currentTargetColor = ColorType.Purple;

                        string finalColorName = "알 수 없는";
                        string finalColorHex = "#FFFFFF";

                        if (npcColorMap.ContainsKey(currentTargetColor))
                        {
                            finalColorName = npcColorMap[currentTargetColor].name;
                            finalColorHex = npcColorMap[currentTargetColor].hex;
                        }

                        if (currentTargetColor != ColorType.None && CurrencyManager.Instance != null)
                        {
                            CurrencyManager.Instance.AddColor(currentTargetColor.ToString(), 1);
                        }

                        string rewardMessage = "<color=" + finalColorHex + ">" + finalColorName + "</color> 컬러 1개를 얻었습니다!";
                        uiManager.SetTextTyping(rewardMessage);

                        float npcTime = rewardMessage.Length * uiManager.dialogueSpeed;
                        yield return new WaitForSeconds(npcTime);

                    }




                    // 💡 [★ 타이밍 정밀 개조] 보상이나 NPC 대사 글자가 다 쳐진 후 1초 더 보여주기
                    if (data.eventType == EventType.RewardItem || data.eventType == EventType.MeetPerson)
                    {
                        // 보물상자일 때는 uiManager에 담긴 현재 화면 글자를 그대로 쓰고, 사람일 때는 인스펙터 대사를 가져옵니다.
                        string currentText = (data.eventType == EventType.RewardItem) ? uiManager.logText.text : data.nextActionPrompt;

                        // 타자가 다 끝날 때까지 컴퓨터가 자동으로 연산하여 안전하게 대기합니다.
                        float displayFinishTime = currentText.Length * uiManager.dialogueSpeed;
                        yield return new WaitForSeconds(displayFinishTime);

                        // ⏳ [기획안 완벽 반영] 타이핑이 완전히 끝난 직후부터 정확히 '1.0초'간 더 화면을 멈춰 세웁니다!
                        yield return new WaitForSeconds(1.0f);
                    }

                    if (uiManager.nextDialogueButton != null)
                        uiManager.nextDialogueButton.gameObject.SetActive(false);

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
        if (data.eventType == EventType.NothingFound)
        {
            if (uiManager.nextDialogueButton != null)
            {
                uiManager.nextDialogueButton.gameObject.SetActive(true);
            }
            yield break;
        }

        // ----------------------------------------------------------------------
        // 3. [최종 종착지 결정 단계] 이벤트가 마무리되고 버튼이 켜지는 구간
        // ----------------------------------------------------------------------
        // 💬 [★ 타이핑 효과 추가] 이벤트 마무리 안내 대사
        string endChoiceText = "계속 전진하시겠습니까, 아니면 마을로 돌아가시겠습니까?";
        uiManager.SetTextTyping(endChoiceText);
        uiManager.SetButtonTexts("전진한다.", "", "돌아간다.");


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


        // 318번째 줄 위치 (우측 버튼: 마을 귀환 선택 시)
        if (isRightButtonClicked)
        {
            // 🏡 마을 귀환 대사 타이핑 연출 시작
            string townReturnText = "모험을 마치고 마을로 발걸음을 돌립니다...";
            uiManager.SetTextTyping(townReturnText);

            float returnTime = townReturnText.Length * uiManager.dialogueSpeed;
            yield return new WaitForSeconds(returnTime + 0.8f);

            // 💡 [★ 꼬임 해결 정석 편집]
            // 복잡한 지갑 매니저 함수를 호출하지 않고, 유니티 정석 세이브 방식을 직접 때려 넣습니다.
            PlayerPrefs.SetInt("IsReturningFromStorage", 1);
            PlayerPrefs.Save(); // 쾅! 영구 저장소 서랍을 확실하게 굳혀 닫습니다.

            // 🚀 한 치의 오차도 없이 즉시 안전하게 마을 화면으로 워프!
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
            case ColorType.Red: return "레드";
            case ColorType.Yellow: return "옐로";
            case ColorType.Green: return "그린";
            case ColorType.Blue: return "블루";
            case ColorType.Purple: return "퍼플";
            default: return "알 수 없는 색";
        }
    }

    // [★ 버그 수정 완료!] NPC 직업명을 한국어로 변환해주는 편리한 함수
    private string GetNpcNameKorean(NpcType type)
    {
        switch (type)
        {
            case NpcType.Chief: return "마을 촌장";
            case NpcType.Merchant: return "방랑 상인";
            case NpcType.Healer: return "약초 치료사";
            case NpcType.Warrior: return "낙향한 전사";
            case NpcType.archaeologist: return "고고 학자";
            default: return "신비한 나그네";
        }
    }
    private IEnumerator FadeInButtonsSequentially(float durationPerButton)
    {
        UnityEngine.UI.Image leftImg = null; UnityEngine.UI.Image topImg = null; UnityEngine.UI.Image rightImg = null;
        TMPro.TMP_Text leftTxt = null; TMPro.TMP_Text topTxt = null; TMPro.TMP_Text rightTxt = null;

        if (uiManager.leftButton != null) 
        { 
            leftImg = uiManager.leftButton.GetComponent<UnityEngine.UI.Image>(); 
            leftTxt = uiManager.leftButton.GetComponentInChildren<TMPro.TMP_Text>(); 
        }
        if (uiManager.topButton != null) 
        { 
            topImg = uiManager.topButton.GetComponent<UnityEngine.UI.Image>(); 
            topTxt = uiManager.topButton.GetComponentInChildren<TMPro.TMP_Text>(); 
        }
        if (uiManager.rightButton != null) 
        { 
            rightImg = uiManager.rightButton.GetComponent<UnityEngine.UI.Image>(); 
            rightTxt = uiManager.rightButton.GetComponentInChildren<TMPro.TMP_Text>(); 
        }


        if (leftImg != null) { leftImg.raycastTarget = false; Color c = leftImg.color; c.a = 0f; leftImg.color = c; }
        if (topImg != null) { topImg.raycastTarget = false; Color c = topImg.color; c.a = 0f; topImg.color = c; }
        if (rightImg != null) { rightImg.raycastTarget = false; Color c = rightImg.color; c.a = 0f; rightImg.color = c; }
        if (leftTxt != null) leftTxt.raycastTarget = false;
        if (topTxt != null) topTxt.raycastTarget = false;
        if (rightTxt != null) rightTxt.raycastTarget = false;

        float currentTime = 0f;
        while (currentTime < durationPerButton)
        {
            currentTime += Time.deltaTime;
            float alpha = currentTime / durationPerButton;
            if (leftImg != null) { Color c = leftImg.color; c.a = alpha; leftImg.color = c; }
            if (leftTxt != null) { Color c = leftTxt.color; c.a = alpha; leftTxt.color = c; }
            yield return null;
        }
        if (leftBtn != null) leftBtn.interactable = true; // 좌측 버튼 확실하게 클릭 활성화

        currentTime = 0f;
        while (currentTime < durationPerButton)
        {
            currentTime += Time.deltaTime;
            float alpha = currentTime / durationPerButton;
            if (topImg != null) { Color c = topImg.color; c.a = alpha; topImg.color = c; }
            if (topTxt != null) { Color c = topTxt.color; c.a = alpha; topTxt.color = c; }
            yield return null;
        }
        if (topBtn != null) topBtn.interactable = true; // 상단 버튼 확실하게 클릭 활성화

        currentTime = 0f;
        while (currentTime < durationPerButton)
        {
            currentTime += Time.deltaTime;
            float alpha = currentTime / durationPerButton;
            if (rightImg != null) { Color c = rightImg.color; c.a = alpha; rightImg.color = c; }
            if (rightTxt != null) { Color c = rightTxt.color; c.a = alpha; rightTxt.color = c; }
            yield return null;
        }
        if (rightBtn != null) rightBtn.interactable = true; // 우측 버튼 확실하게 클릭 활성화
    }
}
