using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using SemanticLibrary;

using SurrogateLibrary;

namespace MaterializerLibrary
{
    internal class SemanticSerializationVisitor<T> : SurrogateVisitor<Metadata>
    {
        private TypeSystem<Metadata> _typeSystem;
        protected IDictionary<string, NavigationPair> _targetLookup;
        private Stack<ICodeGenerationContext> _codeGenContext;
        private ConversionGenerator _conversionGenerator;
        protected Mapping _map;
        ParameterExpression inputWriter;

        public SemanticSerializationVisitor(TypeSystem<Metadata> typeSystem, IDictionary<string, NavigationPair> targetLookup,
            ConversionGenerator conversionGenerator, Mapping map)
        {
            _typeSystem = typeSystem;
            _targetLookup = targetLookup;
            _conversionGenerator = conversionGenerator;
            _map = map;

            OneToManyContext.Reset();
            _codeGenContext = new();
            _codeGenContext.Push(new SimpleContext(typeof(T), typeof(T).Name));
            inputWriter = Expression.Parameter(typeof(Utf8JsonWriter), "writer");
        }

        public override void OnBeginVisitType(SurrogateType<Metadata> type, string path)
        {
            //Console.WriteLine($"********> {path} [{type.Name}]");
            //writer.WriteStartObject();
            var currentContext = _codeGenContext.Peek();
            var writeExpression = GeneratorUtilities.JsonWriteStartObject(inputWriter);
            currentContext.Statements.Add(writeExpression);
        }

        public override void OnEndVisitType(SurrogateType<Metadata> type, string path)
        {
            //writer.WriteEndObject();
            var currentContext = _codeGenContext.Peek();
            var writeExpression = GeneratorUtilities.JsonWriteEndObject(inputWriter);
            currentContext.Statements.Add(writeExpression);
        }

        public override void OnVisitProperty(SurrogateProperty<Metadata> property, string path)
        {
            var propertyKind = property.GetKind();
            if (propertyKind == PropertyKind.OneToOne)
            {
                // one-to-one
                //Console.WriteLine("1-1");
                //writer.WritePropertyName(modelPropertyNode.Name);
                var currentContext1 = _codeGenContext.Peek();
                var writeExpression1 = GeneratorUtilities.JsonWritePropertyName(inputWriter, property.Name);
                currentContext1.Statements.Add(writeExpression1);

                return;
            }
            else if (propertyKind == PropertyKind.OneToMany ||
                propertyKind == PropertyKind.OneToManyBasicType ||
                propertyKind == PropertyKind.OneToManyEnum)
            {
                // collection of something
                // already handled inside the collection callbacks
                return;
            }

            if (!_targetLookup.TryGetValue(path, out var scoredPropertyMapping))
            {
                //Console.WriteLine($"PathProp> {path} unmapped");
                return;
            }

            var sourcePath = scoredPropertyMapping.Source.GetLeafPath();
            //Console.Write($"PathProp> {path} <== Source: {sourcePath}   |   ");

            //var expressions = GeneratorUtilities.CreateGetValue<T>(scoredPropertyMapping.Source);

            // look for the the parent needed to access the chain done with 1-1 and/or basic types 
            var tempNav = scoredPropertyMapping.Source.GetLeaf();
            NavigationSegment<Metadata> relativeRoot = null;
            SurrogateType<Metadata> relativeRootType = null;
            while (true)
            {
                if (tempNav.Previous == null ||
                    (tempNav.Previous != null && tempNav.Previous.IsOneToMany))//ModelPropertyNode.PropertyKind.IsOneToMany()))
                {
                    relativeRoot = tempNav;
                    if (tempNav.Property != null)
                        relativeRootType = tempNav.Property.OwnerType;
                    else
                        relativeRootType = tempNav.Type;

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
                //Console.WriteLine("Skipped <special case>");
                return;
            }

            var currentContext = _codeGenContext.Peek();
            var accessor = GeneratorUtilities.CreateGetValue(_typeSystem, context.SourcePath, context.Variable,
                scoredPropertyMapping.Source);

            var valueExpression = _conversionGenerator.GetJsonConversionExpression(scoredPropertyMapping, accessor);
            var writeExpression = GeneratorUtilities.JsonWriteValue(inputWriter, property.Name, valueExpression);
            //var writeExpression = GeneratorUtilities.JsonWriteString(inputWriter,
            //    modelPropertyNode.Name, Expression.Constant(modelPropertyNode.Name));
            currentContext.Statements.Add(writeExpression);

            //writer.WriteString()

            // basic types
            //Console.Write("B");
            //Console.WriteLine();
        }

        public override void OnBeginVisitCollectionProperty(SurrogateProperty<Metadata> modelPropertyNode, string targetPath)
        {
            // begin collection
            //var sourcePath = scoredPropertyMapping.Source.GetMapPath();
            //Console.WriteLine($"Start  C> {targetPath} ");
            //writer.WriteStartArray(modelPropertyNode.Name);
            var currentContext = _codeGenContext.Peek();
            var writeExpression = GeneratorUtilities.JsonWriteStartArray(inputWriter, modelPropertyNode.Name);
            currentContext.Statements.Add(writeExpression);

            // create var used in the foreach
            // all the expressions after this must use this as "root"
            // in the end-collection we create the foreach with that variable

            var sourceModelPropertyNode = FindFirstCollectionOnSourceModelPropertyNode(targetPath);
            var context = new OneToManyContext(_typeSystem, sourceModelPropertyNode, targetPath);
            _codeGenContext.Push(context);
        }

        public override void OnEndVisitCollectionProperty(SurrogateProperty<Metadata> modelPropertyNode, string path)
        {
            // end collection
            //Console.WriteLine($"End    C> {path} ");
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
        }

        private NavigationSegment<Metadata> FindFirstCollectionOnSourceModelPropertyNode(string targetPath)
        {
            NavigationSegment<Metadata> navigation;
            foreach (var path in _targetLookup.Keys)
            {
                if (path.Contains(targetPath))
                {
                    navigation = _targetLookup[path].Source.GetLeaf();
                    while (navigation != null)
                    {
                        if (navigation.IsOneToMany)// ModelPropertyNode.PropertyKind.IsOneToMany())
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
            string SourcePath { get; }
            NavigationSegment<Metadata> PropertyNode { get; }
            ParameterExpression Variable { get; }
            IList<Expression> Statements { get; }
        }

        private class SimpleContext : ICodeGenerationContext
        {
            public SimpleContext(Type type, string sourcePath)
            {
                PropertyNode = null;
                Variable = Expression.Parameter(type);
                Statements = new List<Expression>();
                SourcePath = sourcePath;
            }

            public string SourcePath { get; }
            public NavigationSegment<Metadata> PropertyNode { get; }
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
            TypeSystem<Metadata> _typeSystem;
            public OneToManyContext(TypeSystem<Metadata> typeSystem, NavigationSegment<Metadata> collectionNode, string targetPath)
            {
                _typeSystem = typeSystem;
                PropertyNode = collectionNode;
                TargetPath = targetPath;
                SourcePath = collectionNode.Path;
                var itemTypeToIterate = PropertyNode.Property.PropertyType.GetCoreType().GetOriginalType();
                _id++;
                Variable = Expression.Variable(itemTypeToIterate, $"loopVar{_id}");
                Statements = new List<Expression>();
                BodyVariables = new();
            }

            public NavigationSegment<Metadata> PropertyNode { get; }
            public string TargetPath { get; }

            /// <summary>
            /// The path of the Variable
            /// </summary>
            public string SourcePath { get; }
            public ParameterExpression Variable { get; }
            public IList<Expression> Statements { get; }
            public List<ParameterExpression> BodyVariables { get; }

            public static void Reset() => _id = 0;

            public Expression CreateForEach(ParameterExpression outerObject)
            {
                //var enumerable = GeneratorUtilities.CreateGetValue(_typeSystem, SourcePath, outerObject, PropertyNode);
                var enumerable = GeneratorUtilities.CreateGetEnumerable(_typeSystem, SourcePath, outerObject, PropertyNode);
                return GeneratorUtilities.ForEach(enumerable, Variable, CreateBody());
            }

            private BlockExpression CreateBody() => Expression.Block(BodyVariables, Statements);
        }

        public Expression<Action<Utf8JsonWriter, T>> GetSerializationAction()
        {
            var finalContext = _codeGenContext.Pop() as SimpleContext;
            var transform = finalContext.CreateTransform(inputWriter);
            return transform;
        }

    }
}
