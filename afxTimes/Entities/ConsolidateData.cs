using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace afx.EmployesTime.Entities
{
    internal class ConsolidateData : TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime RegisterTime { get; set; }
        public int WorkMinutes { get; set; }
    }
}
