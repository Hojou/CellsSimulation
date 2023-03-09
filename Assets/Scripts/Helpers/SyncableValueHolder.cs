using System;

public class SyncableValueHolder<T>
{
    private T _value;
    private bool _isSynced;

    public T Value => _value;
    public bool IsDirty => !_isSynced;

    public SyncableValueHolder(T value, bool isSynced = false)
    {
        _value = value;
        _isSynced = isSynced;
    }

    public void SetValue(T newValue)
    {
        _value = newValue;
        _isSynced = false;
    }

    public void SetDirty()
    {
        _isSynced = false;
    }


    public T SyncValue()
    {
        _isSynced = true;
        return _value;
    }

    public bool SyncValue(ref T r)
    {
        if (_isSynced) return false;
        r = _value;
        _isSynced = true;
        return true;
    }
}

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
