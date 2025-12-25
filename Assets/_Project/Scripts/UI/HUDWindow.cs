using TMPro;
using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Progress;
using ZXTemplate.UI;

/// <summary>
/// A simple HUD overlay window displaying progress data (coins, high score).
///
/// Lifecycle:
/// - OnPushed: subscribe to progress changes and refresh UI
/// - OnPopped: unsubscribe to avoid event leaks
///
/// Note:
/// - HUD is typically shown via UIOverlay channel "HUD" (not UIStack).
/// </summary>
public class HUDWindow : UIWindow
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text highScoreText;

    private IProgressService _progress;

    public override void OnPushed()
    {
        _progress = ServiceContainer.Get<IProgressService>();
        _progress.OnChanged += Refresh;
        Refresh();
    }

    public override void OnPopped()
    {
        if (_progress != null)
            _progress.OnChanged -= Refresh;
    }

    /// <summary>
    /// Reads current ProgressData and updates text UI.
    /// </summary>
    private void Refresh()
    {
        var d = _progress.Data;

        if (coinsText) coinsText.text = $"Coins: {d.coins}";
        if (highScoreText) highScoreText.text = $"HighScore: {d.highScore}";
    }
}
