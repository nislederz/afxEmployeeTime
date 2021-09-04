using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace afxTimes.Entities
{
    public class EmployeesTime : TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime RegisterTime { get; set; }
        public bool TypeOutput { get; set; }
        public bool Consolidate { get; set; }
    }
}
