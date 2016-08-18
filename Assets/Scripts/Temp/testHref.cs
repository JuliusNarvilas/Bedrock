using UnityEngine;

public class testHref : MonoBehaviour
{
    public TextPic textPic;

    void Awake()
    {
        textPic = GetComponent<TextPic>();
    }

    void OnEnable()
    {
        textPic.onHrefClick.AddListener(OnHrefClick);
    }

    void OnDisable()
    {
        textPic.onHrefClick.RemoveListener(OnHrefClick);
    }

    private void OnHrefClick(string hrefName)
    {
        Debug.Log("Click on the " + hrefName);
    }
}
