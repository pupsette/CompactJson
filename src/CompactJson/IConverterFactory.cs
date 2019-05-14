using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IConverterFactory
    {
        /// <summary>
        /// Creates a converter for the given type.
        /// </summary>
        /// <param name="type">The type for which to create the converter.</param>
        /// <returns>An instance of <see cref="IConverter"/> which is capable
        /// of converting objects of the given type.</returns>
        IConverter Create(Type type);
    }
}
