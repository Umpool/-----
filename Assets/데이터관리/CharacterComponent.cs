using UnityEngine;

public class CharacterComponent : MonoBehaviour
{
    // [유니티 화면에서 전사, 마법사, 궁수에 각각 연결해 두신 데이터 주머니]
    public CharacterData myData;

    // 마우스로 이 캐릭터 카드를 클릭했을 때 실행되는 핵심 함수입니다.
    public void OnClickThisCharacterCard()
    {
        // 1. 유니티 월드에서 화면 UI를 통제하는 메인 매니저를 찾아옵니다.
        CharacterSelectManager selectManager = FindAnyObjectByType<CharacterSelectManager>();

        // 2. 통제실과 내 캐릭터 데이터가 모두 존재하는지 안전하게 검사합니다.
        if (selectManager != null && myData != null)
        {
            // 3. 통제실에게 내 데이터와 내 버튼 오브젝트를 동시에 배달합니다.
            selectManager.OnSelectCharacter(myData, this.gameObject);
        }
    }
        private void Start()
    {
        // 💡 [방어막 코드 추가]: 상단 파티창 슬롯 내부에서 태어난 카드라면, 창고용 장착 리스너가 중복 등록되지 않도록 탈출시킵니다!
        if (transform.parent != null && transform.parent.name.Contains("슬롯"))
        {
            return; 
        }
        // 💡 내 몸통에 붙어있는 Button 컴포넌트를 가져옵니다.
        UnityEngine.UI.Button myBtn = GetComponent<UnityEngine.UI.Button>();
        
        // 💡 캐릭터 창고 화면을 통제하는 메인 매니저를 시스템에서 검색합니다.
        CharacterStorageManager storageManager = FindAnyObjectByType<CharacterStorageManager>();
        
        // 💡 모든 부품이 완벽하게 존재한다면, 이 카드를 터치했을 때 창고 매니저의 함수가 발동하도록 연결 단추를 채웁니다!
        if (myBtn != null && storageManager != null && myData != null)
        {
            myBtn.onClick.AddListener(() => storageManager.OnClickUniqueStorageCharacter(myData.characterID));
        }
    }

}
