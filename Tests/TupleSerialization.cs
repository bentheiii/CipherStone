using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TupleSerialization
    {
        [TestMethod] public void Two()
        {
            void Check<T1, T2>(T1 a, T2 b)
            {
                var t = (a, b);
                var ser = getSerializer.GetSerializer<(T1, T2)>();
                var bytes = ser.serialize(t);
                var des = ser.deserialize(bytes);
                Assert.AreEqual(des, t);
            }

            Check(1,2);
            Check("a","bfdsd");
            Check(Tuple.Create(125,54),58.5);
        }
    }
}
