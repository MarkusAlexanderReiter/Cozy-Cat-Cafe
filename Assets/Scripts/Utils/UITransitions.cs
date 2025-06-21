using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// Utility class for simple full-screen fade transitions using a CanvasGroup.
/// </summary>
public static class UITransitions
{
    private static CanvasGroup _fadeGroup;

    /// <summary>
    /// Fades the screen from transparent to black.
    /// Blocks raycasts during the fade.
    /// </summary>
    /// <param name="duration">Duration in seconds (unscaled time).</param>
    public static async UniTask FadeOutAsync(float duration = 0.25f)
    {
        CanvasGroup cg = GetOrCreateFadeGroup();
        cg.blocksRaycasts = true;
        await FadeAsync(cg, cg.alpha, 1f, duration);
    }

    /// <summary>
    /// Fades the screen from black to transparent.
    /// Unlocks raycasts after fade completes.
    /// </summary>
    /// <param name="duration">Duration in seconds (unscaled time).</param>
    public static async UniTask FadeInAsync(float duration = 0.25f)
    {
        CanvasGroup cg = GetOrCreateFadeGroup();
        await FadeAsync(cg, cg.alpha, 0f, duration);
        cg.blocksRaycasts = false;
    }

    #region Internal Helpers

    private static CanvasGroup GetOrCreateFadeGroup()
    {
        if (_fadeGroup != null) return _fadeGroup;

        var fadeGO = new GameObject("ScreenFade");
        Object.DontDestroyOnLoad(fadeGO);

        var canvas = fadeGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // ensure on top of everything

        fadeGO.AddComponent<CanvasScaler>();
        fadeGO.AddComponent<GraphicRaycaster>();

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvas.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;

        var rect = img.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _fadeGroup = imgGO.AddComponent<CanvasGroup>();
        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;

        return _fadeGroup;
    }

    private static async UniTask FadeAsync(CanvasGroup cg, float from, float to, float duration)
    {
        if (Mathf.Approximately(duration, 0f))
        {
            cg.alpha = to;
            return;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        cg.alpha = to;
    }

    #endregion
}
