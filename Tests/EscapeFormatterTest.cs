using System;
using System.Text;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;

namespace Tests
{
    [TestClass]
    public class EscapeFormatterTest
    {
        [TestMethod] public void Simple_Escape_form_greedy()
        {
            var strings = new []{"","a","ab","a0a","a0a0a","a10a001","011", new String('a', 200) + "0" + new String('b', 200) };
            var (asci0, asci1) = Encoding.ASCII.GetBytes("01");
            var formatter = new EscapingFormatter<string>(new StringFormatter(Encoding.ASCII), asci0, asci1);

            foreach (var s in strings)
            {
                var bytes = formatter.serialize(s);
                var dec = formatter.deserialize(bytes);
                Assert.AreEqual(s, dec);
            }
        }
        //[TestMethod]
        public void Simple_Escape_form_non_greedy()
        {
            var strings = new[] { "", "a", "ab", "a0a", "a0a0a", "a10a001", "011", new String('a',200) + "0" + new String('b',200) };
            var (asci0, asci1) = Encoding.ASCII.GetBytes("01");
            var formatter = new EscapingFormatter<string>(new StringFormatter(Encoding.ASCII).EnsureNonGreedy(2), asci0, asci1);

            foreach (var s in strings)
            {
                var encoded = Encoding.ASCII.GetBytes(s);
                var bytes = formatter.serialize(s);
                var dec = formatter.deserialize(bytes);
                Assert.AreEqual(s, dec);
            }
        }
    }
}
