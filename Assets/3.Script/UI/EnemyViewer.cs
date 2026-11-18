using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyViewer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer shieldRenderer;

    [Header("UI")]
    [SerializeField] private CanvasGroup infoGroup;
    [SerializeField] private Image actionIcon;
    [SerializeField] private Text valText;
    [SerializeField] private Image shieldIcon;
    [SerializeField] private Image typeIcon;
    [SerializeField] private Image typeGlyph;
    [SerializeField] private Slider hpBar;

    [Header("아이콘 목록(Enum 순서에 맞춰서 넣을 것)")]
    [SerializeField] private Sprite[] actIcons;
    [SerializeField] private Color[] actColors;
    [SerializeField] private Sprite[] typeGlyphs;

    [Header("연출용 색상")]
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color healColor;

    private Enemy enemy;
    private TurnManager manager;
    private int slotIndex;

    public void SetupEnemy(Enemy enemy, TurnManager manager)
    {
        this.enemy = enemy;
        this.manager = manager;
        int enemyNum = (int)enemy.Data.EnemyType;
        hpBar.maxValue = enemy.MaxHP;
        //if (enemyNum > typeGlyphs.Length)
        //{
        //    Debug.LogWarning($"({enemy.Data.Name})에 대한 아이콘이 추가되지 않았습니다.");
        //}
        //typeIcon.sprite = typeIcons[(int)enemy.Data.EnemyType];
    }

    public void UpdateEnemy(Vector3 pos, int slot)
    {
        transform.position = pos;
        slotIndex = slot;
        hpBar.value = enemy.HP;
        shieldRenderer.enabled = enemy.IsShielded;
        shieldIcon.enabled = enemy.IsShielded;
        shieldIcon.color = Color.white;
        shieldIcon.transform.localScale = Vector3.one;
        //typeIcon.color = Color.white;
        //typeIcon.transform.localScale = Vector3.one;
        
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

            infoGroup.alpha = 0.5f;

            if (enemy.IsFirstAppearance)
            {
                //typeIcon.color = highlightColor;
                //typeIcon.transform.localScale = Vector3.one * 1.3f;
            }

            if (enemy.IsHavingFirstShield)
            {
                shieldIcon.color = highlightColor;
                shieldIcon.transform.localScale = Vector3.one * 1.3f;
            }
        }
        else
        {
            Color c = bodyRenderer.color;
            c.a = 1f;
            bodyRenderer.color = c;

            Color s = shieldRenderer.color;
            s.a = 1f;
            shieldRenderer.color = s;

            infoGroup.alpha = 1f;
        }

    }

    public void UpdateAction(NextAction? action)
    {
        if (action == null)
        {
            actionIcon.enabled = false;
            valText.enabled = false;
            return;
        }

        EnemyBehav behav = action.Value.Behav;

        actionIcon.enabled = true;

        actionIcon.sprite = actIcons[(int)behav];
        actionIcon.color = actColors[(int)behav];

        switch (behav)
        {
            case EnemyBehav.Move:
            case EnemyBehav.Wait:
            case EnemyBehav.Flee:
                valText.enabled = false;
                break;

            case EnemyBehav.Attack:
            case EnemyBehav.DecidedFleeing:
            case EnemyBehav.Heal:
            case EnemyBehav.Charging:
                valText.enabled = true;
                valText.text = action.Value.ChangeVal.ToString();
                valText.color = behav switch
                {
                    EnemyBehav.Heal => healColor,
                    EnemyBehav.Charging => highlightColor,
                    _ => Color.white
                };
                break;

            default:
                valText.enabled = false;
                break;
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
