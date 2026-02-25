using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth; // HP管理スクリプト
    [SerializeField] private Image fillImage;           // HPバーのFill Image

    void Update()
    {
        if (playerHealth == null || fillImage == null) return;

        float fill = (float)playerHealth.GetCurrentHP() / playerHealth.GetCurrentHPMax();
        fillImage.fillAmount = fill;
    }
}