using UnityEngine;
using UnityEngine.InputSystem;     // <- new input system!

public class SeatManagerTester : MonoBehaviour
{
    SeatManager sm;

    void Awake() => sm = Object.FindFirstObjectByType<SeatManager>();

    void Update()
    {   
        // New Input System-style polling
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            var dummy = new GameObject("DummyCustomer");
            var seat  = sm.RequestSeat(dummy);
            if (seat != null) dummy.transform.position = seat.position;
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
            Debug.Log($"Free seats: {sm.FreeSeatCount()}");
    }
}
