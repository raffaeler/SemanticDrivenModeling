using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using ConversionLibrary;
using ConversionLibrary.Converters;

using SemanticLibrary;

/*
    // methods exposed from the Utf8JsonReader
    bool GetBoolean();
    byte GetByte();
    sbyte GetSByte();

    DateTime GetDateTime();
    DateTimeOffset GetDateTimeOffset();

    decimal GetDecimal();
    float GetSingle();
    double GetDouble();

    short GetInt16();
    int GetInt32();
    long GetInt64();
    ushort GetUInt16();
    uint GetUInt32();
    ulong GetUInt64();

    Guid GetGuid();
    string? GetString();

    byte[] GetBytesFromBase64();
    string GetComment();
*/

namespace CodeGenerationLibrary.Serialization
{
    public class ConversionGenerator
    {
        private Dictionary<string, Type> _basicTypes;
        private Dictionary<string, MethodInfo> _readerGetValue;
        private Dictionary<string, Type> _converterTypes;
        private ConversionEngine _conversionEngine;
        private Dictionary<string, SetPropertyDelegate> _cacheSetProperty;
        private Dictionary<string, GetConvertedValueDelegate> _cacheGetValue;
        private PropertyInfo _readerTokenType;

        public delegate void SetPropertyDelegate(ref Utf8JsonReader reader, object instance);
        public delegate object GetConvertedValueDelegate(ref Utf8JsonReader reader);

        public ConversionGenerator(ConversionContext conversionContext = null)
        {
            _conversionEngine = new ConversionEngine(conversionContext);

            _basicTypes = new Dictionary<string, Type>()
            {
                { "Boolean",        typeof(Boolean)            },
                { "Byte",           typeof(Byte)               },
                { "SByte",          typeof(SByte)              },

                { "DateTime",       typeof(DateTime)           },
                { "DateTimeOffset", typeof(DateTimeOffset)     },

                { "Decimal",        typeof(Decimal)            },
                { "Single",         typeof(Single)             },
                { "Double",         typeof(Double)             },

                { "Int16",          typeof(Int16)              },
                { "Int32",          typeof(Int32)              },
                { "Int64",          typeof(Int64)              },
                { "UInt16",         typeof(UInt16)             },
                { "UInt32",         typeof(UInt32)             },
                { "UInt64",         typeof(UInt64)             },

                { "String",         typeof(String)             },
                { "Guid",           typeof(Guid)               },
            };

            var reader = typeof(Utf8JsonReader);

            _readerTokenType = reader.GetProperty("TokenType");
            _readerGetValue = new Dictionary<string, MethodInfo>()
            {
                { "Boolean",            reader.GetMethod("GetBoolean")          },
                { "Byte",               reader.GetMethod("GetByte")             },
                { "SByte",              reader.GetMethod("GetSByte")            },

                { "DateTime",           reader.GetMethod("GetDateTime")         },
                { "DateTimeOffset",     reader.GetMethod("GetDateTimeOffset")   },

                { "Decimal",            reader.GetMethod("GetDecimal")          },
                { "Single",             reader.GetMethod("GetSingle")           },
                { "Double",             reader.GetMethod("GetDouble")           },

                { "Int16",              reader.GetMethod("GetInt16")            },
                { "Int32",              reader.GetMethod("GetInt32")            },
                { "Int64",              reader.GetMethod("GetInt64")            },
                { "UInt16",             reader.GetMethod("GetUInt16")           },
                { "UInt32",             reader.GetMethod("GetUInt32")           },
                { "UInt64",             reader.GetMethod("GetUInt64")           },

                { "String",             reader.GetMethod("GetString")           },
                { "Guid",               reader.GetMethod("GetGuid")             },
            };

            _converterTypes = new Dictionary<string, Type>()
            {
                { "Boolean",            typeof(ToBooleanConversion)            },
                { "Byte",               typeof(ToByteConversion)               },
                { "SByte",              typeof(ToSByteConversion)              },

                { "DateTime",           typeof(ToDateTimeConversion)           },
                { "DateTimeOffset",     typeof(ToDateTimeOffsetConversion)     },

                { "Decimal",            typeof(ToDecimalConversion)            },
                { "Single",             typeof(ToSingleConversion)             },
                { "Double",             typeof(ToDoubleConversion)             },

                { "Int16",              typeof(ToInt16Conversion)              },
                { "Int32",              typeof(ToInt32Conversion)              },
                { "Int64",              typeof(ToInt64Conversion)              },
                { "UInt16",             typeof(ToUInt16Conversion)             },
                { "UInt32",             typeof(ToUInt32Conversion)             },
                { "UInt64",             typeof(ToUInt64Conversion)             },

                { "String",             typeof(ToStringConversion)             },
                { "Guid",               typeof(ToGuidConversion)               },
            };

            _cacheSetProperty = new();
            _cacheGetValue = new();
        }

        public SetPropertyDelegate GetConverter(ScoredPropertyMapping<ModelNavigationNode> nodeMapping)
        {
            var path = nodeMapping.Target.GetObjectMapPath() + $".{nodeMapping.Target.ModelPropertyNode.Property.Name}";
            if (!_cacheSetProperty.TryGetValue(path, out var lambda))
            {
                var expression = GenerateConversion(
                    nodeMapping.Source.ModelPropertyNode.PropertyTypeName,
                    nodeMapping.Target.ModelPropertyNode.Parent.Type,
                    nodeMapping.Target.ModelPropertyNode.PropertyTypeName,
                    nodeMapping.Target.ModelPropertyNode.PropertyName);
                lambda = expression.Compile();
            }

            return lambda;
        }

        private Expression<SetPropertyDelegate> GenerateConversion(string sourceTypeName, Type targetInstanceType, string targetPropertyTypeName,
            string propertyName)
        {
            if (!_basicTypes.TryGetValue(sourceTypeName, out Type sourceType))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for converting data in a json source");
            }

            if (!_basicTypes.TryGetValue(targetPropertyTypeName, out Type targetPropertyType))
            {
                throw new ArgumentException($"The type {targetPropertyTypeName} is not valid for converting data in a json source");
            }

            if (!_readerGetValue.TryGetValue(sourceTypeName, out MethodInfo readerMethod))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for reading data from a json source");
            }

            if (!_conversionEngine.ConversionStrings.TryGetValue(targetPropertyTypeName, out IConversion targetConverter))
            {
                throw new ArgumentException($"There is no converter instance for the target type {targetPropertyTypeName}");
            }

            if (!_converterTypes.TryGetValue(targetPropertyTypeName, out Type converterType))
            {
                throw new ArgumentException($"There is no converter type for the target type {targetPropertyTypeName}");
            }

            var readerInputParameter = Expression.Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");
            var converterInstance = Expression.Constant(targetConverter);
            var castedConverterInstance = Expression.Convert(converterInstance, converterType);
            var fromMethod = converterType.GetMethod("From", new Type[] { sourceType });
            var inputObject = Expression.Parameter(typeof(object), "inputObject");
            var castedInputObject = Expression.Convert(inputObject, targetInstanceType);

            var readerCall = Expression.Call(readerInputParameter, readerMethod);
            var conversionCall = Expression.Call(castedConverterInstance, fromMethod, readerCall);

            // if JsonTokenType is null, I assign the default(targetPropertyType) to the property
            var ternaryIf = Expression.Condition(
                Expression.MakeBinary(ExpressionType.Equal,
                    Expression.MakeMemberAccess(readerInputParameter, _readerTokenType),Expression.Constant(JsonTokenType.Null)),
                Expression.Default(targetPropertyType),
                conversionCall);

            var propertyAssignment = Expression.Assign(
                Expression.Property(castedInputObject, propertyName),
                ternaryIf);

            var lambda = Expression.Lambda<SetPropertyDelegate>(propertyAssignment, readerInputParameter, inputObject);
            return lambda;
        }

        public GetConvertedValueDelegate GetValueConverter(ScoredPropertyMapping<ModelNavigationNode> nodeMapping)
        {
            var path = nodeMapping.Target.GetObjectMapPath() + $".{nodeMapping.Target.ModelPropertyNode.Property.Name}";
            if (!_cacheGetValue.TryGetValue(path, out var lambda))
            {
                var expression = GenerateValueConversion(
                    nodeMapping.Source.ModelPropertyNode.PropertyTypeName,
                    nodeMapping.Target.ModelPropertyNode.PropertyTypeName);
                lambda = expression.Compile();
            }

            return lambda;
        }

        private Expression<GetConvertedValueDelegate> GenerateValueConversion(string sourceTypeName, string targetPropertyTypeName)
        {
            if (!_basicTypes.TryGetValue(sourceTypeName, out Type sourceType))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for converting data in a json source");
            }

            if (!_basicTypes.TryGetValue(targetPropertyTypeName, out Type targetPropertyType))
            {
                throw new ArgumentException($"The type {targetPropertyTypeName} is not valid for converting data in a json source");
            }

            if (!_readerGetValue.TryGetValue(sourceTypeName, out MethodInfo readerMethod))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for reading data from a json source");
            }

            if (!_conversionEngine.ConversionStrings.TryGetValue(targetPropertyTypeName, out IConversion targetConverter))
            {
                throw new ArgumentException($"There is no converter instance for the target type {targetPropertyTypeName}");
            }

            if (!_converterTypes.TryGetValue(targetPropertyTypeName, out Type converterType))
            {
                throw new ArgumentException($"There is no converter type for the target type {targetPropertyTypeName}");
            }

            var readerInputParameter = Expression.Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");
            var converterInstance = Expression.Constant(targetConverter);
            var castedConverterInstance = Expression.Convert(converterInstance, converterType);
            var fromMethod = converterType.GetMethod("From", new Type[] { sourceType });

            var readerCall = Expression.Call(readerInputParameter, readerMethod);
            var conversionCall = Expression.Call(castedConverterInstance, fromMethod, readerCall);

            // if JsonTokenType is null, I assign the default(targetPropertyType) to the property
            var ternaryIf = Expression.Condition(
                Expression.MakeBinary(ExpressionType.Equal,
                    Expression.MakeMemberAccess(readerInputParameter, _readerTokenType), Expression.Constant(JsonTokenType.Null)),
                Expression.Default(targetPropertyType),
                conversionCall);

            var resultCast = Expression.Convert(ternaryIf, typeof(object));

            var lambda = Expression.Lambda<GetConvertedValueDelegate>(resultCast, readerInputParameter);
            return lambda;
        }

        //public void x(ref Utf8JsonReader reader)
        //{
        //    UInt16 val = reader.GetUInt16();
        //    IConversion conv = _conversionEngine.ConversionStrings["UInt16"];
        //    ToUInt64Conversion castedConverter = (ToUInt64Conversion)_conversionEngine.ConversionStrings["UInt64"];
        //    castedConverter.From(val);
        //}


    }
}
