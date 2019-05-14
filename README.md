# CompactJson
Fast lightweight JSON serializer for .NET. Distributed as .NET library and C# source code.

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

## Supported Types

TODO