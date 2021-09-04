using System;

namespace afx.Common.Models
{
    public class EmployeeTime
    {
        public int IdEmployee { get; set; }
        public DateTime RegisterTime { get; set; }
        public bool TypeOutput { get; set; }
        public bool Consolidate { get; set; }
    }
}
