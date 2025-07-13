// SeatManager.cs
using System.Collections.Generic;
using UnityEngine;

public class SeatManager : MonoBehaviour
{
    [SerializeField] Color freeColor = Color.green;

    [SerializeField] Color occColor  = Color.red;

    readonly List<Seat> seats = new();
    private readonly Queue<GameObject> waitQueue = new Queue<GameObject>();

    /* ---------- called from Seat.cs ---------- */
    public void RegisterSeat(Seat s)
    {
        if (!seats.Contains(s))
        {
            seats.Add(s);
            s.SetOccupied(false, freeColor, occColor);
        }
    }

    public void UnregisterSeat(Seat s) => seats.Remove(s);
    /* ----------------------------------------- */



    public Transform RequestSeat(GameObject customer)
    {
        foreach (var s in seats)
            if (!s.IsOccupied)
            {
                s.SetOccupied(true, freeColor, occColor);
                return s.transform;
            }

        // No free seat → add to queue
        waitQueue.Enqueue(customer);
        return null;
    }

    public void FreeSeat(Transform t)
    {
        var seat = seats.Find(s => s.transform == t);
        if (seat == null) return;

        seat.SetOccupied(false, freeColor, occColor);

        // Pop next person in queue, if any
        if (waitQueue.Count > 0)
        {
            var next = waitQueue.Dequeue();
            next.SendMessage("OnSeatAvailable", SendMessageOptions.DontRequireReceiver);
        }
    }

    public int FreeSeatCount() => seats.FindAll(s => !s.IsOccupied).Count;
}
