using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필수 포함
using DG.Tweening;


public class TopBarGoldUI : MonoBehaviour
{
    [Header("골드를 표시할 텍스트 컴포넌트")]
    [SerializeField] private TextMeshProUGUI goldText;
    [Header("Color Currency Texts")]
    [SerializeField] private TMPro.TextMeshProUGUI redText;
    [SerializeField] private TMPro.TextMeshProUGUI yellowText;
    [SerializeField] private TMPro.TextMeshProUGUI greenText;
    [SerializeField] private TMPro.TextMeshProUGUI blueText;
    [SerializeField] private TMPro.TextMeshProUGUI purpleText;
    [SerializeField] private TMPro.TextMeshProUGUI blackText;

    [Header("유저 프로필 닉네임 설정")]
    [SerializeField] private TextMeshProUGUI userNicknameText; // "USER" 글자 오브젝트를 연결할 칸



    void OnEnable()
    {
        // 골드가 변경되는 이벤트 구독 (기존 규칙 안전 보존)
        CurrencyManager.OnGoldChanged += UpdateGoldUI;
        CurrencyManager.OnColorChanged += UpdateColorUI;

        if (CurrencyManager.Instance != null)
        {
            UpdateGoldUI(CurrencyManager.Instance.CurrentGold);
            UpdateColorUI("Red", CurrencyManager.Instance.RedColor);
            UpdateColorUI("Yellow", CurrencyManager.Instance.YellowColor);
            UpdateColorUI("Green", CurrencyManager.Instance.GreenColor);
            UpdateColorUI("Blue", CurrencyManager.Instance.BlueColor);
            UpdateColorUI("Purple", CurrencyManager.Instance.PurpleColor);
            UpdateColorUI("Black", CurrencyManager.Instance.BlackColor);
        }




        // [유저 닉네임 실시간 인지 및 UI 연동 핵심]
        if (PlayerPrefs.HasKey("UserNickname"))
        {
            string savedNickname = PlayerPrefs.GetString("UserNickname");

            if (userNicknameText != null)
            {
                userNicknameText.text = savedNickname; // "USER" 글자를 유저가 적은 진짜 닉네임으로 교체합니다.
                Debug.Log($"[TopBar] 유저 고유 닉네임 인지 및 UI 연동 완료: {savedNickname}");
            }
        }
        else
        {
            if (userNicknameText != null)
            {
                userNicknameText.text = "여행자"; // 만약 데이터가 없을 때를 대비한 안전 백업 이름
            }
        }
    }


    void OnDisable()
    {
        // 메모리 누수 방지를 위한 구독 해제
        CurrencyManager.OnGoldChanged -= UpdateGoldUI;
        CurrencyManager.OnColorChanged -= UpdateColorUI;

    }

    void Start()
    {
        // 게임 시작 시 현재 가지고 있는 골드로 텍스트 초기화
        if (CurrencyManager.Instance != null)
        {
            UpdateGoldUI(CurrencyManager.Instance.CurrentGold);
            UpdateColorUI("Red", CurrencyManager.Instance.RedColor);
            UpdateColorUI("Blue", CurrencyManager.Instance.BlueColor);
            UpdateColorUI("Yellow", CurrencyManager.Instance.YellowColor);
            UpdateColorUI("Purple", CurrencyManager.Instance.PurpleColor);
            UpdateColorUI("Green", CurrencyManager.Instance.GreenColor);
            UpdateColorUI("Black", CurrencyManager.Instance.BlackColor);


        }
    }
    private void UpdateColorUI(string colorName, int count)
    {
        if (colorName == "Red" && redText != null) redText.text = count.ToString();
        if (colorName == "Blue" && blueText != null) blueText.text = count.ToString();
        if (colorName == "Yellow" && yellowText != null) yellowText.text = count.ToString();
        if (colorName == "Purple" && purpleText != null) purpleText.text = count.ToString();
        if (colorName == "Green" && greenText != null) greenText.text = count.ToString();
        if (colorName == "Black" && blackText != null) blackText.text = count.ToString();

    }


    // 숫자를 1,000 단위 콤마(,) 텍스트로 예쁘게 변환하여 반영
    private void UpdateGoldUI(int newGold)
    {
        if (goldText != null)
        {
            string abbreviatedText = "";

            if (newGold >= 1000000000) // 10억 이상 (Billion)
            {
                abbreviatedText = (newGold / 1000000000f).ToString("F1") + "B";
            }
            else if (newGold >= 1000000) // 100만 이상 (Million)
            {
                abbreviatedText = (newGold / 1000000f).ToString("F1") + "M";
            }
            else if (newGold >= 1000) // 1천 이상 (Kilo)
            {
                abbreviatedText = (newGold / 1000f).ToString("F1") + "K";
            }
            else // 1,000 미만은 그냥 순수 숫자로 표시
            {
                abbreviatedText = newGold.ToString();
            }

            goldText.text = abbreviatedText;
            Debug.Log($"[TopBar] 재화 축약 연동 완료: {newGold} -> {abbreviatedText}");
        }
    }
        // 숫자를 부드럽게 올리는 전용 기술
    private void TweenText(TMPro.TextMeshProUGUI textMesh, string textValue)
    {
        if (textMesh == null) return;
        
        // 현재 글자에 적힌 숫자를 정수로 읽어옵니다. 실패하면 0으로 시작합니다.
        int startValue = 0;
        int.TryParse(textMesh.text.Replace(",", ""), out startValue);
        
        // 새롭게 바뀔 목표 숫자입니다.
        int targetValue = 0;
        int.TryParse(textValue, out targetValue);

        // 0.3초 동안 숫자를 원래 값에서 목표 값까지 또르르륵 올려줍니다.
        DOTween.To(() => startValue, x => startValue = x, targetValue, 0.3f)
            .OnUpdate(() => {
                if (textMesh != null) textMesh.text = startValue.ToString("N0");
            });
    }


}
