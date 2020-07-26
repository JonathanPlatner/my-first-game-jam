using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform rt;
    [SerializeField]
    Player player;
    private void UpdateHealthBar()
    {

        rt.sizeDelta = new Vector2(400 * player.GetHealthPercentage(), rt.rect.height);
    }

    private void Update()
    {
        UpdateHealthBar();
    }
}
