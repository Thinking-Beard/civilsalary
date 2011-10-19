using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;

namespace civilsalary.datasync
{
    public sealed class EmployeeDataProvider : IEnumerable<EmployeeData>
    {
        Type _t;
        string _key;

        public EmployeeDataProvider(string governmentKey, string typeName)
        {
            _key = governmentKey;
            _t = Type.GetType(typeName);
        }

        public EmployeeDataProvider(string governmentKey, Type t)
        {
            _key = governmentKey;
            _t = t;
        }

        public IEnumerator<EmployeeData> GetEnumerator()
        {
            var instance = (IEnumerator<EmployeeData>)Activator.CreateInstance(_t, _key);

            return instance;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string GovernmentKey 
        { 
            get 
            { 
                return _key; 
            } 
        }
    }

    public abstract class EmployeeDataProviderEnumerator : IEnumerator<EmployeeData>
    {
        protected bool IsDisposed { get; private set; }

        protected EmployeeDataProviderEnumerator()
        {
            IsDisposed = false;
        }

        public abstract EmployeeData Current { get; }
        
        public void Dispose() { }

        object System.Collections.IEnumerator.Current
        {
            get 
            {
                if (IsDisposed) throw new ObjectDisposedException(null);

                return this.Current; 
            }
        }

        public abstract bool MoveNext();

        public void Reset()
        {
            if (IsDisposed) throw new ObjectDisposedException(null);

            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }
    }
}
