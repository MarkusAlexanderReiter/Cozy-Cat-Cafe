using UnityEngine;
using TMPro;

public partial class CustomerLifecycle : MonoBehaviour
{
    [Tooltip("Optional UI label to show the current request.")]
    [SerializeField] private TMP_Text _requestLabel; // Use base TMP_Text to accept both 3D and UI variants

    /// <summary>
    /// Exposes the current request for other systems / UI.
    /// </summary>
    public CustomerRequestType CurrentRequest => _currentRequest;

    /// <summary>
    /// Called by the player (or other system) to give an item to this customer.
    /// </summary>
    /// <param name="deliveredItem">The type of item handed to the customer.</param>
    public void DeliverItem(CustomerRequestType deliveredItem)
    {
        bool correct = deliveredItem == _currentRequest;
        // Simple satisfaction: correct item and reasonable wait time
        _isSatisfied = correct && _lastWaitDuration <= 5f;
        Debug.Log($"[CustomerLifecycle] Delivered {deliveredItem}. Wanted {_currentRequest}. Success = {correct}");
        // Hide the request label now that the request has been fulfilled (whether correct or not)
        SetRequestLabelVisible(false);
        // Show emote only when we actually want visual feedback (e.g., on leave or after request)
        if (emoteBubblePrefab != null)
        {
            ShowEmote(_isSatisfied);
        }
    }

    private void Update()
    {
        // Ensure label is hidden on start (in case prefab was left enabled)
        if (_requestLabel != null && !_requestLabel.gameObject.activeSelf)
        {
            // do nothing, just keep hidden
        }
    }

    /// <summary>
    /// Show or hide the request label.
    /// </summary>
    public void SetRequestLabelVisible(bool visible)
    {
        if (_requestLabel != null)
        {
            _requestLabel.gameObject.SetActive(visible);
        }
    }
}
