using System.Numerics;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;

namespace Tests
{
    [TestClass]
    public class VarSizeFormatting
    {
        public TestContext TestContext { get; set; }
        [TestMethod] public void Simple_Var_Form()
        {
            void check(int size, int max)
            {
                IFormatter<BigInteger> form = new VarSizeIntFormatter(size, true);
                foreach (var x in range.IRange(-max,max).Select(a=>new BigInteger(a)))
                {
                    var bytes = form.serialize(x);
                    Assert.AreEqual(bytes.Length, size);
                    var obj = form.deserialize(bytes);
                    Assert.AreEqual(x, obj);
                }

                form = new VarSizeIntFormatter(size, false);
                foreach (var x in range.IRange(0, max).Select(a => new BigInteger(a)))
                {
                    TestContext.WriteLine(x.ToString());
                    var bytes = form.serialize(x);
                    Assert.AreEqual(bytes.Length, size);
                    var obj = form.deserialize(bytes);
                    Assert.AreEqual(x, obj);
                }
            }
            check(1, 125);
            check(2, 32_000);
            check(3, 800_000);
            check(4, 800_000);
        }
    }
}
