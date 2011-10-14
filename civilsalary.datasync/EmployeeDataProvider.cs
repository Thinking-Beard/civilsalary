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
        public abstract EmployeeRow Current { get; }
        
        public virtual void Dispose() { }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public abstract bool MoveNext();

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
