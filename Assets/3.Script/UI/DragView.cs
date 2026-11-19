using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragView : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Text weaponText;

    private void Awake()
    {
        weaponIcon.gameObject.SetActive(false);
        weaponText.gameObject.SetActive(false);
    }
    public void Show(Sprite sprite, string text, Vector2 pos)
    {
        if (sprite != null)
        {
            weaponIcon.sprite = sprite;
            weaponIcon.gameObject.SetActive(true);
        }
        else
        {
            weaponIcon.gameObject.SetActive(false);
        }

        weaponText.text = text;
        weaponText.gameObject.SetActive(true);

        transform.position = pos;
    }

    public void Move(Vector2 pos)
    {
        transform.position = pos;
    }

    public void Hide()
    {
        weaponIcon.gameObject.SetActive(false);
        weaponText.gameObject.SetActive(false);
    }
}
