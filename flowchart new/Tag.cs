using Dataweb.NShape;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flowchart_new
{
    class Tag
    {
        public Shape shape = null;
        public Record record = null;

        public Tag(Shape s, Record r)
        {
            shape = s;
            record = r;
        }
    }
}
