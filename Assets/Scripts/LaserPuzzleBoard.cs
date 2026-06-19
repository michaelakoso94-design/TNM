using UnityEngine;

/// <summary>
/// 5x5 toggle-button puzzle ("L0345" board). When the lit buttons exactly match
/// <see cref="solutionRows"/>, the lasers are disabled and the board locks.
/// </summary>
public class LaserPuzzleBoard : MonoBehaviour
{
    [Tooltip("Solution pattern, 5 rows top-to-bottom as the player sees the board. '1' = lit.")]
    public string[] solutionRows = { "11000", "01110", "00000", "00000", "00110" };

    [Header("References")]
    public GameObject lasersRoot;
    public Material litMaterial;
    public Material unlitMaterial;
    public Renderer statusLamp;
    public Material lampArmedMaterial;
    public Material lampSolvedMaterial;
    public AudioSource audioSource;
    public AudioClip toggleClip;
    public AudioClip solvedClip;

    public bool Solved { get; private set; }

    readonly LaserPuzzleButton[,] _buttons = new LaserPuzzleButton[5, 5];

    void Awake()
    {
        foreach (var b in GetComponentsInChildren<LaserPuzzleButton>())
        {
            if (b.row >= 0 && b.row < 5 && b.col >= 0 && b.col < 5)
                _buttons[b.row, b.col] = b;
        }
    }

    public void OnButtonToggled(LaserPuzzleButton _)
    {
        if (Solved) return;
        if (audioSource != null && toggleClip != null) audioSource.PlayOneShot(toggleClip);
        if (IsPatternMatched()) Solve();
    }

    bool IsPatternMatched()
    {
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                var btn = _buttons[r, c];
                if (btn == null) return false;
                bool want = r < solutionRows.Length && c < solutionRows[r].Length
                            && solutionRows[r][c] == '1';
                if (btn.IsLit != want) return false;
            }
        }
        return true;
    }

    void Solve()
    {
        Solved = true;
        if (lasersRoot != null) lasersRoot.SetActive(false);
        if (statusLamp != null && lampSolvedMaterial != null)
            statusLamp.sharedMaterial = lampSolvedMaterial;
        if (audioSource != null && solvedClip != null) audioSource.PlayOneShot(solvedClip);
        Debug.Log("[LaserPuzzle] Pattern matched — lasers disarmed.");
    }
}
