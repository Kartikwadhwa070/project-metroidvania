using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    void Start()
    {
        playerController.Instance.onHealthChangedCallback += UpdateHeartsHUD;
        Debug.Log("Subscribed to OnHealthChangedCallback");
        heartContainers = new GameObject[playerController.Instance.maxHealth];
        heartFills = new Image[playerController.Instance.maxHealth];
        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < playerController.Instance.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }
    void SetFilledHearts()
    {
        Debug.Log($"SetFilledHearts called - Current Health: {playerController.Instance.Health}, MaxHealth: {playerController.Instance.maxHealth}");
        for (int i = 0; i < heartFills.Length; i++)
        {
            Debug.Log($"Setting heart {i} - Should be filled: {i < playerController.Instance.Health}");
            if (i < playerController.Instance.Health)
            {
                heartFills[i].fillAmount = 1;
                Debug.Log($"Heart {i} filled");
            }
            else
            {
                heartFills[i].fillAmount = 0;
                Debug.Log($"Heart {i} emptied");
            }
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < playerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("Heart_Fill").GetComponent<Image>();
        }
    }



    void UpdateHeartsHUD()
    {
        Debug.Log($"UpdateHeartsHUD called - Current Health: {playerController.Instance.Health}");
        SetHeartContainers();
        SetFilledHearts();
    }
}