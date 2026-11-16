using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyViewer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer shieldRenderer;

    private Enemy enemy;
    private TurnManager manager;
    private int slotIndex;

    public void SetupEnemy(Enemy enemy, TurnManager manager)
    {
        this.enemy = enemy;
        this.manager = manager;
    }

    public void UpdateEnemy(Vector3 pos, int slot)
    {
        transform.position = pos;
        slotIndex = slot;
        shieldRenderer.enabled = enemy.IsShielded;

        if (enemy.IsLoaded)
        {
            bodyRenderer.sprite = enemy.Data.ChargingSprite;
        }
        else
        {
            bodyRenderer.sprite = enemy.Data.NormalSprite;
        }

        if (slot.Equals(-1))
        {
            Color c = bodyRenderer.color;
            c.a = 0.5f;
            bodyRenderer.color = c;

            Color s = shieldRenderer.color;
            s.a = 0.5f;
            shieldRenderer.color = s;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotIndex < 0)
        {
            return;
        }

        manager.SelectTarget(slotIndex);
    }

}
