/* -----------------------------------------------------------
 * CustomerSpawnedChannelSO
 *  A ScriptableObject event channel. Any system can raise or
 *  listen to the customer-spawned event without singletons.
 * ----------------------------------------------------------*/
using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CustomerSpawnedChannel",
    menuName = "Events/Channels/Customer Spawned")]
public sealed class CustomerSpawnedChannelSO : ScriptableObject
{
    public event Action<GameObject> OnCustomerSpawned;

    /// <summary>Publisher call-point.</summary>
    public void Raise(GameObject customer) =>
        OnCustomerSpawned?.Invoke(customer);
}
