using System;

namespace CompactJson
{
    /// <summary>
    /// An interface for initializing a converter after creation. This is
    /// used to solve recursive converter creation.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IConverterInitialization
    {
        /// <summary>
        /// Initializes the converter. This method will be called by converter
        /// factories after creation and before usage.
        /// </summary>
        void InitializeConverter();
    }
}
