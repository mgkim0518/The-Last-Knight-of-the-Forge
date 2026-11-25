using UnityEngine;
using UnityEngine.UI;

// 피해·회복 숫자: 위로 떠오르며 사라진 뒤 스스로 삭제된다.
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float rise = 0.5f;

    private Vector3 startPos;
    private float elapsed;

    public static void Spawn(DamagePopup prefab, Vector3 pos, string text, Color color)
    {
        if (prefab == null)
        {
            return;
        }

        DamagePopup popup = Instantiate(prefab, pos, Quaternion.identity);
        popup.Show(text, color);
    }

    private void Show(string text, Color color)
    {
        label.text = text;
        label.color = color;
        startPos = transform.position;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        transform.position = startPos + Vector3.up * (rise * t);
        group.alpha = 1f - t;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
