using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SemanticGlossaryGenerator.Helpers
{
    public class ClassGenerator
    {
        public ClassGenerator(string @namespace, string className)
        {
            this.Namespace = @namespace;
            this.ClassName = className;
        }

        public HashSet<string> Usings { get; } = new();
        public string Namespace { get; }
        public string ClassName { get; }
        public string BaseClass { get; set; }

        internal List<MemberDeclarationSyntax> Members { get; } = new();


        public SourceText Generate()
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();

            var usingDirectives = Usings
                .Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(u)))
                .ToArray();
            compilationUnit = compilationUnit.AddUsings(usingDirectives);

            var nspaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace));

            var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName);
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            classDeclaration = AddBaseType(classDeclaration);


            //classDeclaration = classDeclaration.AddMembers();

            classDeclaration = classDeclaration.AddMembers(Members.ToArray());

            nspaceDeclaration = nspaceDeclaration.AddMembers(classDeclaration);

            compilationUnit = compilationUnit.AddMembers(nspaceDeclaration);
            return SourceText.From(compilationUnit.NormalizeWhitespace().ToFullString(), Encoding.UTF8);
        }

        private ClassDeclarationSyntax AddBaseType(ClassDeclarationSyntax classDeclaration)
        {
            if (!string.IsNullOrEmpty(BaseClass))
            {
                classDeclaration = classDeclaration.AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.IdentifierName(BaseClass)));
            }

            return classDeclaration;
        }


        internal ClassDeclarationSyntax AddSerializableAttribute(ClassDeclarationSyntax classDeclaration)
        {
            classDeclaration = classDeclaration.AddAttributeLists(
            SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList<AttributeSyntax>(
                    new List<AttributeSyntax>()
            {
                SyntaxFactory.Attribute(SyntaxFactory.ParseName("Serializable"))
            })));

            return classDeclaration;
        }

        internal ExpressionSyntax CreateInitializersWithStrings(string type, params string[] arguments)
        {
            var typeName = SyntaxFactory.ParseTypeName(type);
            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(arguments
                    .Select(a =>
                        SyntaxFactory.Argument(CreateStringLiteralExpression(a)))));

            var initializer = SyntaxFactory.ObjectCreationExpression(typeName, argumentList, null);
            return initializer;
        }

        internal ExpressionSyntax CreateInitializersWithExpressions(string type, params ExpressionSyntax[] arguments)
        {
            var typeName = SyntaxFactory.ParseTypeName(type);
            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(arguments
                    .Select(a => SyntaxFactory.Argument(a))));

            var initializer = SyntaxFactory.ObjectCreationExpression(typeName, argumentList, null);
            return initializer;
        }

        internal ExpressionSyntax CreateStringLiteralExpression(string literal)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(literal));
        }

        internal ExpressionSyntax CreateNumericLiteralExpression(int number)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(number));
        }

        internal ExpressionSyntax CreateMemberAccess2(string left, string right)
        {
            return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(left),
                        SyntaxFactory.IdentifierName(right));
        }

        internal ExpressionSyntax CreateTuple(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.TupleExpression(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                SyntaxFactory.Argument(left),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                SyntaxFactory.Argument(right)
                            }));
        }

        internal ExpressionSyntax CreateCreateObject(string typeName, params ExpressionSyntax[] arguments)
        {
            List<SyntaxNodeOrToken> nodes = new();
            bool isFirst = true;
            foreach (var arg in arguments)
            {
                if (isFirst)
                    isFirst = false;
                else
                    nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                nodes.Add(SyntaxFactory.Argument(arg));

            }

            return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(typeName))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(nodes))).NormalizeWhitespace();
        }

        internal ExpressionSyntax CreateInitializerWithCollection(string typeName, params ExpressionSyntax[] arguments)
        {
            List<SyntaxNodeOrToken> argumentList = new();
            bool isFirst = true;
            foreach (var arg in arguments)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    //argumentList.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                    argumentList.Add(SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(),
                            SyntaxKind.CommaToken,
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.CarriageReturnLineFeed)));
                }

                argumentList.Add(arg);
            }

            var initializer = SyntaxFactory.ImplicitObjectCreationExpression()
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(argumentList)));
            return initializer;
        }

        /// <summary>
        /// public static Term Name = new Term("Name", "The name of the entity");
        /// </summary>
        internal FieldDeclarationSyntax CreateStaticField(IEnumerable<string> commentLines,
            string type, string variableName, ExpressionSyntax initializer)
        {
            SyntaxTrivia comment = CreateXmlComment(true, commentLines);

            var typeName = SyntaxFactory.ParseTypeName(type);

            var variableDeclaration = SyntaxFactory.VariableDeclaration(typeName)
                .AddVariables(SyntaxFactory.VariableDeclarator(variableName)
                    .WithInitializer(SyntaxFactory.EqualsValueClause(initializer))
                );

            var field = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(comment),
                        SyntaxKind.PublicKeyword,
                        SyntaxFactory.TriviaList())))
                //.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            return field;
        }


        internal GenericNameSyntax MakeListOfT(string genericType)
        {
            return SyntaxFactory.GenericName(SyntaxFactory.Identifier("List"))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(genericType))));
        }

        internal PropertyDeclarationSyntax CreatePropertyWithInitializer(string[] commentLines,
            NameSyntax typeName, string propertyName, ExpressionSyntax initializer)
        {
            SyntaxTrivia comment = CreateXmlComment(true, commentLines);

            var type2 = SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(
                    SyntaxFactory.TriviaList(comment),
                        "int", SyntaxFactory.TriviaList()));

            //typeName = typeName.(comment);

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(typeName,
                SyntaxFactory.Identifier(propertyName))
                ;

            propertyDeclaration = propertyDeclaration
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(comment),
                        SyntaxKind.PublicKeyword,
                        SyntaxFactory.TriviaList())))
                .WithAccessorList(SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
                .WithInitializer(SyntaxFactory.EqualsValueClause(initializer))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            //propertyDeclaration = propertyDeclaration
            //    .WithLeadingTrivia(comment);

            return propertyDeclaration.NormalizeWhitespace();
        }

        public ConstructorDeclarationSyntax CreateConstructor(string[] commentLines,
            params StatementSyntax[] statements)
        {
            var methodDeclaration = SyntaxFactory.ConstructorDeclaration(this.ClassName)
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.List<StatementSyntax>(statements)));

            return methodDeclaration;
        }


        //public SyntaxTrivia CreateXmlComment(bool prependSpace, params string[] lines)
        //{
        //    return CreateXmlComment(prependSpace, lines);
        //}

        public SyntaxTrivia CreateXmlComment(bool prependSpace, IEnumerable<string> lines)
        {
            var tokens = new List<SyntaxToken>();
            tokens.Add(SyntaxFactory.XmlTextNewLine(Environment.NewLine));

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    tokens.Add(SyntaxFactory.XmlTextLiteral(line.StartsWith(" ") ? line : " " + line));
                    tokens.Add(SyntaxFactory.XmlTextNewLine(Environment.NewLine));
                }
            }

            tokens.Add(SyntaxFactory.XmlTextLiteral(" "));

            var xmlnodes = new List<XmlNodeSyntax>();
            if (prependSpace)
            {
                xmlnodes.Add(SyntaxFactory.XmlText()
                    .WithTextTokens(SyntaxFactory.TokenList(
                        SyntaxFactory.XmlTextNewLine(SyntaxFactory.TriviaList(),
                                                            Environment.NewLine,
                                                            Environment.NewLine,
                                                            SyntaxFactory.TriviaList()))));
            }


            xmlnodes.AddRange(new XmlNodeSyntax[]
            {

                SyntaxFactory.XmlText().WithTextTokens(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.XmlTextLiteral(
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.DocumentationCommentExterior("///")),
                            " ",
                            " ",
                            SyntaxFactory.TriviaList()))),

                SyntaxFactory.XmlExampleElement(
                    SyntaxFactory.SingletonList<XmlNodeSyntax>(
                        SyntaxFactory.XmlText()
                        .WithTextTokens(
                            SyntaxFactory.TokenList(tokens))))
                .WithStartTag(
                    SyntaxFactory.XmlElementStartTag(
                        SyntaxFactory.XmlName(SyntaxFactory.Identifier("summary"))))
                .WithEndTag(
                    SyntaxFactory.XmlElementEndTag(
                        SyntaxFactory.XmlName(SyntaxFactory.Identifier("summary")))),

                SyntaxFactory.XmlText()
                    .WithTextTokens(SyntaxFactory.TokenList(
                        SyntaxFactory.XmlTextNewLine(SyntaxFactory.TriviaList(),
                                                            Environment.NewLine,
                                                            Environment.NewLine,
                                                            SyntaxFactory.TriviaList()))),
            });

            return SyntaxFactory.Trivia(
                        SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia,
                        SyntaxFactory.List<XmlNodeSyntax>(xmlnodes)));
        }


        public StatementSyntax CreateAddCollection(string collection, ExpressionSyntax expression)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(collection),
                        SyntaxFactory.IdentifierName("Add")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                expression)))));
        }

    }
}
