using System;
using System.IO;

namespace CompactJson
{
    /// <summary>
    /// The base class for the object model of
    /// arbitrary JSON data.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class JsonValue
    {
        /// <summary>
        /// Writes this <see cref="JsonValue"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="Write(TextWriter, bool)"/>, 
        /// <see cref="ToModel(Type)"/> and <see cref="ToModel{T}"/> in order to convert
        /// this generic object model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public abstract void Write(IJsonConsumer consumer);

        /// <summary>
        /// Converts this generic object model to a JSON string.
        /// </summary>
        /// <param name="indentation">A flag indicating, whether to pretty-print the JSON
        /// string using indentation or not.</param>
        /// <returns>The JSON string.</returns>
        public string ToString(bool indentation)
        {
            using (var writer = new StringWriter())
            {
                Write(writer, indentation);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Converts this generic object model to a JSON string by writing to the
        /// given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The text writer to write to.</param>
        /// <param name="indentation">A flag indicating, whether to pretty-print the JSON
        /// string using indentation or not.</param>
        public void Write(TextWriter textWriter, bool indentation = true)
        {
            var consumer = new TextWriterJsonConsumer(textWriter, indentation);
            Write(consumer);
        }

        /// <summary>
        /// Converts this generic object model to another .NET object of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the .NET object to convert to.</param>
        /// <returns>An instance of the given <paramref name="type"/>.</returns>
        public object ToModel(Type type)
        {
            var consumer = new ModelJsonConsumer(ConverterRegistry.Get(type));
            Write(consumer);
            return consumer.Result;
        }

        /// <summary>
        /// Converts this generic object model to another .NET object of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the .NET object to convert to.</typeparam>
        /// <returns>An instance of the given type <typeparamref name="T"/>.</returns>
        public T ToModel<T>()
        {
            return (T)ToModel(typeof(T));
        }

        /// <summary>
        /// Converts the given .NET object to the generic JSON object model represented
        /// by <see cref="JsonValue"/> and deriving classes.
        /// </summary>
        /// <param name="model">The .NET object to read from.</param>
        /// <returns>The resulting <see cref="JsonValue"/>.</returns>
        public static JsonValue FromModel(object model)
        {
            if (model == null)
                return new JsonNull();

            var consumer = new JsonValueJsonConsumer();
            IConverter converter = ConverterRegistry.Get(model.GetType());
            converter.Write(model, consumer);
            return consumer.Result;
        }

        /// <summary>
        /// Converts this generic object model to a JSON string with indentation.
        /// </summary>
        /// <returns>The JSON string.</returns>
        public override string ToString()
        {
            return ToString(true);
        }
    }
}
