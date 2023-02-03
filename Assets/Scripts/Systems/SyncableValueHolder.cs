using System;

public class SyncableValueHolder<T>
{
    private T _value;
    private bool _isSynced;

    public SyncableValueHolder(T value)
    {
        _value = value;
        _isSynced = true;
    }

    public void SetValue(T newValue)
    {
        //if (_value.Equals(newValue)) return;
        UpdateValue(newValue);
    }

    //public bool SyncValue()
    //{
    //    var wasSynced = _isSynced;
    //    _isSynced = true;
    //    return wasSynced;
    //}

    public bool SyncValue(ref T r)
    {
        if (_isSynced) return false;
        r = _value;
        _isSynced = true;
        return true;
    }

    private void UpdateValue(T newValue)
    {
        _value = newValue;
        _isSynced = false;

    }
}
