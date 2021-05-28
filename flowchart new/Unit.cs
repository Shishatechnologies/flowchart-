using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using System.Collections.Generic;

namespace flowchart_new
{
     class Unit
    {
        public Shape shape = null;
        public List<Record> records = new List<Record>();

        public Unit(Shape s, Record r)
        {
            shape = s;
            records.Add(r);
        }
    }
}