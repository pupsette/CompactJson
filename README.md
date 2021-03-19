# CompactJson
Fast lightweight JSON serializer for .NET. Distributed as .NET library and C# source code.

## Why another JSON serializer?
This serializer is primarily designed to be embedded into other .NET libraries written in C#. This reduces dependencies to other wide-spread JSON parsing libraries.

## Features & Performance
The supported set of features has been reduced to the very basics by design. For example, you cannot control the order of class members during serialization. Also, CompactJson does not ship converters for `HashSet<T>` or `LinkedList<T>` (you may write your own Converter, of course). The intention is to keep it small and simple.

Performance has been compared to `Newtonsoft.Json` by a very small and simple benchmark, which is part of the source code repository. Execution on an i5-3550 showed the following results:

|               | Newtonsoft.Json (12.0.2) | CompactJson |
| ------------- | ------------- | ---------- |
| Warm-up  | 209ms  | 61ms |
| Serialization (small object graphs, total 406MB)  | 8.5s | 6.1s |
| Serialization (large object graphs, total 445MB)  | 9.1s | 6.8s |
| Deserialization (small object graphs, total 406MB)  | 12.1s | 7.7s |
| Deserialization (large object graphs, total 445MB)  | 14.7s | 11.2s |

As usual: performance measurements have to be interpreted carefully. For details about test execution, have a look at the code.

## Installation
Add either the `CompactJson` or the `CompactJson.Sources` Nuget package to your project. See the [packages on Nuget](https://www.nuget.org/profiles/pupsette).

## Usage
For simple conversions from a JSON string to a .NET object:
```csharp
int[] model = (int[])Serializer.Parse("[1,2,3]", typeof(int[]));
```
or the generic version:
```csharp
int[] model = Serializer.Parse<int[]>("[1,2,3]");
```
and vice-versa:
```csharp
string json = Serializer.ToString(new int[] {1,2,3}, false);
```
where the boolean parameter decides whether to indent (pretty-print) the JSON or not.

There are also overloads for streaming to a `TextWriter` or parsing from a `TextReader`.

When serializing classes and structs the public properties (with public getter *and* setter) and fields will be serialized and deserialized by default. For example:
```csharp
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

Converters are responsible for 'converting' between JSON-like data and .NET objects. You may customize the serialization and deserialization by implementing your own converters. The interface that you need to implement is *IConverter*. It has methods for all the possible JSON input tokens (like *string*, *number*, *array*). In general, custom converters are very specific and so we recommend deriving from *ConverterBase* which refuses all input tokens by default and you have to override the accepted ones.

Here's an example of a converter implementation for the .NET GUID class.
```csharp
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

Once implemented, you may choose to register it globally at the static *ConverterRegistry* or to use it for individual properties by adding the *CustomConverter* attribute.

### Global Converter Registration

The converter may then added to the *ConverterRegistry* like this:
```csharp
ConverterRegistry.AddConverter(new GuidConverter());
```

For more advanced converters, you may want to register a custom implementation of *IConverterFactory*. This basically allows you to write converters for a range of types. For example, this is useful if the type you want to convert is a generic type.

### Property-specific Converter

The converter may be also be defined for a specific property like this:

```csharp
class MyClass
{
    [CustomConverter(typeof(GuidConverter))]
    public Guid ID { get; set; }
}
```

## Supported Types

There are converters already registered for the following types:
* `string`
* `int` / `uint`
* `long` / `ulong`
* `float` ("NaN", "Infinity" and "-Infinity" are encoded as strings)
* `double` ("NaN", "Infinity" and "-Infinity" are encoded as strings)
* `char`
* `bool`
* `DateTime` (see DateTime Formatting below)
* `Guid`
* `byte[]` (base64 encoded)
* `JsonValue` / `JsonObject` / `JsonArray` / `JsonNumber` / `JsonBoolean` / `JsonString` (see 'JSON Object Model' below.
* `List<T>` / `IList<T>`
* `T[]`
* `Dictionary<string, T>` / `IDictionary<string, T>` (maps to a JSON object, where the dictionary keys are JSON properties)
* `Enum` (enumeration values are encoded as string)
* `Nullable<T>` (e.g. if there is a converter for `int`, `int?` will also be supported.)

## Classes and Structs

### Properties and Fields
For converting from JSON to custom .NET classes and back there are a few rules, that are applied to the fields and properties of the custom class. The following members will be included during serialization/deserialization:
1. All public fields
2. All public properties (with getter and setter, getter must be public)
3. Properties and fields with the `[JsonProperty]` attribute

The property or field name is kept as-is. If you want to use a different name when converting to JSON, you have to assign it using the `[JsonProperty("myCustomName")]` attribute.

:heavy_exclamation_mark: Trying to parse into a `readonly` field will throw an exception during deserialization. Also parsing into a property without setter will fail.

Properties and fields may also be excluded from serialization/deserialization by using the `[JsonIgnoreMember]` or `[IgnoreDataMember]` attribute.

Example:
```csharp
class User
{
    [JsonIgnoreMember]
    public Guid InternalId { get; set; }

    [JsonProperty("UserName")]
    public string Name { get; set; }

    public string EMail { get; set; }

    [JsonProperty]
    private string Account;
}
```

### Null and Default Values

During serialization, null values will not be emitted, by default. Default values other than `null` will be emitted (e.g. 0 for an int). If you want to serialize a property or field in any case, you need to add the `[JsonEmitNullValue]` attribute to it. If you don't want to serialize the default value of a property or field, you need to add the `[JsonSuppressDefaultValue]` attribute.

For example, consider the following class:
```csharp
class User
{
    public string Name { get; set; }
    public string EMail { get; set; }
    public string Phone { get; set; }
	public int Age { get; set; }
}
```
If this one gets default-constructed and serialized
```csharp
string json = Serializer.ToString(new User());
```
the resulting JSON will look like this:
```
{"Age":0}
```

Adding the `[JsonEmitNullValue]` and `[JsonSuppressDefaultValue]` attributes changes the behavior.
```csharp
class User
{
    [JsonEmitNullValue]
    public string Name { get; set; }
    public string EMail { get; set; }
    public string Phone { get; set; }
	[JsonSuppressDefaultValue]
	public int Age { get; set; }
}
```
now, serialization yields:
```
{"Name":null}
```

:heavy_exclamation_mark: Note, that _default_ values are the language defined default values, which can be obtained by the `default` keyword in C#. Property and field initialization (e.g. from your constructor) does not define the _default_ value. Also the `[System.ComponentModel.DefaultValue]` attribute is not considered, currently.

### Including Type Information

Often, when deserializing JSON you may want to map JSON objects to .NET classes according to type information in your JSON. Consider the following example:
The JSON probably looks like this:
```json
[
  {
    "Type": "Service",
    "Name": "proxy endpoint config",
    "ServiceName": "Proxy",
    "Endpoint": "http://localhost:9111/proxy"
  },
  {
    "Type": "Log",
    "Name": "proxy log config",
    "LogFolder": "./logs"
  }
]
```

JSON objects don't carry type information, unless it has been added explicitly. In this example we want the deserializer to choose the .NET class according to the `Type` property. In order to do so, we must add attributes for assigning the type names as well as using the `TypedConverterFactory`. Like this:

```csharp
[JsonTypeName(typeof(ServiceConfiguration), "Service")]
[JsonTypeName(typeof(LogConfiguration), "Log")]
[JsonCustomConverter(typeof(TypedConverterFactory), "Type")]
class ConfigurationBase
{
    public string Name { get; set; }
}
class ServiceConfiguration : ConfigurationBase
{
    public string ServiceName { get; set; }
    public string Endpoint { get; set; }
}
class LogConfiguration : ConfigurationBase
{
    public string LogFolder { get; set; }
}
```

All the attributes are added to the base type, which is the one that has to be used when deserializing. In our example:
```csharp
List<ConfigurationBase> configs = Serializer.Parse<List<ConfigurationBase>>(json);
```
Of course, serialization works the same way:
```
string json = Serializer.ToString(configs);
```

The name of the JSON property, which is used to encode the type name can be chosen as a parameter of the `TypedConverterFactory` in the `[JsonCustomConverterAttribute]`. Note, that this property must always appear as first property in the JSON object. Also, the classes themselves should not have a property with the same name.

This example excludes the base type from serialization, because we did not assign a type name. Also there is no reflection involved, trying to determine all sub classes! Only the type name assignments at your base class makes the type visible to the serializer.

If your application determines supported types at run-time you can create a custom converter factory, which creates a `TypedConverter` by passing your own implementation of `ITypeNameResolver`.

One of the `[JsonTypeName]` attributes may set the name parameter to `null` (e.g. `[JsonTypeName(typeof(LogConfiguration), null)]`). This type is then deserialized in case the type property is missing. Also when serializing this type, the type property will not be emitted.

## DateTime Formatting

.NET `DateTime` values will be encoded as JSON string. The serialized DateTime string is ISO 8601 conformant, however
it does not convert all possible ISO 8601 representations to a DateTime object.

Allowed formats (examples):
* 2010-08-22T09:15:00
* 2010-08-22T09:15:00.910
* 2010-08-22T09:15:00Z
* 2010-08-22T09:15:00.910Z
* 2010-08-22T09:15:00+01:30
* 2010-08-22T09:15:00-03:00
* 2010-08-22T09:15:00.911+01:30
* 2010-08-22T09:15:00.911-03:00

When serializing, the milliseconds part will be omitted, if this part is 000. Also the 'Z' indicator will only be appended, if the DateTime object has the DateTimeKind.Utc. The UTC offset (+/-) will only be appended, if the DateTime object has the DateTimeKind.Local. If the DateTime object has DateTimeKind.Unspecified, there will be no suffix appended.

When deserializing, one of the above formats is expected, otherwise an exception is thrown. The resulting DateTime object will have the DateTimeKind set according to the encountered suffix. Keep in mind, that serializer and deserializer might have different UTC offsets. In this case, you cannot expect the deserialization and subsequent serialization to reproduce the input string.

If you want to apply your own custom converter for `DateTime` objects globally, overwrite the registered converter simply by adding it to the static `ConverterRegistry` (make sure, the `Type` property of your converter implementation returns `typeof(DateTime)`). Keep in mind, that the nullable variant `DateTime?` has a dedicated converter!

## JSON Object Model

If you don't want to convert to a specific custom .NET class during deserialization, you can do so by using the classes derived from `JsonValue`. You may also represent a part of a custom .NET class as generic JSON data by adding `JsonValue` or any derived class as property or field to your custom class. For example:
```csharp
class OperationParameter
{
    public string ParameterName { get; set; }
    public JsonValue ParameterValue { get; set; }
}
```

This allows you to inspect the contents of the `ParameterValue` at a later stage after deserialization. Another use-case is to simply pass JSON data from one component to another without actually interpreting it.

Using a more specific sub class of `JsonValue` is understood as a constraint during deserialization. For example using `JsonObject` as type for the `ParameterValue` property will make the serializer throw an exception, if anything but `null` or `{ [...] }` is encountered during deserialization.
