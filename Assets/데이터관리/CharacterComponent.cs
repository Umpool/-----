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
}
