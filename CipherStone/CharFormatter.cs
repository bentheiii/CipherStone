using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherStone
{
    public class CharFormatter : SelectFormatter<char, string>
    {
        public CharFormatter(Encoding enc = null):base(
            new StringFormatter(enc),
            true)
        { }
        protected override string ToInner(char outer)
        {
            return new string(outer,1);
        }
        protected override char ToOuter(string inner)
        {
            return inner[0];
        }
    }
}
