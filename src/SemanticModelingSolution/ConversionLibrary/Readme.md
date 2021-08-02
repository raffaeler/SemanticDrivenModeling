# Conversion library

The usage context is the following. The entry point is the deserialization of a json file using `System.Text.Json`.

The possible source basic data types are therefore limited to the following types:

* Boolean
* Byte, SByte
* DateTime, DateTimeOffset
* Decimal
* Single, Double
* Guid
* Int16, Int32, Int64, UInt16, UInt32, UInt64
* String

The JsonConverter may receive one of the following types

* Null: meaning that reference and nullable types being null fall in this case
* True, False: Boolean
* Number: any number type
* String: may receive String, Guid, DateTime, DateTimeOffset or any other non-object, non-array type
* Array and objects are processed by the deserializer appropriately to match the graph

## Interpreting deserialized data

Source metadata is needed to correctly interpret the data inside the json **before** converting to the target data type.

The workflow is therefore:

- If the data is null, ignore the source metadata and invoke the target converter with null
  - Set null for reference types
  - Set default(T) for value types
- If the data is true/false, ignore the source metadata and invoke the target converter with the Boolean value
- For String and Number, deserialize using the source metadata first. Then convert the data into the destination type.















