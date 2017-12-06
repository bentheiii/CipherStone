using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CipherStone
{
    public class JsonFormatter<T> : IFormatter<T>
    {
        public JsonFormatter(Encoding encoder = null, Formatting formatting = Formatting.None)
        {
            this.encoder = encoder ?? new UTF8Encoding(false);
            this.formatting = formatting;
            inner = new JsonSerializer {Formatting = formatting};
        }
        public Formatting formatting { get; }
        public Encoding encoder { get; }
        public JsonSerializer inner { get; }
        public T Deserialize(Stream source)
        {
            using (var sr = new StreamReader(source, encoder,false,1024,true))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                if (!jsonTextReader.Read())
                    throw new EndOfStreamException();
                return inner.Deserialize<T>(jsonTextReader);
            }
        }
        public void Serialize(T o, Stream sink)
        {
            using (var sr = new StreamWriter(sink, encoder, 1024, true))
            using (var jsonTextWriter = new JsonTextWriter(sr))
            {
                inner.Serialize(jsonTextWriter, o);
            }
        }
        public int SerializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => false;
    }
}