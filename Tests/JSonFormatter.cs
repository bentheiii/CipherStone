using System.Collections.Generic;
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

        class Foo
        {
            public int x;
            public Foo(int x)
            {
                this.x = x;
            }
        }

        /*[TestMethod]*/ public void Json_IsnotGreedy()
        {
            var form = new JsonFormatter<Foo>();
            var ms = new MemoryStream();
            form.Serialize(new Foo(5), ms);
            form.Serialize(new Foo(6), ms);
            ms.Seek(0, SeekOrigin.Begin);
            var dec = form.Deserialize(ms);
            Assert.AreEqual(dec.x, 5);
            dec = form.Deserialize(ms);
            Assert.AreEqual(dec.x, 6);
        }

    }
}
