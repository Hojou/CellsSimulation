using System;

public class SyncableValueHolder<T> where T : IEquatable<T>
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
        if (_value.Equals(newValue)) return;
        UpdateValue(newValue);
    }

    public void SyncValue(ref T r)
    {
        if (_isSynced) return;
        r = _value;
        _isSynced = true;
    }

    private void UpdateValue(T newValue)
    {
        _value = newValue;
        _isSynced = false;

    }
}
