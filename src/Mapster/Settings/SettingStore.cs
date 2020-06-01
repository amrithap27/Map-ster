﻿using System;
using System.Collections;
using Mapster.Utils;
using System.Collections.Concurrent;

namespace Mapster
{
    public class SettingStore: IApplyable<SettingStore>
    {
        private readonly ConcurrentDictionary<string, object> _objectStore = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, bool?> _booleanStore = new ConcurrentDictionary<string, bool?>();
       
        public void Set(string key, bool? value)
        {
            if (value == null)
                _booleanStore.TryRemove(key, out _);
            else
                _booleanStore[key] = value;
        }

        public void Set(string key, object? value)
        {
            if (value == null)
                _objectStore.TryRemove(key, out _);
            else
                _objectStore[key] = value;
        }

        public bool? Get(string key)
        {
            return _booleanStore.GetValueOrDefault(key);
        }

        public T Get<T>(string key)
        {
            return (T)_objectStore.GetValueOrDefault(key)!;
        }

        public T Get<T>(string key, Func<T> initializer)
        {
            var value = _objectStore.GetValueOrDefault(key);
            if (value == null)
            {
                _objectStore.AddOrUpdate(key,  value = initializer()!, (key, oldValue) => value);
            }
            return (T)value;
        }

        public virtual void Apply(object other)
        {
            if (other is SettingStore settingStore)
                Apply(settingStore);
        }
        public void Apply(SettingStore other)
        {
            foreach (var kvp in other._booleanStore)
            {
                if (_booleanStore.GetValueOrDefault(kvp.Key) == null)
                    _booleanStore.AddOrUpdate(kvp.Key, kvp.Value, (key, oldValue) => kvp.Value);
            }

            foreach (var kvp in other._objectStore)
            {
                var self = _objectStore.GetValueOrDefault(kvp.Key);
                if (self == null)
                {
                    var value = kvp.Value;
                    if (value is IApplyable)
                    {
                        var applyable = (IApplyable)Activator.CreateInstance(value.GetType())!;
                        applyable.Apply(value);
                        value = applyable;
                    }
                    else if (value is IList side)
                    {
                        var list = (IList)Activator.CreateInstance(value.GetType())!;
                        foreach (var item in side)
                            list.Add(item);
                        value = list;
                    }
                    _objectStore.AddOrUpdate(kvp.Key, value, (key, oldValue) => value);
                }
                else if (self is IApplyable applyable)
                {
                    applyable.Apply(kvp.Value);
                }
                else if (self is IList list && kvp.Value is IList side)
                {
                    if (!list.IsSynchronized)
                    {
                        lock (list.SyncRoot)
                        {
                            foreach (var item in side)
                                list.Add(item);
                        }
                    }
                }
            }
        }
    }
}