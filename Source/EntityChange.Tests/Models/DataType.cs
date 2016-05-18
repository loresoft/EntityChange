using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityChange.Tests.Models
{
    public class DataType
    {
        public string String { get; set; }

        public int Integer { get; set; }

        public long Long { get; set; }

        public double Double { get; set; }

        public decimal Decimal { get; set; }

        public DateTime DateTime { get; set; }

        public static DataType Sample()
        {
            return new DataType
            {
                String = "This is text.",
                Integer = 42,
                Long = 100042,
                Double = 1032.5d,
                Decimal = 12000.25m,
                DateTime = new DateTime(1980, 1, 1)
            };
        }
    }
}
