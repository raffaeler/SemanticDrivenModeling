using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T>
    {
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _targetLookup = new();
        //private Stack<ICodeGenerationContext> _codeGenContext;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            //Console.WriteLine($"TesterConverter.Write> ");
            //OneToManyContext.Reset();   // move to ctor
            //_codeGenContext = new();
            //_codeGenContext.Push(new SimpleContext(typeof(T)));
            //ParameterExpression inputWriter = Expression.Parameter(typeof(Utf8JsonWriter), "writer");

            var visitor = new SemanticSerializationVisitor<T>(_targetLookup, _conversionGenerator, _map);
            visitor.Visit(_map.TargetModelTypeNode);
            var expression = visitor.GetSerializationAction();
            var transformDelegate = expression.Compile();
            transformDelegate(writer, value);

/*
            var visitor = new ModelTypeNodeVisitor();
            visitor.Visit(_map.TargetModelTypeNode, (modelTypeNode, path) =>
            {
                Console.WriteLine($"********> {path} [{modelTypeNode.Type.Name}]");
                //writer.WriteStartObject();
                var currentContext = _codeGenContext.Peek();
                var writeExpression = GeneratorUtilities.JsonWriteStartObject(inputWriter);
                currentContext.Statements.Add(writeExpression);
            },
            (modelTypeNode, path) =>
            {
                //writer.WriteEndObject();
                var currentContext = _codeGenContext.Peek();
                var writeExpression = GeneratorUtilities.JsonWriteEndObject(inputWriter);
                currentContext.Statements.Add(writeExpression);
            },
            (modelPropertyNode, path) =>
            {
                if (modelPropertyNode.PropertyKind.IsOneToOne())
                {
                    // one-to-one
                    Console.WriteLine("1-1");
                    //writer.WritePropertyName(modelPropertyNode.Name);
                    var currentContext1 = _codeGenContext.Peek();
                    var writeExpression1 = GeneratorUtilities.JsonWritePropertyName(inputWriter, modelPropertyNode.Name);
                    currentContext1.Statements.Add(writeExpression1);

                    return;
                }
                else if (modelPropertyNode.PropertyKind.IsOneToMany())
                {
                    // collection of something
                    // already handled inside the collection callbacks
                    return;
                }

                if (!_targetLookup.TryGetValue(path, out var scoredPropertyMapping))
                {
                    Console.WriteLine($"PathProp> {path} unmapped");
                    return;
                }

                var sourcePath = scoredPropertyMapping.Source.GetMapPath();
                Console.Write($"PathProp> {path} <== Source: {sourcePath}   |   ");

                //var expressions = GeneratorUtilities.CreateGetValue<T>(scoredPropertyMapping.Source);

                // look for the the parent needed to access the chain done with 1-1 and/or basic types 
                var tempNav = scoredPropertyMapping.Source;
                ModelNavigationNode relativeRoot = null;
                SurrogateType relativeRootType = null;
                while (true)
                {
                    if (tempNav.Previous == null ||
                        (tempNav.Previous != null && tempNav.Previous.ModelPropertyNode.PropertyKind.IsOneToMany()))
                    {
                        relativeRoot = tempNav;
                        relativeRootType = tempNav.ModelPropertyNode.Parent.Type;
                        break;
                    }

                    tempNav = tempNav.Previous;
                }

                // get the first context whose type matches with the one found above
                ICodeGenerationContext context = null;
                foreach (var ctx in _codeGenContext)
                {
                    if (ctx.Variable.Type == relativeRootType.GetOriginalType())
                    {
                        context = ctx;
                        break;
                    }
                }

                //Debug.Assert(context != null);
                if (context == null)
                {
                    // context is null when
                    // source object is inside a collection
                    // target object is outside the collection
                    // since there is no "for loop" in the target, we have to build a special expression:
                    // - get the first element of the collection and use it to extract the value to write in the target
                    // It makes no sense to repeat the operation for all the source element of the collection
                    // because they will just overwrite the previous
                    // Example:
                    // Target: OnlineOrder.Description
                    // Source: Order.OrderItems.$.Article.Description
                    // This property should be applied to the [collection].FirstOrDefault() from the parent context
                    // relativeRoot.ModelPropertyNode.PropertyInfo
                    Console.WriteLine("Skipped <special case>");
                    return;
                }

                var currentContext = _codeGenContext.Peek();
                var accessor = GeneratorUtilities.CreateGetValue(context.Variable,
                    scoredPropertyMapping.Source);

                var valueExpression = _conversionGenerator.GetJsonConversionExpression(scoredPropertyMapping, accessor);
                var writeExpression = GeneratorUtilities.JsonWriteValue(inputWriter, modelPropertyNode.Name, valueExpression);
                //var writeExpression = GeneratorUtilities.JsonWriteString(inputWriter,
                //    modelPropertyNode.Name, Expression.Constant(modelPropertyNode.Name));
                currentContext.Statements.Add(writeExpression);

                //writer.WriteString()

                // basic types
                Console.Write("B");
                Console.WriteLine();
            },
            (modelPropertyNode, path) =>
            {
                // begin collection
                //var sourcePath = scoredPropertyMapping.Source.GetMapPath();
                Console.WriteLine($"Start  C> {path} ");
                //writer.WriteStartArray(modelPropertyNode.Name);
                var currentContext = _codeGenContext.Peek();
                var writeExpression = GeneratorUtilities.JsonWriteStartArray(inputWriter, modelPropertyNode.Name);
                currentContext.Statements.Add(writeExpression);

                // create var used in the foreach
                // all the expressions after this must use this as "root"
                // in the end-collection we create the foreach with that variable

                var sourceModelPropertyNode = FindFirstCollectionOnSourceModelPropertyNode(path);
                var context = new OneToManyContext(sourceModelPropertyNode, path);
                _codeGenContext.Push(context);
            },
            (modelPropertyNode, path) =>
            {
                // end collection
                Console.WriteLine($"End    C> {path} ");
                //writer.WriteEndArray();

                var currentContext = _codeGenContext.Pop() as OneToManyContext;
                var outerContext = _codeGenContext.Peek();


                // if the outerContext.Variable has a Type mismatch,
                // the code must be modified to search the correct one in the other contexts
                // as we already did in other parts
                // this may happen with two nested list<>
                var loop = currentContext.CreateForEach(outerContext.Variable);
                outerContext.Statements.Add(loop);

                var writeExpression = GeneratorUtilities.JsonWriteEndArray(inputWriter);
                outerContext.Statements.Add(writeExpression);
            });

            var finalContext = _codeGenContext.Pop() as SimpleContext;
            var transform = finalContext.CreateTransform(inputWriter);
            var transformDelegate = transform.Compile();
            transformDelegate(writer, value);
*/
        }
/*
        private ModelNavigationNode FindFirstCollectionOnSourceModelPropertyNode(string targetPath)
        {
            ModelNavigationNode navigation;
            foreach (var path in _targetLookup.Keys)
            {
                if (path.Contains(targetPath))
                {
                    navigation = _targetLookup[path].Source;
                    while (navigation != null)
                    {
                        if (navigation.ModelPropertyNode.PropertyKind.IsOneToMany())
                        {
                            return navigation;
                        }

                        navigation = navigation.Previous;
                    }
                }
            }

            Debug.Fail($"Cannot find a suitable collection mapped to {targetPath}");
            return null;
        }

        private interface ICodeGenerationContext
        {
            ModelNavigationNode PropertyNode { get; }
            ParameterExpression Variable { get; }
            IList<Expression> Statements { get; }
        }

        private class SimpleContext : ICodeGenerationContext
        {
            public SimpleContext(Type type)
            {
                PropertyNode = null;
                Variable = Expression.Parameter(type);
                Statements = new List<Expression>();
            }

            public ModelNavigationNode PropertyNode { get; }
            public ParameterExpression Variable { get; }
            public IList<Expression> Statements { get; }

            public Expression<Action<Utf8JsonWriter, T>> CreateTransform(ParameterExpression writer)
            {
                var body = Expression.Block(Statements);
                return Expression.Lambda<Action<Utf8JsonWriter, T>>(body, writer, Variable);
            }
        }

        private class OneToManyContext : ICodeGenerationContext
        {
            private static int _id = 0;
            public OneToManyContext(ModelNavigationNode collectionNode, string path)
            {
                PropertyNode = collectionNode;
                Path = path;
                var itemTypeToIterate = PropertyNode.ModelPropertyNode.CoreType.GetOriginalType();
                _id++;
                Variable = Expression.Variable(itemTypeToIterate, $"loopVar{_id}");
                Statements = new List<Expression>();
                BodyVariables = new();
            }

            public ModelNavigationNode PropertyNode { get; }
            public string Path { get; }
            public ParameterExpression Variable { get; }
            public IList<Expression> Statements { get; }
            public List<ParameterExpression> BodyVariables { get; }

            public static void Reset() => _id = 0;

            public Expression CreateForEach(ParameterExpression outerObject)
            {
                var enumerable = GeneratorUtilities.CreateGetValue(outerObject, PropertyNode);
                return GeneratorUtilities.ForEach(enumerable, Variable, CreateBody());
            }

            private BlockExpression CreateBody() => Expression.Block(BodyVariables, Statements);
        }

*/
    }
}
