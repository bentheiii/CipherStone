using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;

namespace Tests
{
    [TestClass]
    public class LenSerialize
    {
        [TestMethod] public void Simple_Len_Ser()
        {
            void Check<T>(T obj)
            {
                var ser = new LengthFormatter<T>(getFormatter.GetFormatter<T>());
                var arr = ser.serialize(obj);
                T des = ser.deserialize(arr);
                if (obj is ValueType || obj is string)
                    Assert.AreEqual(obj,des);
                else if (obj is Array)
                {
                    var oa = obj as Array;
                    var da = des as Array;
                    foreach (var inds in oa.Indices())
                    {
                        Assert.AreEqual(oa.GetValue(inds), da.GetValue(inds));
                    }
                }
                else if (obj is IDictionary)
                {
                    var od = obj as IDictionary;
                    var dd = des as IDictionary;
                    foreach (var inds in od.Keys)
                    {
                        Assert.AreEqual(od[inds], dd[inds]);
                    }
                }
                else if (obj.GetType() == typeof(object))
                {
                    Assert.IsTrue(des.GetType() == typeof(object));
                }
                else
                {
                    Assert.Fail($"unhandled type: {obj.GetType()}");
                }
            }

            var strings = new[] {"", "a", "abcdef", "garland", new string('a', 1000), new string('a', 67_000) };
            foreach (var s in strings)
            {
                Check(s);
            }
            Check(strings);
            var ints = range.Range(67_000).ToArray();
            foreach (int i in ints)
            {
                Check(i);
            }
            Check(ints);
            var bytes = new byte[255];
            Check(bytes);
            var objs = new [] {new object(), new Dictionary<int,string>{[1]="one",[2]="two"}, 5, "14587", strings, ints, bytes};
            foreach (var o in objs)
            {
                Check(o);
            }
        }
        [TestMethod] public void Simple_Term_Big()
        {
            var ser = new TerminateIntegerFormatter(false);

            void Check(BigInteger obj)
            {
                var arr = ser.serialize(obj);
                var des = ser.deserialize(arr);

                Assert.AreEqual(des, obj, obj.ToString());
            }

            foreach (var bi in range.Range(new BigInteger(67_000)))
            {
                Check(bi);
            }

            ser = new TerminateIntegerFormatter(true);

            foreach (var bi in range.Range(new BigInteger(-67_000),new BigInteger(67_000)))
            {
                Check(bi);
            }
        }
    }
}
