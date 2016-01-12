namespace BoardCraft.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ReadOnlySet<T> : ISet<T>, IReadOnlyCollection<T>
    {
        private ISet<T> _backer;
        public ReadOnlySet(ISet<T> backer)
        {
            _backer = backer;
        }

        public int Count => _backer.Count;
        bool ICollection<T>.IsReadOnly => true;

        #region ForbiddenModification
        void ICollection<T>.Add(T item)
        {
            throw ModificationErrors();
        }

        bool ISet<T>.Add(T item)
        {
            throw ModificationErrors();
        }

        void ICollection<T>.Clear()
        {
            throw ModificationErrors();
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw ModificationErrors();
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw ModificationErrors();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw ModificationErrors();
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw ModificationErrors();
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw ModificationErrors();
        }

        #endregion

        public bool Contains(T item)
        {
            return _backer.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _backer.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _backer.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _backer.GetEnumerator();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _backer.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _backer.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _backer.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _backer.IsSupersetOf(other);
        }

        private Exception ModificationErrors()
        {
            return new InvalidOperationException("Modification not allowed");
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _backer.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _backer.SetEquals(other);
        }
    }
}
