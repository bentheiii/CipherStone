using System.IO;
using System.Text;
using Newtonsoft.Json;
using WhetStone.Streams;

namespace CipherStone
{
    public class JsonSerializer<T> : IByteSerializer<T>
    {
        public JsonSerializer(Encoding encoder = null, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
        {
            this.encoder = encoder ?? Encoding.UTF8;
            this.formatting = formatting;
            this.settings = settings;
        }
        public Formatting formatting { get; }
        public JsonSerializerSettings settings { get; }
        public Encoding encoder { get; }
        public T deserialize(Stream source)
        {
            return JsonConvert.DeserializeObject<T>(encoder.GetString(source.ReadAll()));
        }
        public void serialize(T o, Stream sink)
        {
            var ret = encoder.GetBytes(JsonConvert.SerializeObject(o, formatting, settings));
            sink.Write(ret,0,ret.Length);
        }
        public int serializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => false;
    }
}