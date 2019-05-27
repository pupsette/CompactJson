using System;

namespace CompactJson
{
    /// <summary>
    /// An interface for converter factories. Implementing classes may be
    /// used together with the <see cref="CustomConverterAttribute"/>.
    /// </summary>
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
        /// <param name="parameters">The <see cref="ConverterParameters"/> This is 
        /// determined by the attributes of the reflected property or field.</param>
        /// <returns>An instance of <see cref="IConverter"/> which is capable
        /// of converting objects of the given type.</returns>
        IConverter Create(Type type, ConverterParameters parameters);

        /// <summary>
        /// Checks, whether this converter factory is capable of converting the
        /// given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true, if this converter factory can create a converter for
        /// the given type; false, otherwise.</returns>
        bool CanConvert(Type type);
    }
}
