using UnityEngine;

public class GraceTimer
{
    private float _time;
    public bool Active => _time > 0f;

    public void Start(float duration)
    {
        _time = duration;
    }

    public void Tick(float delta)
    {
        _time = Mathf.Max(0f, _time - delta);
    }

    public void Reset() => _time = 0f;
}
