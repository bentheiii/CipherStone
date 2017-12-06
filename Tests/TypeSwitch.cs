using System;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public interface IFoo
    {
        int bar { get; }
        int x { get; }
    }
    public class Foo0 : IFoo
    {
        public int bar => 0;
        public int x { get; set; }
    }
    public class Foo1 : IFoo
    {
        public int bar => 1;
        public int x { get; set; }
    }
    public class Foo2 : IFoo
    {
        public int bar => 2;
        public int x { get; set; }
    }
    public class Foo3 : IFoo
    {
        public int bar => 3;
        public int x { get; set; }
    }
    public class Foo4 : IFoo
    {
        public int bar => 4;
        public int x { get; set; }
    }
    [TestClass]
    public class IntTypeSwitch
    {
        public readonly IFormatter<IFoo> ser = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo>(new (Type, IGenericFormatter)[]
        {
            (typeof(Foo0), null),
            (typeof(Foo1), null),
            (typeof(Foo2), null),
            (typeof(Foo3), null)
        }, getFormatter.DefaultFormatter.Json);
        public readonly IFormatter<IFoo> ser4 = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo>(new(Type, IGenericFormatter)[]
        {
            (typeof(Foo0), null),
            (typeof(Foo1), null),
            (typeof(Foo2), null),
            (typeof(Foo3), null),
            (typeof(Foo4), null)
        },  getFormatter.DefaultFormatter.Json);
        public readonly IFormatter<IFoo> serSkip4 = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo>(new(Type, IGenericFormatter)[]
        {
            (typeof(Foo0), null),
            (null, null),
            (null, null),
            (null, null),
            (typeof(Foo4), null)
        }, getFormatter.DefaultFormatter.Json);
        [TestMethod] public void Simple_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = ser.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check(new Foo1 { x = 6 });
            Check(new Foo2 { x = 7 });
            Check(new Foo3 { x = 10 });
            Check4(new Foo4{ x = 100_000});
        }
        [TestMethod] public void Changing_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check(new Foo1 { x = 6 });
            Check(new Foo2 { x = 7 });
            Check(new Foo3 { x = 10 });
            Check4(new Foo4 { x = 100_000 });
        }
        [TestMethod] public void ChangingToSkip_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = serSkip4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = serSkip4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check4(new Foo4 { x = 100_000 });
        }
    }

    [TestClass]
    public class StringTypeSwitch
    {
        public readonly IFormatter<IFoo> ser = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo, string>(new(Type, IGenericFormatter, string)[]
        {
            (typeof(Foo0), null, "0"),
            (typeof(Foo1), null, "1"),
            (typeof(Foo2), null, "2"),
            (typeof(Foo3), null, "3")
        }, onDefault:getFormatter.DefaultFormatter.Json);
        public readonly IFormatter<IFoo> ser4 = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo, string>(new(Type, IGenericFormatter, string)[]
        {
            (typeof(Foo0), null, "0"),
            (typeof(Foo1), null, "1"),
            (typeof(Foo2), null, "2"),
            (typeof(Foo3), null, "3"), 
            (typeof(Foo4), null, "4")
        }, onDefault: getFormatter.DefaultFormatter.Json);
        public readonly IFormatter<IFoo> serSkip4 = TypePrefixSwitchFormatter.getTypePrefixSwitchFormatter<IFoo, string>(new(Type, IGenericFormatter, string)[]
        {
            (typeof(Foo0), null, "0"),
            (typeof(Foo4), null, "4")
        }, onDefault: getFormatter.DefaultFormatter.Json);
        [TestMethod]
        public void Simple_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = ser.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check(new Foo1 { x = 6 });
            Check(new Foo2 { x = 7 });
            Check(new Foo3 { x = 10 });
            Check4(new Foo4 { x = 100_000 });
        }
        [TestMethod]
        public void Changing_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = ser4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check(new Foo1 { x = 6 });
            Check(new Foo2 { x = 7 });
            Check(new Foo3 { x = 10 });
            Check4(new Foo4 { x = 100_000 });
        }
        [TestMethod]
        public void ChangingToSkip_int_TypeSwitch()
        {
            void Check(IFoo f)
            {
                var arr = ser.serialize(f);
                var des = serSkip4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            void Check4(IFoo f)
            {
                var arr = ser4.serialize(f);
                var des = serSkip4.deserialize(arr);
                Assert.AreEqual(des.x, f.x);
                Assert.AreEqual(des.bar, f.bar);
            }
            Check(new Foo0 { x = 5 });
            Check4(new Foo4 { x = 100_000 });
        }
    }
}
