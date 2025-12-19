using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Progress;

public class ProgressDemoHotkeys : MonoBehaviour
{
    private IProgressService _p;

    private void Start()
    {
        _p = ServiceContainer.Get<IProgressService>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) _p.AddCoins(10);
        if (Input.GetKeyDown(KeyCode.Alpha2)) _p.TrySpendCoins(5);
        if (Input.GetKeyDown(KeyCode.Alpha3)) _p.SetHighScoreIfBetter(_p.Data.highScore + 1);
        if (Input.GetKeyDown(KeyCode.R)) _p.Reset();
    }

    private void OnGUI()
    {
        var d = _p.Data;
        GUI.Label(new Rect(10, 10, 600, 24), $"Coins: {d.coins} | HighScore: {d.highScore} | UnlockedLevel: {d.unlockedLevel}");
        GUI.Label(new Rect(10, 34, 900, 24), "1:+10 coins | 2:-5 coins | 3:HighScore+1 | R:Reset (persist via JSON)");
    }
}
