using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;

namespace civilsalary.datasync
{
    public sealed class EmployeeDataProvider : IEnumerable<EmployeeRow>
    {
        Type _t;

        public EmployeeDataProvider(string typeName)
        {
            _t = Type.GetType(typeName);
        }

        public EmployeeDataProvider(Type t)
        {
            _t = t;
        }

        public IEnumerator<EmployeeRow> GetEnumerator()
        {
            var instance = (IEnumerator<EmployeeRow>) Activator.CreateInstance(_t);

            return instance;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class EmployeeDataProviderEnumerator : IEnumerator<EmployeeRow>
    {
        protected bool IsDisposed { get; private set; }

        protected EmployeeDataProviderEnumerator()
        {
            IsDisposed = false;
        }

        public abstract EmployeeRow Current { get; }
        
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
