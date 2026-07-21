using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    // [어디서나 접근할 수 있는 싱글톤 통제실 인스턴스]
    public static PartyManager Instance { get; private set; }
    
    // 유저가 선택하여 최종적으로 마을로 들고 갈 파티 데이터 주머니 (깃허브 연동)
    public List<string> currentPartyList = new List<string>();

    private void Awake()
    {
        // 싱글톤 규칙 적용: 씬 전환 시에도 데이터를 안전하게 보존합니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    // 캐릭터 선택창에서 1명을 영입하거나 교체할 때 데이터 주머니를 갱신해 주는 함수입니다.
    public void SetMainCharacter(string characterID)
    {
        // 메인 캐릭터는 언제나 1명뿐이므로, 기존에 저장된 데이터가 있다면 깨끗이 비워줍니다.
        currentPartyList.Clear();
        
        // 새로 선택된 캐릭터의 ID를 파티 리스트에 최종 등록합니다.
        currentPartyList.Add(characterID);
        
        Debug.Log($"[Party] 메인 캐릭터 등록 완료: {characterID}");
    }
}
