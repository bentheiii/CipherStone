using System.Collections.Generic;
using System.Linq;
using System.Text;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhetStone.Looping;
using WhetStone.WordPlay;

namespace Tests
{
    [TestClass]
    public class SecureEncryption
    {
        static readonly IEnumerable<byte[]> Strings = new []
        {
            "","test", "plain", "askaskljsjdfbvdskjfgdksfdklschabcgfdhkasc",
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            "עברית",
            "3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798609437027705392171762931767523846748184676694051320005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235420199561121290219608640344181598136297747713099605187072113499999983729780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083814206171776691473035982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989",
            "אָנֹכִי ה\' אֱלֹהֶיךָ אֲשֶׁר הוֹצֵאתִיךָ מֵאֶרֶץ מִצְרַיִם מִבֵּית עֲבָדִים.\r\n\r\nלֹא יִהְיֶה לְךָ אֱלֹהִים אֲחֵרִים עַל פָּנָי, לֹא תַעֲשֶׂה לְךָ פֶסֶל וְכָל תְּמוּנָה אֲשֶׁר בַּשָּׁמַיִם מִמַּעַל וַאֲשֶׁר בָּאָרֶץ מִתָּחַת וַאֲשֶׁר בַּמַּיִם מִתַּחַת לָאָרֶץ. לֹא תִשְׁתַּחֲוֶה לָהֶם וְלֹא תָעָבְדֵם, כִּי אָנֹכִי ה\' אֱלֹהֶיךָ אֵל קַנָּא, פֹּקֵד עֲוֹן אָבֹת עַל בָּנִים עַל שִׁלֵּשִׁים וְעַל רִבֵּעִים לְשׂנְאָי, וְעֹשֶׂה חֶסֶד לַאֲלָפִים לְאֹהֲבַי וּלְשֹׁמְרֵי מִצְוֹתָי.\r\n\r\nלֹא תִשָּׂא אֶת שֵׁם ה\' אֱלֹהֶיךָ לַשָּׁוְא, כִּי לֹא יְנַקֶּה ה\' אֵת אֲשֶׁר יִשָּׂא אֶת שְׁמוֹ לַשָּׁוְא.\r\n\r\nזָכוֹר אֶת יוֹם הַשַּׁבָּת לְקַדְּשׁוֹ. שֵׁשֶׁת יָמִים תַּעֲבֹד וְעָשִׂיתָ כָּל מְלַאכְתֶּךָ, וְיוֹם הַשְּׁבִיעִי שַׁבָּת לַה\' אֱלֹהֶיךָ. לֹא תַעֲשֶׂה כָל מְלָאכָה אַתָּה וּבִנְךָ וּבִתֶּךָ עַבְדְּךָ וַאֲמָתְךָ וּבְהֶמְתֶּךָ וְגֵרְךָ אֲשֶׁר בִּשְׁעָרֶיךָ. כִּי שֵׁשֶׁת יָמִים עָשָׂה ה\' אֶת הַשָּׁמַיִם וְאֶת הָאָרֶץ, אֶת הַיָּם וְאֶת כָּל אֲשֶׁר בָּם, וַיָּנַח בַּיּוֹם הַשְּׁבִיעִי, עַל כֵּן בֵּרַךְ ה\' אֶת יוֹם הַשַּׁבָּת וַיְקַדְּשֵׁהוּ.\r\n\r\nכַּבֵּד אֶת אָבִיךָ וְאֶת אִמֶּךָ לְמַעַן יַאֲרִכוּן יָמֶיךָ עַל הָאֲדָמָה אֲשֶׁר ה\' אֱלֹהֶיךָ נֹתֵן לָךְ.\r\nלֹא תִרְצַח.\r\n\r\nלֹא תִנְאָף.\r\n\r\nלֹא תִגְנֹב.\r\n\r\nלֹא תַעֲנֶה בְרֵעֲךָ עֵד שָׁקֶר.\r\n\r\nלֹא תַחְמֹד בֵּית רֵעֶךָ. לֹא תַחְמֹד אֵשֶׁת רֵעֶךָ וְעַבְדּוֹ וַאֲמָתוֹ וְשׁוֹרוֹ וַחֲמֹרוֹ וְכֹל אֲשֶׁר לְרֵעֶךָ.",
            "what if we hadd spe\0cial esc\ape charact\bers?",
            //range.Range('\u0000','\u0fff').ConvertToString()
        }.Select(Encoding.UTF8.GetBytes).Concat(range.Range(32).Select(a=>fill.Fill(a,(byte)10))).ToArray();
        static readonly IEnumerable<(byte[], byte[])> Combos;
        static SecureEncryption()
        {
            Combos = Strings.Join().SelectMany(a => a.Enumerate(5)).ToArray();
        }
        [TestMethod]
        public void Simple_enc_V1()
        {
            foreach (var (plain,key) in Combos)
            {
                var cipher = SecureEncryptionV1.Encrypt(plain, key, plain.GetHashCode()%640);
                var deciphered = SecureEncryptionV1.Decrypt(cipher, key);
                Assert.IsTrue(plain.SequenceEqual(deciphered));
            }
        }
        public TestContext TestContext { get; set; }
        [TestMethod]
        public void Simple_enc_V2()
        {
            foreach (var (plain, key) in Combos)
            {
                //TestContext.WriteLine("plain: "+ v.Item1);
                //TestContext.WriteLine("key: "+ v.Item2);
                {
                    var cipher = SecureEncryptionV2.Encrypt(plain, key);
                    Assert.AreEqual(cipher.Length, SecureEncryptionV2.EncSize(plain.Length));
                    var deciphered = SecureEncryptionV2.Decrypt(cipher, key);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                {
                    var cipher = SecureEncryptionV2.Encrypt(plain, key, SecureEncryptionV2.EncryptionOptions.Hashing);
                    Assert.AreEqual(cipher.Length, SecureEncryptionV2.EncSize(plain.Length, SecureEncryptionV2.EncryptionOptions.Hashing));
                    var deciphered = SecureEncryptionV2.Decrypt(cipher, key, SecureEncryptionV2.EncryptionOptions.Hashing);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                var pad = key.GetHashCode() % 640;
                {
                    var cipher = SecureEncryptionV2.Encrypt(plain, key, paddingSize: pad);
                    var ec = SecureEncryptionV2.EncSize(plain.Length, paddingSize: pad);
                    Assert.AreEqual(cipher.Length, ec);
                    var deciphered = SecureEncryptionV2.Decrypt(cipher, key);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                {
                    var cipher = SecureEncryptionV2.Encrypt(plain, key, SecureEncryptionV2.EncryptionOptions.Hashing, pad);
                    Assert.AreEqual(cipher.Length, SecureEncryptionV2.EncSize(plain.Length, SecureEncryptionV2.EncryptionOptions.Hashing, pad));
                    var deciphered = SecureEncryptionV2.Decrypt(cipher, key, SecureEncryptionV2.EncryptionOptions.Hashing);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
            }
        }
        [TestMethod]
        public void Simple_enc_V3()
        {
            foreach (var (plain, key) in Combos)
            {
                //TestContext.WriteLine("plain: "+ v.Item1);
                //TestContext.WriteLine("key: "+ v.Item2);
                {
                    var cipher = SecureEncryptionV3.Encrypt(plain, key);
                    var pred = SecureEncryptionV3.EncyptSize(plain.Length);
                    Assert.AreEqual(cipher.Length, pred);
                    var deciphered = SecureEncryptionV3.Decrypt(cipher, key);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                {
                    var cipher = SecureEncryptionV3.Encrypt(plain, key, SecureEncryptionV3.EncryptionOptions.Hashing);
                    var pred = SecureEncryptionV3.EncyptSize(plain.Length, SecureEncryptionV3.EncryptionOptions.Hashing);
                    Assert.AreEqual(cipher.Length, pred);
                    var deciphered = SecureEncryptionV3.Decrypt(cipher, key, SecureEncryptionV3.EncryptionOptions.Hashing);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                var pad = key.GetHashCode() % 640;
                {
                    var cipher = SecureEncryptionV3.Encrypt(plain, key, paddingSize: pad);
                    var ec = SecureEncryptionV3.EncyptSize(plain.Length, paddingSize: pad);
                    Assert.AreEqual(cipher.Length, ec);
                    var deciphered = SecureEncryptionV3.Decrypt(cipher, key);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
                {
                    var cipher = SecureEncryptionV3.Encrypt(plain, key, SecureEncryptionV3.EncryptionOptions.Hashing, pad);
                    Assert.AreEqual(cipher.Length, SecureEncryptionV3.EncyptSize(plain.Length, SecureEncryptionV3.EncryptionOptions.Hashing, pad));
                    var deciphered = SecureEncryptionV3.Decrypt(cipher, key, SecureEncryptionV3.EncryptionOptions.Hashing);
                    Assert.IsTrue(plain.SequenceEqual(deciphered));
                }
            }
        }
    }
}
