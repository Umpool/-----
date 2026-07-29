using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextRPGUIManager : MonoBehaviour
{
    [Header("[RPG 출력용 UI 세팅]")]
    public TextMeshProUGUI logText;        
    public Button nextDialogueButton;      

    [Header("[대사 출력 속도 조절]")]
    [Range(0.001f, 0.5f)] 
    public float dialogueSpeed = 0.5f; // 💡 인스펙터에 슬라이더 바가 생겨나게 해주는 마법의 문장!
    public bool isTypingFinished { get; private set; } = true;


    [Header("[모험에 필요한 3개 버튼]")]
    public Button leftButton;
    public Button rightButton;
    public Button topButton;

    private Coroutine typingCoroutine;

    public void SetTextInstant(string message)
    {
        if (typingCoroutine != null) 
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (logText != null) logText.text = message;
    }

    public void SetTextTyping(string message)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        // 💡 이제 코드가 하드코딩된 숫자가 아니라, 네가 인스펙터에서 바꾼 속도를 실시간으로 가져와!
        typingCoroutine = StartCoroutine(TypeText(message, dialogueSpeed));
    }


private IEnumerator TypeText(string message, float speed)
{
    isTypingFinished = false; // 💡 타자 치기 시작했으니 보초 서기!
    logText.text = "";

    foreach (char letter in message.ToCharArray())
    {
        logText.text += letter;
        yield return new WaitForSeconds(speed);
    }

    isTypingFinished = true; // 🎯 글자가 완전히 다 찍혔다고 만천하에 알리기!
}


    public void SetDirectionButtonsActive(bool isActive)
    {
        if (leftButton != null) leftButton.gameObject.SetActive(isActive);
        if (rightButton != null) rightButton.gameObject.SetActive(isActive);
        if (topButton != null) topButton.gameObject.SetActive(isActive);
    }
}
