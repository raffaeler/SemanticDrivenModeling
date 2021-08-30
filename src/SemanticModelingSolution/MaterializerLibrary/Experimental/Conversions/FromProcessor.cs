using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
            
//var typeName = SyntaxFactory.ParseTypeName("bool");

namespace MaterializerLibrary
{
    public class FromProcessor
    {
        public ExpressionSyntax InvokeBoolTryParse()
        {
            var typeName = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
            var method = SyntaxFactory.IdentifierName("TryParse");

            var invocation = CreateInvokeMethod(typeName, method,
                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")),
                CreateOutArgument(typeName, "res"));

            return invocation;
        }

        internal ExpressionSyntax CreateInvokeMethod(TypeSyntax typeName, IdentifierNameSyntax method, params ArgumentSyntax[] arguments)
        {
            var member = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeName, method);
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
            return SyntaxFactory.InvocationExpression(member, argumentList);
        }

        internal IEnumerable<ArgumentSyntax> FromExpressionSyntax(params ExpressionSyntax[] expressions)
        {
            return expressions.Select(a => SyntaxFactory.Argument(a));
        }

        internal ArgumentSyntax CreateOutArgument(TypeSyntax typeName, string argName)
        {
            return SyntaxFactory.Argument(
                SyntaxFactory.DeclarationExpression(
                    typeName,
                    SyntaxFactory.SingleVariableDesignation(
                        SyntaxFactory.Identifier(argName))))
                .WithRefOrOutKeyword(
                SyntaxFactory.Token(SyntaxKind.OutKeyword));
        }


    }
}
