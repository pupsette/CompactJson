# CompactJson
Fast lightweight JSON serializer for .NET. Distributed as .NET library and C# source code.
Embedding the source code of this JSON serializer into your own library reduces dependencies to other wide-spread JSON parsing libraries.

## Usage
For simple conversions from a JSON string to a .NET object:
```
int[] model = (int[])Serializer.Parse("[1,2,3]", typeof(int[]));
```
or the generic version:
```
int[] model = Serializer.Parse<int[]>("[1,2,3]");
```
and vice-versa:
```
string json = Serializer.ToString(new int[] {1,2,3}, false);
```
where the boolean parameter decides whether to indent (pretty-print) the JSON or not.

When serializing classes and structs the public properties (with public getter *and* setter) and fields will be serialized and deserialized by default. For example:
```
enum JobState
{
    Running,
    Completed,
    Aborted
}

class Job
{
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string Description { get; set; }
    public State State { get; set; }
}
```
For more details and options regarding serialization of classes and struct, see the corresponding section below.

## Converters

Converters are responsible for 'converting' between JSON-like data and .NET objects. They are registered globally at the static *ConverterRegistry*. You may customize the serialization and deserialization by adding your own converters. The interface that you need to implement is *IConverter*. It has methods for all the possible JSON input tokens (like *string*, *number*, *array*). In general, custom converters are very specific and so we recommend deriving from *ConverterBase* which refuses all input tokens by default and you have to override the accepted ones.

Here's an example of a converter implementation for the .NET GUID class.
```
class GuidConverter : ConverterBase
{
    public GuidConverter() : base(typeof(Guid))
    {
    }

    public override void Write(object value, IJsonConsumer writer)
    {
        writer.String(((Guid)value).ToString());
    }

    public override object FromString(string value)
    {
        return Guid.Parse(value);
    }
}
```
Note, that this implementation does not accept a JSON *null* value! If you want to allow *null* values, you should use the *NullableConverterBase* instead of *ConverterBase* in order to control the behavior with a constructor parameter.

The converter is then added to the *ConverterRegistry* like this:
```
ConverterRegistry.AddConverter(new GuidConverter());
```

For more advanced converters, you may want to add a custom implementation of *IConverterFactory*. This basically allows you to write converters for a range of types. For example, this is useful if the type you want to convert is a generic type.

## Supported Types

There are converters already registered for the following types:
* `int` / `int?`
* `long` / `long?`
* `float` / `float?`
* `double` / `double?`
* `string`
* `bool`
* `DateTime` / `DateTime?` (see DateTime Formatting below)
* `Guid` / `Guid?`
* `JsonValue` / `JsonObject` / `JsonArray` / `JsonNumber` / `JsonBoolean` / `JsonString`
* `List<T>`
* `T[]`
* `Dictionary<string, T>` (maps to a JSON object, where the dictionary keys are JSON properties)
* `Enum` (enumeration values are currently encoded as string)

## Classes and Structs

TODO

## DateTime Formatting

TODO
