using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace civilsalary.data
{
    public sealed class GovernmentAssociationRow : TableServiceEntity
    {
        string _key1;
        string _key2;
        string _assocation;

        public const string ParentOfType = "Parent";
        public const string ChildOfType = "Child";
        public const string AdjacentToType = "Adjacent";

        public string Association
        {
            get
            {
                return _assocation;
            }
            set
            {
                SecUtility.CheckKey(value);

                _assocation = value;

                PartitionKey = SecUtility.CombineToKey(_key1, value.ToString());
            }
        }
        public string Key1 
        {
            get
            {
                return _key1;   
            }
            set
            {
                SecUtility.CheckKey(value);

                _key1 = value;

                PartitionKey = SecUtility.CombineToKey(value, _assocation.ToString());
            }
        }
        public string Key2
        {
            get
            {
                return _key2;
            }
            set
            {
                SecUtility.CheckKey(value);

                _key2 = value;

                RowKey = SecUtility.EscapeKey(value);
            }
        }
    }
}
