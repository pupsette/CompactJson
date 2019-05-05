using System;
using System.IO;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    abstract class JsonValue
    {
        public abstract void Write(IJsonConsumer consumer);

        public string ToString(bool indentation)
        {
            using (var writer = new StringWriter())
            {
                Write(writer, indentation);
                return writer.ToString();
            }
        }

        public void Write(TextWriter textWriter, bool indentation = true)
        {
            var consumer = new TextWriterJsonConsumer(textWriter, indentation);
            Write(consumer);
        }

        public object ToModel(Type type)
        {
            var consumer = new ModelJsonConsumer(ConverterRegistry.Get(type));
            Write(consumer);
            return consumer.Result;
        }

        public T ToModel<T>()
        {
            return (T)ToModel(typeof(T));
        }

        public static JsonValue FromModel(object model)
        {
            if (model == null)
                return new JsonNull();

            var consumer = new JsonValueJsonConsumer();
            IConverter converter = ConverterRegistry.Get(model.GetType());
            converter.Write(model, consumer);
            return consumer.Result;
        }

        public override string ToString()
        {
            return ToString(true);
        }
    }
}
