using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image hpFill;
    public Image previewFill;

    public void UpdateHP(int currentHP, int maxHP)
    {
        float fillAmount = (float)currentHP / maxHP;
        hpFill.fillAmount = fillAmount;

        previewFill.fillAmount = fillAmount;
    }

    public void ShowPreview(int currentHP, int damage ,int maxHP)
    {
        int afterDamageHp = Mathf.Max(0, currentHP - damage);

        hpFill.fillAmount = (float)afterDamageHp / maxHP;
        previewFill.fillAmount = (float)currentHP / maxHP;
    }

    public void CancelPreview(int currentHP, int maxHP)
    {
        UpdateHP(currentHP, maxHP);
    }
}
