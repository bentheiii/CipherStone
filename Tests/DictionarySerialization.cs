using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;

namespace Tests
{
    [TestClass]
    public class DictionarySerialization
    {
        [TestMethod] public void Simple_Dict_Ser()
        {
            void Check<K,V>(params (K,V)[] arr)
            {
                var a = new Dictionary<K, V>(arr.ToDictionary());
                var ser = getFormatter.GetFormatter<Dictionary<K,V>>();
                var bytes = ser.serialize(a);
                var des = ser.deserialize(bytes);
                foreach (var k in a.Keys)
                {
                    Assert.AreEqual(a[k],des[k]);
                }
            }

            Check((1,5),(7,8),(10,5),(-8,0));
            Check(("a",8), ("bfdsd",25));
            Check((8,Tuple.Create(125, 54)),(-1, Tuple.Create(8,-100)));
        }
    }
}
