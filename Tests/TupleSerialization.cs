using System;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TupleSerialization
    {
        [TestMethod] public void Simple_Tuple_Ser()
        {
            void Check2<T1, T2>(T1 a, T2 b)
            {
                var t = (a, b);
                var ser = getFormatter.GetFormatter<(T1, T2)>();
                Assert.IsFalse(ser is DotNetFormatter<(T1, T2)>);
                var bytes = ser.serialize(t);
                var des = ser.deserialize(bytes);
                Assert.AreEqual(des, t);
            }

            Check2(1,2);
            Check2("a","bfdsd");
            Check2(Tuple.Create(125,54),58.5);

            void Check3<T1, T2, T3>(T1 a, T2 b, T3 c)
            {
                var t = (a, b, c);
                var ser = getFormatter.GetFormatter<(T1, T2, T3)>();
                Assert.IsFalse(ser is DotNetFormatter<(T1, T2, T3)>);
                var bytes = ser.serialize(t);
                var des = ser.deserialize(bytes);
                Assert.AreEqual(des, t);
            }

            Check3(1, 2, 3.0);
            Check3("a", "bfdsd", -1);
            Check3(Tuple.Create(125, 54), 58.5, 'a');

            void Check4<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d)
            {
                var t = (a, b, c, d);
                var ser = getFormatter.GetFormatter<(T1, T2, T3, T4)>();
                Assert.IsFalse(ser is DotNetFormatter<(T1,T2,T3,T4)>);
                var bytes = ser.serialize(t);
                var des = ser.deserialize(bytes);
                Assert.AreEqual(des, t);
            }
            Check4((byte)1, (byte)2, (byte)3, (byte)4);
            Check4(1, 2, 3.0, "din");
            Check4("a", "bfdsd", -1, -0.1);
            Check4(Tuple.Create(125, 54), 58.5, 'a', long.MaxValue);
        }
    }
}
