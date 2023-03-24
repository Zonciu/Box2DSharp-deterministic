using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Box2DSharp.Dynamics.Contacts
{
    public class ContactPool<T>
        where T : Contact, new()
    {
        private T[] _objects;

        private int _total;

        private int _capacity;

        public ContactPool(int capacity = 256)
        {
            _capacity = capacity;
            _objects = new T[_capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            if (_total > 0)
            {
                --_total;
                var item = _objects[_total];
                _objects[_total] = null;
                return item;
            }

            return new T();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            item.Reset();
            if (_total < _capacity)
            {
                _objects[_total] = item;
                ++_total;
            }
            else
            {
                _capacity *= 2;
                Array.Resize(ref _objects, _capacity);
                _objects[_total] = item;
                ++_total;
            }
        }
    }
}