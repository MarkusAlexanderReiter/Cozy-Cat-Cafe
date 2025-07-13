using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Seat : MonoBehaviour
{
    public bool   IsOccupied { get; private set; }
    SeatManager   manager;
    SpriteRenderer sr;

    void Awake()
    {
        manager = Object.FindFirstObjectByType<SeatManager>();
        sr      = GetComponent<SpriteRenderer>();
    }

    void OnEnable()  => manager.RegisterSeat(this);
    void OnDisable() => manager.UnregisterSeat(this);

    public void SetOccupied(bool occupied, Color freeCol, Color occCol)
    {
        IsOccupied = occupied;
        sr.color   = occupied ? occCol : freeCol;
    }
}
