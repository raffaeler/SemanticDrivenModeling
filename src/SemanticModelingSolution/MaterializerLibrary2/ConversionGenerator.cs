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
using SurrogateLibrary;

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

namespace MaterializerLibrary
{
    public class ConversionGenerator
    {
        private Dictionary<string, Type> _basicTypes;
        private Dictionary<string, MethodInfo> _readerGetValue;
        private Dictionary<string, Type> _converterTypes;
        private ConversionEngine _conversionEngine;
        private Dictionary<string, SetPropertiesDelegate> _cacheSetProperty;
        private Dictionary<string, GetConvertedValueDelegate> _cacheGetValue;
        private PropertyInfo _readerTokenType;
        private MethodInfo _readerSkipMethodInfo;
        private MethodInfo _readerReadMethodInfo;

        private Type[] _jsonNumericTypes = new Type[]
        {
            typeof(decimal), typeof(double), typeof(float),
            typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64)
        };

        private string[] _jsonNumericTypeStrings = new string[]
        {
            "Decimal", "Double", "Single",
            "Int32", "UInt32", "Int64", "UInt64"
        };

        public delegate void SetPropertiesDelegate(ref Utf8JsonReader reader, IContainer[] instances);
        public delegate object GetConvertedValueDelegate(ref Utf8JsonReader reader);

        public PropertyInfo ReaderTokenType => _readerTokenType;
        public MethodInfo ReaderSkip => _readerSkipMethodInfo;
        public MethodInfo ReaderRead => _readerReadMethodInfo;
        public Dictionary<string, MethodInfo> ReaderGetValue => _readerGetValue;

        public ConversionGenerator(/*TypeSystem<Metadata> typeSystem, */ConversionContext conversionContext = null)
        {
            //_typeSystem = typeSystem;
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
            _readerSkipMethodInfo = reader.GetMethod("Skip");
            _readerReadMethodInfo = reader.GetMethod("Read");

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

        /// <summary>
        /// Serialization
        /// </summary>
        public Expression GetJsonConversionExpression(NavigationPair nodeMapping,
            Expression sourceExpression)
        {
            var source = nodeMapping.Source.GetLeaf();
            var target = nodeMapping.Target.GetLeaf();
            var sourceType = source.Property.PropertyType.GetOriginalType();
            var targetType = target.Property.PropertyType.GetOriginalType();

            if (targetType.Name == "String" ||
                targetType.Name == "Boolean")
            {
                return CreateConvertedExpression(_conversionEngine.ConversionStrings[targetType.Name], sourceType, targetType, sourceExpression);
            }

            if (targetType.Name == "Guid" ||
                targetType.Name == "DateTime" ||
                targetType.Name == "DateTimeOffset")
            {
                return CreateConvertedExpression(_conversionEngine.ConversionStrings[targetType.Name], sourceType, targetType, sourceExpression);
            }

            if (targetType.Name == "TimeSpan")
            {
                var step1 = CreateConvertedExpression(_conversionEngine.ConversionStrings[targetType.Name], sourceType, targetType, sourceExpression);
                var tostring = CreateConvertedExpression(_conversionEngine.ConversionStrings["String"], step1.Type, typeof(string), step1);
                return tostring;
            }

            if (targetType.Name == "Byte" ||
                targetType.Name == "SByte" ||
                targetType.Name == "Int16" ||
                targetType.Name == "UInt16")
            {
                var step1 = CreateConvertedExpression(_conversionEngine.ConversionStrings[targetType.Name], sourceType, targetType, sourceExpression);
                var toint = Expression.Convert(step1, typeof(int));
                return toint;
            }

            if (targetType.Name == "Int32" || targetType.Name == "UInt32" ||
                targetType.Name == "Int64" || targetType.Name == "UInt64" ||
                targetType.Name == "Decimal" || targetType.Name == "Double" || targetType.Name == "Single")
            {
                return CreateConvertedExpression(_conversionEngine.ConversionStrings[targetType.Name], sourceType, targetType, sourceExpression);
            }

            throw new Exception($"Unsupported basic type: {targetType.Namespace}.{targetType.Name}");
        }

        /// <summary>
        /// Serialization
        /// </summary>
        private Expression CreateConvertedExpression(IConversion converter, Type sourceType, Type targetType, Expression sourceExpression)
        {
            if (!_converterTypes.TryGetValue(targetType.Name, out var converterType))
            {
                throw new Exception($"Missing converter for type {sourceType.Name}");
            }

            var converterInstance = Expression.Constant(converter);
            var castedConverterInstance = converterInstance;// Expression.Convert(converterInstance, converterType);

            var fromMethod = converterType.GetMethod("From", new Type[] { sourceType });
            var conversionCall = Expression.Call(castedConverterInstance, fromMethod, sourceExpression);
            return conversionCall;
        }

        /// <summary>
        /// Deserialization
        /// </summary>
        public SetPropertiesDelegate GetConverterMultiple(string sourcePath, IReadOnlyCollection<NavigationPair> nodeMappings)
        {
            if (!_cacheSetProperty.TryGetValue(sourcePath, out var lambda))
            {
                var expression = GenerateConversionMultiple(nodeMappings);
                lambda = expression.Compile();
                _cacheSetProperty[sourcePath] = lambda;
            }

            return lambda;
        }

        private Expression<SetPropertiesDelegate> GenerateConversionMultiple(IReadOnlyCollection<NavigationPair> nodeMappings)
        {
            if (nodeMappings == null) throw new ArgumentNullException(nameof(nodeMappings));
            if (nodeMappings.Count == 0) throw new ArgumentException(nameof(nodeMappings));

            var sourceTypeName = nodeMappings.First().Source.GetLeaf().Property.PropertyType.Name;
            if (!_basicTypes.TryGetValue(sourceTypeName, out Type sourceType))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for converting data in a json source");
            }

            if (!_readerGetValue.TryGetValue(sourceTypeName, out MethodInfo readerMethod))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for reading data from a json source");
            }

            var inputInstances = Expression.Parameter(typeof(IContainer[]), "inputInstancesArray");

            List<Expression> nullStatements = new();
            List<Expression> valueStatements = new();

            var readerInputParameter = Expression.Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");

            // DateTimeOffset val = reader.GetDateTimeOffset();
            var valueFromReader = Expression.Variable(sourceType, "val");
            var readerCall = Expression.Assign(valueFromReader, Expression.Call(readerInputParameter, readerMethod));
            valueStatements.Add(readerCall);

            int i = 0;
            foreach (var nodeMapping in nodeMappings)
            {
                Type targetInstanceType = nodeMapping.Target.GetLeaf().Property.OwnerType.GetOriginalType();
                string targetPropertyTypeName = nodeMapping.Target.GetLeaf().Property.PropertyType.Name;
                string propertyName = nodeMapping.Target.GetLeaf().Property.Name;

                if (!_basicTypes.TryGetValue(targetPropertyTypeName, out Type targetPropertyType))
                {
                    throw new ArgumentException($"The type {targetPropertyTypeName} is not valid for converting data in a json source");
                }

                if (!_conversionEngine.ConversionStrings.TryGetValue(targetPropertyTypeName, out IConversion targetConverter))
                {
                    throw new ArgumentException($"There is no converter instance for the target type {targetPropertyTypeName}");
                }

                if (!_converterTypes.TryGetValue(targetPropertyTypeName, out Type converterType))
                {
                    throw new ArgumentException($"There is no converter type for the target type {targetPropertyTypeName}");
                }

                nullStatements.Add(CreateAssignFromStatement(i, inputInstances, null,
                    sourceType, targetInstanceType, propertyName, targetPropertyType, targetConverter, converterType));
                valueStatements.Add(CreateAssignFromStatement(i, inputInstances, valueFromReader,
                    sourceType, targetInstanceType, propertyName, targetPropertyType, targetConverter, converterType));
                i++;
            }

            nullStatements.Add(CreateReaderSkip(readerInputParameter));

            var blockNull = Expression.Block(nullStatements);
            var blockValue = Expression.Block(new ParameterExpression[] { valueFromReader }, valueStatements);



            // if JsonTokenType is null, I assign the default(targetPropertyType) to the property
            var IfCondition = Expression.IfThenElse(
                Expression.MakeBinary(ExpressionType.Equal,
                    Expression.MakeMemberAccess(readerInputParameter, _readerTokenType), Expression.Constant(JsonTokenType.Null)),
                blockNull,
                blockValue);

            var lambda = Expression.Lambda<SetPropertiesDelegate>(IfCondition, readerInputParameter, inputInstances);
            return lambda;
        }

        /// <summary>
        /// Generates a statement like this one:
        /// ((Article)instances[i++]).ExpirationDate = ((ToDateTimeConversion)_conversion).FromNull();
        /// ((Article)instances[i++]).ExpirationDate = ((ToDateTimeConversion)_conversion).From(val);
        /// 
        /// where "i" is the index and must have the same order as the input instance array object[]
        /// </summary>
        private Expression CreateAssignFromStatement(int index,
            ParameterExpression inputInstancesArray,
            ParameterExpression fromMethodArgument, Type sourceType,
            Type targetInstanceType, string propertyName,
            Type targetPropertyType, IConversion targetConverter, Type converterType)
        {
            MethodInfo fromMethod = fromMethodArgument == null
                ? converterType.GetMethod("FromNull")
                : converterType.GetMethod("From", new Type[] { sourceType });

            //((Article)((Container<Article>)instances[i++]).Item).ExpirationDate
            var typedContainerType = typeof(Container<>).MakeGenericType(targetInstanceType);
            var itemProperty = typedContainerType.GetProperty("Item");
            var left = Expression.Property(
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Convert(
                            Expression.ArrayIndex(inputInstancesArray, Expression.Constant(index)),
                                typedContainerType), itemProperty), targetInstanceType),
                propertyName);

            // ((ToDateTimeConversion)_conversion).FromNull()
            // or
            // ((ToDateTimeOffsetConversion)_conversion).From(val);
            var converterInstance = Expression.Constant(targetConverter);   // _conversion variable
            var castedConverterInstance = Expression.Convert(converterInstance, converterType);
            var right = fromMethodArgument == null
                ? Expression.Call(castedConverterInstance, fromMethod)
                : Expression.Call(castedConverterInstance, fromMethod, fromMethodArgument);

            return Expression.Assign(left, right);
        }

        private Expression CreateReaderSkip(ParameterExpression readerInput)
            => Expression.Call(readerInput, _readerSkipMethodInfo);


        // if it is used in serialization is GetLeafPath
        // in deserialization is GetLeafPathAlt
        public SetPropertiesDelegate GetConverter(NavigationPair nodeMapping)
        {
            var path = nodeMapping.Target.GetLeafPathAlt() + $".{nodeMapping.Target.Property.Name}";
            if (!_cacheSetProperty.TryGetValue(path, out var lambda))
            {
                var expression = GenerateConversion(
                    nodeMapping.Source.Property.PropertyType.Name,
                    nodeMapping.Target.Property.OwnerType.GetOriginalType(),
                    nodeMapping.Target.Property.PropertyType.Name,
                    nodeMapping.Target.Property.Name);
                lambda = expression.Compile();
            }

            return lambda;
        }

        private Expression<SetPropertiesDelegate> GenerateConversion(string sourceTypeName,
            Type targetInstanceType, string targetPropertyTypeName, string propertyName)
        {
            if (!_basicTypes.TryGetValue(sourceTypeName, out Type sourceType))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for converting data in a json source");
            }

            if (!_readerGetValue.TryGetValue(sourceTypeName, out MethodInfo readerMethod))
            {
                throw new ArgumentException($"The type {sourceTypeName} is not valid for reading data from a json source");
            }

            if (!_basicTypes.TryGetValue(targetPropertyTypeName, out Type targetPropertyType))
            {
                throw new ArgumentException($"The type {targetPropertyTypeName} is not valid for converting data in a json source");
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
                    Expression.MakeMemberAccess(readerInputParameter, _readerTokenType), Expression.Constant(JsonTokenType.Null)),
                Expression.Default(targetPropertyType),
                conversionCall);

            var propertyAssignment = Expression.Assign(
                Expression.Property(castedInputObject, propertyName),
                ternaryIf);

            var lambda = Expression.Lambda<SetPropertiesDelegate>(propertyAssignment, readerInputParameter, inputObject);
            return lambda;
        }


        // verify pathalt
        public GetConvertedValueDelegate GetValueConverter(NavigationPair nodeMapping)
        {
            var path = nodeMapping.Target.GetLeafPathAlt() + $".{nodeMapping.Target.Property.Name}";
            if (!_cacheGetValue.TryGetValue(path, out var lambda))
            {
                var expression = GenerateValueConversion(
                    nodeMapping.Source.Property.PropertyType.Name,
                    nodeMapping.Target.Property.PropertyType.Name);
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

            // read the json into a suitable type (decided from the sourceTypeName)
            var readerInputParameter = Expression.Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");
            var readerCall = Expression.Call(readerInputParameter, readerMethod);

            var converterInstance = Expression.Constant(targetConverter);
            var castedConverterInstance = Expression.Convert(converterInstance, converterType);

            var fromMethod = converterType.GetMethod("From", new Type[] { sourceType });
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


        // vNext
        private void GeneratedCodeExample(ref Utf8JsonReader reader, object[] instances)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                ((Article)instances[0]).ExpirationDate = ((ToDateTimeConversion)_conversion).FromNull();
                ((Order)instances[1]).Date = ((ToDateTimeOffsetConversion)_conversion).FromNull();

                reader.Skip();
            }
            else
            {
                DateTimeOffset val = reader.GetDateTimeOffset();

                ((Article)instances[0]).ExpirationDate = ((ToDateTimeConversion)_conversion).From(val);
                ((Order)instances[1]).Date = ((ToDateTimeOffsetConversion)_conversion).From(val);
            }
        }


        private class Article
        {
            public DateTime ExpirationDate { get; set; }
        }

        private class Order
        {
            public DateTimeOffset Date { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier",
            Justification = "Sample Code to generate")]
        private IConversion _conversion = null;

        // current
        private void Func(ref Utf8JsonReader reader, Object inputObject)
        {
            ((Article)inputObject).ExpirationDate = reader.TokenType == JsonTokenType.Null
                ? default(DateTime)
                : ((ToDateTimeConversion)_conversion).From(reader.GetDateTimeOffset());
        }

    }
}
