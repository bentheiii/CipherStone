﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class JsonFormatter
    {
        [TestMethod] public void Simple_Json()
        {
            void check<T>(T o)
            {
                var form = new JsonFormatter<T>();
                var bytes = form.serialize(o);
                var dec = form.deserialize(bytes);
                Assert.AreEqual(o,dec);
            }
            void checkNum<T, I>(I o) where I : IEnumerable<T>
            {
                var form = new JsonFormatter<I>();
                var bytes = form.serialize(o);
                var dec = form.deserialize(bytes);
                Assert.IsTrue(o.SequenceEqual(dec));
            }
            check(1);
            check(2);
            check("aa");
            check<int[]>(null);

            checkNum<int, int[]>(new []{1,2,3,4});
        }
    }
}
