using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;
using WhetStone.WordPlay;

namespace Tests
{
    [TestClass]
    public class SecureEncryption
    {
        [TestMethod]
        public void Simple()
        {
            string[] strings = {
                "test", "plain", "askaskljsjdfbvdskjfgdksfdklschabcgfdhkasc",
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            };
            foreach (var v in strings.Join(@join.CartesianType.AllPairs))
            {
                var plain = v.Item1.ToBytes();
                var key = v.Item2.ToBytes();
                var cipher = CipherStone.SecureEncryption.Encrypt(plain, key, plain.GetHashCode()%640, () => 0);
                var deciphered = CipherStone.SecureEncryption.Decrypt(cipher, key);
                Assert.IsTrue(plain.SequenceEqual(deciphered));
            }
        }
    }
}
