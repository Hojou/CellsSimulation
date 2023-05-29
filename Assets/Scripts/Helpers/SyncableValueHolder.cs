using System;

public class DirtyTracker
{
    private bool _isClean;
    public bool IsDirty => !_isClean;

    public void SetDirty()
    {
        _isClean = false;
    }
    public bool CheckIfDirtyAndThenClean()
    {
        var wasDirty = !_isClean;
        _isClean = true;
        return wasDirty;
    }
}
