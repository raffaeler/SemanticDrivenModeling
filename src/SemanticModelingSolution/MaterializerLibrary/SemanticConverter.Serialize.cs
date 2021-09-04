﻿using System;
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
        private Stack<ICodeGenerationContext> _codeGenContext;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Console.WriteLine($"TesterConverter.Write> ");
            OneToManyContext.Reset();   // move to ctor
            _codeGenContext = new();
            _codeGenContext.Push(new SimpleContext(typeof(T)));
            ParameterExpression inputWriter = Expression.Parameter(typeof(Utf8JsonWriter), "writer");

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

                ICodeGenerationContext context = null;
                foreach (var ctx in _codeGenContext)
                {
                    if(ctx.Variable.Type == scoredPropertyMapping.Source.ModelPropertyNode.PropertyInfo.ParentType.GetOriginalType())
                    {
                        context = ctx;
                        break;
                    }
                }

                Debug.Assert(context != null);

                var currentContext = _codeGenContext.Peek();
                var accessor = GeneratorUtilities.CreateGetValue(context.Variable,
                    scoredPropertyMapping.Source);

                // TODO: create expression to write accessor.ToString() to json
                // TODO: create utilities to create Expressions writing json
                var writeExpression = GeneratorUtilities.JsonWriteString(inputWriter,
                    modelPropertyNode.Name, Expression.Constant(modelPropertyNode.Name));
                currentContext.Statements.Add(writeExpression);

                

                // basic types
                Console.Write("B");
                //writer.WritePropertyName(modelPropertyNode.Name);
                //writer.WriteString(modelPropertyNode.Name, modelPropertyNode.Name);

                Console.WriteLine();
                //foreach (var expression in expressions)
                //{
                //    Console.WriteLine($"     [E]>{expression}");
                //}
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
                // 
                // every begin-collection must be put inside a stack
                // every end-collection must pop-out from the stack

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

                var writeExpression = GeneratorUtilities.JsonWriteEndArray(inputWriter);
                outerContext.Statements.Add(writeExpression);

                // if the outerContext.Variable has a Type mismatch,
                // the code must be modified to search the correct one in the other contexts
                // as we already did in other parts
                // this may happen with two nested list<>
                var loop = currentContext.CreateForEach(outerContext.Variable);
                outerContext.Statements.Add(loop);
            });

            var finalContext = _codeGenContext.Pop() as SimpleContext;
            var transform = finalContext.CreateTransform(inputWriter);
            var transformDelegate = transform.Compile();
            transformDelegate(writer, value);
        }

        private ModelNavigationNode FindFirstCollectionOnSourceModelPropertyNode(string targetPath)
        {
            ModelNavigationNode navigation;
            foreach(var path in _targetLookup.Keys)
            {
                if(path.Contains(targetPath))
                {
                    navigation = _targetLookup[path].Source;
                    while(navigation != null)
                    {
                        if(navigation.ModelPropertyNode.PropertyKind.IsOneToMany())
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


    }
}
