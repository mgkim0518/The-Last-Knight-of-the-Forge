using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSlots : MonoBehaviour
{
    [Header("칸 (왼쪽부터 순서대로 넣으세요)")]
    [SerializeField] private Transform[] slots;
    [SerializeField] private Transform next;

    public bool TryGetSlotPos(int slot, out Vector3 pos)
    {
        pos = default;

        if (slot >= slots.Length || slot < 0)
        {
            Debug.LogWarning($"칸을 벗어난 값이 입력되었습니다: {slot}");
            return false;
        }

        pos = slots[slot].position;
        return true;

    }

    public Vector3 NextEnemyPos()
    {
        return next.position;
    }
}
