using UnityEngine;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    [System.Serializable]
    public struct TabGroup
    {
        public Button tabButton;
        public Image tabImage;         
        public GameObject contentPanel; 
    }

    [Header("Settings Tabs Setup")]
    [SerializeField] private TabGroup[] tabs;

    [Header("Color States")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; 
            tabs[i].tabButton.onClick.AddListener(() => OpenTab(index));
        }

        if (tabs.Length > 0)
        {
            OpenTab(0);
        }
    }

    public void OpenTab(int targetIndex)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (i == targetIndex)
            {
                tabs[i].contentPanel.SetActive(true);
                tabs[i].tabImage.color = selectedColor;
            }
            else
            {
                tabs[i].contentPanel.SetActive(false);
                tabs[i].tabImage.color = unselectedColor;
            }
        }
    }
}