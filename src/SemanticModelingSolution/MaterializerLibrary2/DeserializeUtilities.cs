using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    public class DeserializeUtilities<T>
    {
        private ParameterExpression _reader;
        private ParameterExpression _typeToConvert;
        private ParameterExpression _options;

        private ParameterExpression _isFinished;
        private ParameterExpression _returnValue;

        private readonly Type _targetType;
        private LabelTarget _whileContinue;
        private readonly ConversionGenerator _conversionGenerator;
        public delegate T ReadDelegate(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        private Expression _startArray = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("StartArray").Single());
        private Expression _endArray = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("EndArray").Single());
        private Expression _startObject = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("StartObject").Single());
        private Expression _endObject = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("EndObject").Single());
        private Expression _propertyName = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("PropertyName").Single());
        private Expression _string = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("String").Single());
        private Expression _number = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("Number").Single());
        private Expression _null = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("Null").Single());
        private Expression _true = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("True").Single());
        private Expression _false = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("False").Single());
        private Expression _none = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("None").Single());
        private Expression _comment = Expression.MakeMemberAccess(null, typeof(JsonTokenType).GetMember("Comment").Single());

        private MethodInfo _debugWriteLine = typeof(Debug).GetMethod("WriteLine", new Type[] { typeof(string) });

        public DeserializeUtilities(ConversionGenerator conversionGenerator)
        {
            _targetType = typeof(T);

            _reader = Expression.Parameter(typeof(Utf8JsonReader).MakeByRefType(), "reader");
            _typeToConvert = Expression.Parameter(typeof(Type), "typeToConvert");
            _options = Expression.Parameter(typeof(JsonSerializerOptions), "options");
            _returnValue = Expression.Parameter(_targetType, "result");

            _whileContinue = Expression.Label();
            _isFinished = Expression.Variable(typeof(bool), "isFinished");

            _conversionGenerator = conversionGenerator;
        }

        public Expression<ReadDelegate> CreateExpression()
        {
            var cases = new SwitchCase[]
            {
                CreateCaseStartArray(),
                CreateCaseEndArray(),
                CreateCaseStartObject(),
                CreateCaseEndObject(),
                CreateCasePropertyName(),
                CreateCaseString(),
                CreateCaseNumber(),
                CreateCaseNull(),
                CreateCaseTrue(),
                CreateCaseFalse(),
                //CreateCaseNone(),
                //CreateCaseComment(),
            };

            var switchCase = CreateSwitchCase(cases);
            var exp = CreateLambda(switchCase);
            return exp;
        }

        private Expression CreateDebugWriteLine(string message) => Expression.Call(null, _debugWriteLine, Expression.Constant(message));

        /// <summary>
        /// The reader needs to consume the json stream even if the content is trashed
        /// This must be called every time the json token cannot be consumed
        /// </summary>
        private Expression CreateReaderSkip() => Expression.Call(_reader, _conversionGenerator.ReaderSkip);

        /// <summary>
        /// Creates the switch/case statement with a default block skipping the current token
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        private SwitchExpression CreateSwitchCase(SwitchCase[] cases)
        {
            var tokenType = Expression.MakeMemberAccess(_reader, _conversionGenerator.ReaderTokenType);
            var defaultBlock = Expression.Block(
                CreateReaderSkip()
                //, Expression.Assign(_isFinished, Expression.Constant(true))
                );

            return Expression.Switch(tokenType, defaultBlock, cases);
        }


        /// <summary>
        /// Creates the lambda with the exit condition (isFinished variable)
        /// Returns the deserialized root object in _returnValue
        /// </summary>
        /// <param name="switchCaseBlock"></param>
        /// <returns></returns>
        private Expression<ReadDelegate> CreateLambda(Expression switchCaseBlock)
        {
            var whileBreak = Expression.Label(_targetType, "exitLoop");
            var loopBody = Expression.Block(
                switchCaseBlock,
                Expression.IfThen(
                    Expression.OrElse(_isFinished, Expression.Not(Expression.Call(_reader, _conversionGenerator.ReaderRead))),
                    Expression.Break(whileBreak, _returnValue))
                );
            var loop = Expression.Loop(loopBody, whileBreak, _whileContinue);

            var outerBlock = Expression.Block(_targetType,
                new ParameterExpression[] { _isFinished, _returnValue },
                //Expression.Assign(_isFinished, Expression.Constant(false)),
                loop, _returnValue);

            return Expression.Lambda<ReadDelegate>(outerBlock, _reader, _typeToConvert, _options);
        }

        private SwitchCase CreateCaseStartArray()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("StartArray"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _startArray);
        }

        private SwitchCase CreateCaseEndArray()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("EndArray"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _endArray);
        }

        private SwitchCase CreateCaseStartObject()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("StartObject"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _startObject);
        }

        private SwitchCase CreateCaseEndObject()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("EndObject"),
                Expression.Assign(_isFinished, Expression.Constant(true)),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _endObject);
        }

        private SwitchCase CreateCasePropertyName()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("PropertyName"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _propertyName);
        }

        private SwitchCase CreateCaseString()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("String"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _string);
        }

        private SwitchCase CreateCaseNumber()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("Number"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _number);
        }

        private SwitchCase CreateCaseNull()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("Null"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _null);
        }

        private SwitchCase CreateCaseTrue()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("True"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _true);
        }

        private SwitchCase CreateCaseFalse()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("False"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _false);
        }

        private SwitchCase CreateCaseNone()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("None"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _none);
        }

        private SwitchCase CreateCaseComment()
        {
            var body = Expression.Block(
                CreateDebugWriteLine("Comment"),
                Expression.Empty()
                );
            return Expression.SwitchCase(body, _comment);
        }
    }
}








