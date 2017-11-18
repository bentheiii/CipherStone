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
    public class ArraySerialization
    {
        [TestMethod] public void Simple()
        {
            void Check<T>(params T[] a)
            {
                var ser = getSerializer.GetSerializer<T[]>();
                var bytes = ser.serialize(a);
                var des = ser.deserialize(bytes);
                Assert.IsTrue(a.SequenceEqual(des));
            }
            
            Check("a", "bfdsd", "", "aaaaaaaaa");
            Check(Tuple.Create(125, 54), Tuple.Create(125,145));
            Check(1,2,3,4,5);
            Check(1.5,0.5,0.0,1e8,double.PositiveInfinity);
        }
    }
}
