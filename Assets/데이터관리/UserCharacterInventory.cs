using System.Collections.Generic;
using UnityEngine;

public class UserCharacterInventory : MonoBehaviour
{
    public static UserCharacterInventory Instance { get; private set; }

    public List<string> ownedCharacterIDs = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventoryData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddCharacter(string characterID)
    {
        if (!ownedCharacterIDs.Contains(characterID))
        {
            ownedCharacterIDs.Add(characterID);
            SaveInventoryData();
            Debug.Log($"[Inventory] 새로운 캐릭터 창고 영입 성공: {characterID}");
        }
    }

    public void SaveInventoryData()
    {
        string combinedIDs = string.Join(",", ownedCharacterIDs);
        PlayerPrefs.SetString("UserOwnedCharacters", combinedIDs);
        PlayerPrefs.Save();
    }

    private void LoadInventoryData()
    {
        ownedCharacterIDs.Clear();
        if (PlayerPrefs.HasKey("UserOwnedCharacters"))
        {
            string combinedIDs = PlayerPrefs.GetString("UserOwnedCharacters");
            if (!string.IsNullOrEmpty(combinedIDs))
            {
                string[] splitIDs = combinedIDs.Split(',');
                foreach (string id in splitIDs)
                {
                    ownedCharacterIDs.Add(id);
                }
                Debug.Log($"[Inventory] 기존 창고 데이터 복구 완료: {combinedIDs}");
            }
        }
    }
}
