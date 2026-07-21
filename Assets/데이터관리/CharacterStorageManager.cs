using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStorageManager : MonoBehaviour
{
    [Header("독립 파티창 슬롯 UI (0번이 맨 왼쪽 리더)")]
    [SerializeField] private List<Image> uniquePartySlots = new List<Image>();
    [SerializeField] private Sprite storageEmptySlotSprite;

    [Header("독립 격자 배치 UI (Grid Layout Group 부모)")]
    [SerializeField] private Transform storageGridParentGroup;
    [SerializeField] private GameObject storageCharacterSlotPrefab; // 만능 붕어빵 틀 프리팹

    [Header("독립 시너지 및 하단 정보 UI")]
    [SerializeField] private TextMeshProUGUI storageSynergyText;
    [SerializeField] private TextMeshProUGUI storageCountText;
    [SerializeField] private int storageMaxCount = 10;

    private List<string> storageTempPartyList = new List<string>();

    private void OnEnable()
    {
        if (PartyManager.Instance != null)
        {
            storageTempPartyList = new List<string>(PartyManager.Instance.currentPartyList);
        }

        RefreshUniqueStorageUI();
        RefreshUniquePartyUI();
    }

    /// <summary>
    /// [최종 혁신 엔진] 프로젝트 '캐릭터' 폴더에 들어있는 진짜 실물 데이터 시트(.asset)를 직접 찾아와 프리팹에 통째로 수동 도킹시킵니다!
    /// </summary>
    public void RefreshUniqueStorageUI()
    {
        foreach (Transform child in storageGridParentGroup)
        {
            Destroy(child.gameObject);
        }

        // 선택창에서 유저가 콕 집어 고른 나만의 고유 캐릭터 ID를 영리하게 꺼내옵니다.
        string currentMainHeroID = PlayerPrefs.GetString("SelectedCharacterID", "01");

        List<string> testOwnedIDs = new List<string> { currentMainHeroID };

        if (storageCountText != null)
        {
            storageCountText.text = $"{testOwnedIDs.Count} / {storageMaxCount}";
        }

        foreach (string charID in testOwnedIDs)
        {
            // 1. 비어있는 만능 프리팹 액자 껍데기를 격자판에 하나 찍어냅니다.
            GameObject newSlot = Instantiate(storageCharacterSlotPrefab, storageGridParentGroup);
            CharacterComponent comp = newSlot.GetComponent<CharacterComponent>();

            if (comp != null)
            {
                // 2. 🎯 [데이터 다이렉트 도킹 핵심]
                // 질문자님의 에디터 'Assets/캐릭터' 폴더 안에 들어있는 진짜 실물 문서 파일 이름("메인_01전사")을 정밀 저격해 불러옵니다!
                // (※ 만약 ID가 "01" 이라면 'Assets/캐릭터/메인_01전사' 라는 진짜 원본 문서를 찾아옵니다.)
                string assetPath = $"캐릭터/메인_{charID}전사";
                CharacterData originalData = Resources.Load<CharacterData>(assetPath);

                // 만약 Resources 내부에 없다면 일반 에셋 경로 폴더를 싹 뒤져서 강제 로드 연동시킵니다.
                if (originalData == null)
                {
                    CharacterData[] allDataSheets = Resources.LoadAll<CharacterData>("캐릭터");
                    foreach (var sheet in allDataSheets)
                    {
                        if (sheet != null && sheet.name.Contains(charID))
                        {
                            originalData = sheet;
                            break;
                        }
                    }
                }

                // 3. 찾은 진짜 실물 전사 기획서(.asset)를 비어있던 프리팹 컴포넌트 내부(myData)에 코드가 강제로 직접 꼽아버립니다!
                if (originalData != null)
                {
                    comp.myData = Instantiate(originalData); // 원본 데이터 훼손 방지를 위해 복사본 주입
                    comp.myData.characterID = charID;

                    // 4. 전사 기획서 문서 인스펙터창에 심어두신 캐릭터 고유 일러스트 그림(characterSprite)을 
                    // 프리팹 슬롯 이미지 칸에 1:1로 직접 복사 각인시켜 유저 눈에 선명하게 보여줍니다!
                    Image cardImage = newSlot.GetComponent<Image>();
                    if (cardImage != null && comp.myData.characterSprite != null)
                    {
                        cardImage.sprite = comp.myData.characterSprite;
                        cardImage.color = Color.white; // 하얀색 네모 상자 버그 완전 소멸!
                    }
                }
            }

            Button btn = newSlot.GetComponent<Button>();
            if (btn == null) btn = newSlot.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnClickUniqueStorageCharacter(charID));
        }
    }

    private void OnClickUniqueStorageCharacter(string characterID)
    {
        if (storageTempPartyList.Contains(characterID))
        {
            storageTempPartyList.Remove(characterID);
            Debug.Log($"[Storage] 파티에서 메인 캐릭터 제외 완료: {characterID}");
        }
        else
        {
            if (storageTempPartyList.Count >= uniquePartySlots.Count)
            {
                Debug.LogWarning("[Storage] 파티원이 이미 가득 찼습니다!");
                return;
            }

            storageTempPartyList.Add(characterID);
            Debug.Log($"[Storage] 파티 맨 왼쪽 리더 자리 자동 영입 성공: {characterID}");
        }

        RefreshUniquePartyUI();
    }

    public void RefreshUniquePartyUI()
    {
        for (int i = 0; i < uniquePartySlots.Count; i++)
        {
            if (uniquePartySlots[i] != null) uniquePartySlots[i].sprite = storageEmptySlotSprite;
        }

        // 상단 파티 슬롯 역시 하단에 복사 생성된 진짜 영웅 데이터(comp.myData)의 그림을 쓱 빼내어 똑같이 투영시킵니다!
        CharacterComponent[] spawnedCards = storageGridParentGroup.GetComponentsInChildren<CharacterComponent>();

        for (int i = 0; i < storageTempPartyList.Count; i++)
        {
            if (i >= uniquePartySlots.Count) break;

            string partyCharID = storageTempPartyList[i];

            foreach (CharacterComponent card in spawnedCards)
            {
                if (card != null && card.myData != null && card.myData.characterID == partyCharID)
                {
                    if (uniquePartySlots[i] != null && card.myData.characterSprite != null)
                    {
                        uniquePartySlots[i].sprite = card.myData.characterSprite;
                        uniquePartySlots[i].color = Color.white;
                    }
                    break;
                }
            }
        }

        UpdateUniqueSynergyText();
    }

    private void UpdateUniqueSynergyText()
    {
        if (storageSynergyText == null) return;

        if (storageTempPartyList.Count == 0)
        {
            storageSynergyText.text = "현재 배치된 아군이 없습니다.\n하단 보관함에서 내 메인 캐릭터를\n터치해 파티를 구성해 보세요.";
            return;
        }

        string resultText = "";
        string leaderID = storageTempPartyList.Count > 0 ? storageTempPartyList[0] : ""; resultText += "<color=yellow>[👑 현재 배치된 메인 리더 효과]</color>\n";

        if (leaderID == "01" || leaderID == "001") resultText += "- [촌장의 위엄] 맨 왼쪽 리더의 격려로 아군 전체 공격력 +10%\n";
        else if (leaderID == "02" || leaderID == "002") resultText += "- [하마의 뚝심] 맨 왼쪽 리더의 존재감으로 아군 전체 체력 +15%\n";
        else if (leaderID == "03" || leaderID == "003") resultText += "- [악어의 투지] 맨 왼쪽 리더의 포효로 치명타 확률 +12%\n";
        else if (leaderID == "04" || leaderID == "004") resultText += "- [너구리의 지혜] 맨 왼쪽 리더의 영리함으로 코인 골드 획득량 +20%\n";
        else resultText += $"- 일반 리더 보너스: 아군 전체 이동 속도 +5%\n";

        storageSynergyText.text = resultText;
    }

    public void OnClickUniquePartySlot(int slotIndex)
    {
        if (slotIndex < storageTempPartyList.Count)
        {
            string removedCharID = storageTempPartyList[slotIndex];
            storageTempPartyList.RemoveAt(slotIndex);
            Debug.Log($"[Storage] 상단 파티장 터치 -> 파티에서 제외 완료: {removedCharID}");
            RefreshUniquePartyUI();
        }
    }

    public void OnClickReturnToTown()
    {
        Debug.Log("[Storage] 변경 사항 세이브 및 마을 복귀 가동");

        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.currentPartyList.Clear();
            foreach (string charID in storageTempPartyList)
            {
                PartyManager.Instance.currentPartyList.Add(charID);
            }

            string combinedParty = string.Join(",", PartyManager.Instance.currentPartyList);
            PlayerPrefs.SetString("SelectedCharacterID", storageTempPartyList.Count > 0 ? storageTempPartyList[0] : "");
            PlayerPrefs.SetString("CurrentPartyListString", combinedParty);
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetInt("IsReturningFromStorage", 1);
        PlayerPrefs.Save();

        CancelInvoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene("게임초반에서마을까지");
    }
}