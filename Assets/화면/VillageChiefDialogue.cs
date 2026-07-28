using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VillageChiefDialogue : MonoBehaviour
{
    [Header("[대사 UI 컴포넌트]")]
    public GameObject dialoguePanel;      // 촌장 대사 팝업창 UI 전체
    public TextMeshProUGUI dialogueText;  // 대사가 출력될 글자창
    public Button nextDialogueButton;     // 팝업창 전체를 덮는 클릭용 버튼

    [Header("[대사 데이터]")]
    [TextArea(2, 5)]
    public string[] dialogues;            // 유니티 인스펙터에서 작성할 촌장 대사 배열
    private int currentDialogueIndex = 0; // 현재 진행 중인 대사 번호

    [Header("[이동할 모험 씬 이름]")]
    public string nextSceneName = "TextRPGScene"; // 이동할 텍스트 RPG 씬 이름

    void Start()
    {
        // 처음에는 팝업창을 숨겨두고 버튼 이벤트 연결
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (nextDialogueButton != null) nextDialogueButton.onClick.AddListener(OnClickNextDialogue);
    }

    // 촌장 NPC 버튼을 눌렀을 때 실행될 함수 (인스펙터의 촌장 버튼 OnClick에 연결)
    public void OnClickChiefButton()
    {
        if (dialogues == null || dialogues.Length == 0) return;

        currentDialogueIndex = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        ShowDialogue();
    }

    // 대사를 화면에 출력
    private void ShowDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogues[currentDialogueIndex];
        }
    }

    // 팝업창을 터치했을 때 다음 대사로 넘어가거나 씬 이동
    public void OnClickNextDialogue()
    {
        currentDialogueIndex++;

        // 아직 읽을 대사가 남아있다면 다음 대사 출력
        if (currentDialogueIndex < dialogues.Length)
        {
            ShowDialogue();
        }
        else
        {
            // 모든 대사가 끝났다면 다음 씬으로 안전하게 이동
            Debug.Log("[System] 촌장 대사 종료. 모험 씬으로 이동합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
