using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("[UI 글자창 및 버튼]")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    public Button confirmButton;       // 출발하기 버튼
    public GameObject secondEventPanel; // 마을 화면 패널

    [Header("[파티창에 배치된 실제 빈 이미지 컴포넌트]")]
    public Image partyCharacterImage; 

    [Header("[캐릭터 선택 안 했을 때의 경고 텍스트창]")]
    public TextMeshProUGUI warningText; 

    // [CharacterSelectManager.cs 파일 맨 아래, 클래스 닫히기 직전에 이 변수와 함수를 추가하세요]
    [Header("[타이틀 화면 패널 오브젝트]")]
    public GameObject titlePanel; // 유니티 에디터에서 타이틀 패널을 드래그해 넣을 칸


    // [흐름의 핵심]: 파티창에 들어간 이전 버튼이 누구였는지 기억해 두는 임시 보관소입니다.
    private GameObject previousHiddenButton = null;

    // 화면의 시작
    private void Start()
    {
        // [버그 완벽 박멸]: 처음에 오자마자 유저가 출발하기를 누를 수 있도록 잠금장치(False)를 True로 해제합니다!
        if (confirmButton != null) confirmButton.interactable = true;
        
        if (confirmButton != null) confirmButton.onClick.AddListener(OnClickConfirm);

        // 기존에 짜두셨던 캐릭터 버튼 고유 색상 염색 로직 (그대로 유지)
        CharacterComponent[] allCards = FindObjectsByType<CharacterComponent>();
        foreach (CharacterComponent card in allCards)
        {
            Image cardImage = card.GetComponent<Image>();
            if (cardImage != null && card.myData != null)
            {
                cardImage.color = new Color(card.myData.characterColor.r, card.myData.characterColor.g, card.myData.characterColor.b, 1f);
            }
        }
    }



    // 캐릭터 버튼을 클릭했을 때 글자를 띄우고, 그림판 기획 흐름을 실시간 실행하는 정답 함수입니다.
    public void OnSelectCharacter(CharacterData data, GameObject clickedButton)
    {
        // 1. 이름과 상세 능력치 정보를 우측 글자창에 실시간 출력
        if (nameText != null) nameText.text = data.characterName;
        if (infoText != null)
        {
            infoText.text = $"종족 : {data.characterRace}\n체력(HP) : {data.hp}\n공격력 : {data.attackPower}\n" +
                            $"방어력 : {data.defense}\n고유 스킬 : {data.uniqueSkillName}\n" +
                            $"스킬 설명 : {data.uniqueSkillDescription}\n시너지 소속 : {data.synergySystem}";
        }
        // 2. [이전 버튼 복귀 규칙]: 이전에 파티창에 들어와서 숨겨졌던 버튼이 있다면, 다시 제자리에 켜줍니다!
        if (previousHiddenButton != null)
        {
            previousHiddenButton.SetActive(true);
        }

        // 3. [그림판 복사 연동]: 오른쪽 파티창 빈 이미지에 클릭된 캐릭터의 원본 그림과 고유 색상을 100% 선명하게 복사합니다.
        if (partyCharacterImage != null)
        {
            partyCharacterImage.gameObject.SetActive(true);
            partyCharacterImage.sprite = data.characterSprite;
            // 유저님이 데이터 파일에 지정해 두신 고유 색상(빨강, 파랑 등)을 투명도 없이 선명하게 입혀줍니다!
            partyCharacterImage.color = new Color(data.characterColor.r, data.characterColor.g, data.characterColor.b, 1f); 
        }

        // 4. [숨김 규칙]: 유저님 기획대로, 클릭되어 파티창으로 복사된 해당 캐릭터의 왼쪽 버튼은 화면에서 안 보이게 숨깁니다.
        if (clickedButton != null)
        {
            clickedButton.SetActive(false);
            previousHiddenButton = clickedButton; // 이제 이 버튼이 '이전 숨겨진 버튼'이 되어 보관소에 들어갑니다.
        }

        // 5. 파티 매니저 데이터 주머니 갱신 및 출발하기 활성화
        if (PartyManager.Instance != null) PartyManager.Instance.SetMainCharacter(data.characterID);
        if (confirmButton != null) confirmButton.interactable = true;
    }

    // [CharacterSelectManager.cs 파일 맨 아래 OnClickConfirm 함수 내부를 요렇게 갈아끼우세요]
    private void OnClickConfirm()
    {
        if (PartyManager.Instance != null && PartyManager.Instance.currentPartyList.Count > 0)
        {
            Debug.Log("[System] 파티창에 캐릭터 확인 완료! 모험을 출발합니다.");
            
            if (warningText != null) warningText.text = "";

            if (secondEventPanel != null)
            {
                secondEventPanel.SetActive(true);

                // [원격 시동 장치 장착] 화면을 켜자마자 2차 이벤트의 0.5초 대기 타이머를 강제로 가동합니다!
                SecondEventScreenManager eventManager = secondEventPanel.GetComponent<SecondEventScreenManager>();
                if (eventManager != null)
                {
                    eventManager.StartEventWithDelay();
                }

                this.gameObject.SetActive(false);
            }

        }
        else
        {
            Debug.LogWarning("캐릭터를 선택해 주십시오.");

            // 1. [기획 완벽 연동 - 순정 색상 살리기]: <color> 태그를 완전히 빼버려서 유저님이 지정한 글자색 그대로 출력합니다!
            if (warningText != null)
            {
                warningText.text = "캐릭터를 선택해 주십시오.";
                
                // 2. [기획 완벽 연동 - 약간 빠른 소멸]: 
                // 기존에 돌아가던 타이머가 있다면 끄고, 0.8초 뒤에 글자를 지워주는 타이머 코루틴을 새롭게 가동합니다.
                StopAllCoroutines();
                StartCoroutine(ClearWarningTextAfterDelay(0.8f)); // 0.8초 설정 (더 빠르게 원하시면 1.0f로 조절 가능)
            }
        }
    }

    // [30년 차 공법]: 지정된 시간(초)이 지나면 자동으로 경고 대사를 뽀얗게 지워주는 타이머 부속품입니다.
    private System.Collections.IEnumerator ClearWarningTextAfterDelay(float delay)
    {
        // 1. 글자가 나타나고 유저가 읽을 수 있도록 약 1초 동안은 선명하게 가만히 유지합니다.
        yield return new WaitForSeconds(delay - 0.5f);

        if (warningText != null)
        {
            // 유저님이 인스펙터 창에 세팅해 둔 오리지널 고유 색상을 그대로 가져옵니다.
            Color originalColor = warningText.color;
            float fadeTime = 0.5f; // 0.5초 동안 서서히 사라지게 연출 속도를 조절합니다.
            float elapsedTime = 0f;

            // 2. [30년 차 공법]: 0.5초 동안 매 프레임마다 투명도(Alpha)를 100%에서 0%로 스르륵 줄여나갑니다.
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                
                // 고유 색상은 1%도 건드리지 않고 오직 '투명도'만 부드럽게 조절합니다!
                warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null; // 유니티 다음 프레임까지 대기
            }

            // 3. 연출이 완벽히 끝나면 완전히 청소하고 다음번 출력을 위해 투명도를 원래대로(100%) 원상복구해 둡니다.
            warningText.text = "";
            warningText.color = originalColor;
        }
    }



        // '타이틀 화면으로' 버튼을 마우스로 클릭했을 때 유저가 했던 행동을 전부 리셋하는 함수입니다.
    // [CharacterSelectManager.cs 파일 맨 아래 OnClickReturnToTitle 함수를 이 정답 조각으로 교체하세요]
    public void OnClickReturnToTitle()
    {
        Debug.Log("[System] 모든 정보를 완벽하게 포맷하고 타이틀로 돌아갑니다.");

        // 1. [데이터 리셋]: 파티 주머니를 깨끗하게 비웁니다.
        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.currentPartyList.Clear();
        }

        // 2. [파티창 리셋]: 파티창에 복사되어 떠있던 캐릭터 이미지를 숨깁니다.
        if (partyCharacterImage != null)
        {
            partyCharacterImage.gameObject.SetActive(false);
        }

        // 3. [버튼 리셋]: 화면에서 꺼져 숨어버린 버튼까지 뒤져서 찾아내도록 Inactive.Include 옵션을 달아줍니다!
        CharacterComponent[] allCards = FindObjectsByType<CharacterComponent>(FindObjectsInactive.Include);
        foreach (CharacterComponent card in allCards)
        {
            if (card != null) card.gameObject.SetActive(true);
        }

        // 4. [버그 완벽 박멸 - 보관소 리셋]: 타이틀로 도망갈 때 이전 숨김 버튼의 기록을 완전히 비워줍니다.
        previousHiddenButton = null;

        // 4. [버그 완벽 수리 - 텍스트 리셋]: 타이틀에 갔다 왔을 때 마지막 정보가 남아있지 않도록 글자창을 깨끗하게 비웁니다!
        if (nameText != null) nameText.text = "";
        if (infoText != null) infoText.text = "";

        // 6. [화면 전환]: 선택창은 당연히 완전히 끄고, 타이틀 패널을 켜줍니다!
        if (titlePanel != null) titlePanel.SetActive(true);
        
        this.gameObject.SetActive(false); // 내 자신은 꺼집니다.

        PlayerPrefs.DeleteKey("SelectedCharacterName");
        PlayerPrefs.Save();

        if (titlePanel != null) titlePanel.SetActive(true);
        this.gameObject.SetActive(false);
    }

} // CharacterSelectManager 클래스의 최종 마침표
