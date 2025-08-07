// SeatManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class SeatManager : MonoBehaviour
{
    public static SeatManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Duplicate SeatManager detected, destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
        // Gather all free seats
        var freeSeats = new List<Seat>();
        foreach (var s in seats)
        {
            if (!s.IsOccupied)
                freeSeats.Add(s);
        }

        if (freeSeats.Count > 0)
        {
            // Pick a random free seat
            int randomIndex = UnityEngine.Random.Range(0, freeSeats.Count);
            var chosenSeat = freeSeats[randomIndex];
            chosenSeat.SetOccupied(true, freeColor, occColor);
            return chosenSeat.transform;
        }

        // No free seat → add to queue if valid and not already queued
        if (customer != null && !waitQueue.Contains(customer))
            waitQueue.Enqueue(customer);
        return null;
    }

    public void FreeSeat(Transform t)
    {
        var seat = seats.Find(s => s.transform == t);
        if (seat == null) return;

        seat.SetOccupied(false, freeColor, occColor);

        // Pop next valid person in queue, if any
        while (waitQueue.Count > 0)
        {
            var next = waitQueue.Dequeue();
            if (next != null) // GameObject could have been destroyed while waiting
            {
                next.SendMessage("OnSeatAvailable", SendMessageOptions.DontRequireReceiver);
                break;
            }
            // else skip destroyed entry and continue
        }
    }

    public int FreeSeatCount() => seats.FindAll(s => !s.IsOccupied).Count;

    /// <summary>
    /// Removes a customer from the waiting queue (e.g. if they despawn).
    /// </summary>
    public void RemoveFromQueue(GameObject customer)
    {
        // Simple rebuild approach; queue is usually small
        if (!waitQueue.Contains(customer)) return;
        var temp = new Queue<GameObject>();
        while (waitQueue.Count > 0)
        {
            var c = waitQueue.Dequeue();
            if (c != customer && c != null)
                temp.Enqueue(c);
        }
        // restore remaining
        while (temp.Count > 0)
            waitQueue.Enqueue(temp.Dequeue());
    }
}
