using UnityEngine;

/// <summary>
/// One cell of the 5x5 laser puzzle. Toggles lit/unlit when poked by a
/// <see cref="PokeTip"/> (sphere on the controller tips).
/// </summary>
[RequireComponent(typeof(Collider))]
public class LaserPuzzleButton : MonoBehaviour
{
    public int row;
    public int col;
    public LaserPuzzleBoard board;
    public float pokeCooldown = 0.4f;

    public bool IsLit { get; private set; }

    Renderer _renderer;
    float _nextPokeTime;

    void Awake() => _renderer = GetComponent<Renderer>();

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PokeTip>() == null) return;
        if (Time.time < _nextPokeTime) return;
        if (board != null && board.Solved) return;

        _nextPokeTime = Time.time + pokeCooldown;
        SetLit(!IsLit);
        if (board != null) board.OnButtonToggled(this);
    }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (board == null || _renderer == null) return;
        var mat = lit ? board.litMaterial : board.unlitMaterial;
        if (mat != null) _renderer.sharedMaterial = mat;
    }
}
