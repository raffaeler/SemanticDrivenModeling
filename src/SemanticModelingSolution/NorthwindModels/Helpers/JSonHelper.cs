using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

// https://github.com/AjaxStack/AjaxStack/blob/master/src/jsonreport/jsonreport.js

// TODO: try indenting json:
// http://json-indent.appspot.com/

namespace NorthwindDataLayer.Helpers
{
	public class JSonHelper
	{
		StringBuilder _builder;
		string _jscript;

		private string GetScriptFromResources()
		{
			if (_jscript != null)
				return _jscript;
			string resource = "ODataLayer.Resources.jsonreport.js";
			var currentAsm = Assembly.GetExecutingAssembly();
			
			string js;
			using (var st = currentAsm.GetManifestResourceStream(resource))
			{
				byte[] blob = new byte[st.Length];
				st.Read(blob, 0, blob.Length);
				js = Encoding.UTF8.GetString(blob);
			}
			_jscript = js;
			return js;
		}

		public string GetHtmlPageForJson(string json)
		{
			_builder = new StringBuilder();
			_builder.Append("<!DOCTYPE html>\r\n<html><head>");
			_builder.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
			//_builder.Append(CreateStyles());
			CreateScripts(json);

			var bodyStyle = ".st {font-size: font:x-small 'Verdana';margin-right:1.5em;}";
			_builder.AppendLine("<style type=\"text/css\">" + bodyStyle + "</style>\r\n");
			_builder.Append("</head>");
			_builder.Append("<body onload=\"return assignResult();\" class=\"st\">");

			_builder.Append("<div id=\"jsonreport\" class=\"jsonreport\"></div>");

			_builder.Append("</body>");
			_builder.Append("</html>");
			return _builder.ToString();

		}

		private void CreateScripts(string json)
		{
			_builder.AppendLine("<script type=\"text/javascript\"><!--");
			_builder.Append(GetScriptFromResources());

			var newJson = json.Replace("\r", "").Replace("\n", "").Replace("\"", "\\\"").Replace("'", "\\'").Replace("\\r", "").Replace("\\n", "");
			_builder.AppendLine("function assignResult() {");
			_builder.AppendFormat("var j = _.jsonreport('{0}');\r\n", newJson);
			_builder.AppendLine("document.getElementById(\"jsonreport\").innerHTML = j;");
			_builder.AppendLine("}");

			////_builder.AppendFormat("document.getElementsByClassName('jsonreport')[0].innerHTML = _.jsonreport('{0}');", json.Replace("\r\n", ""));
			//_builder.AppendFormat("document.getElementById(\"jsonreport\").innerHtml = _.jsonreport('{0}');", newJson);
			_builder.AppendLine("--></script>");
		}


		private const string INDENT_STRING = "    ";
		/// <summary>
		/// http://stackoverflow.com/questions/4580397/json-formatter-in-c
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Format(string str)
		{
			var indent = 0;
			var quoted = false;
			var sb = new StringBuilder();
			for (var i = 0; i < str.Length; i++)
			{
				var ch = str[i];
				switch (ch)
				{
					case '{':
					case '[':
						sb.Append(ch);
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						break;
					case '}':
					case ']':
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						sb.Append(ch);
						break;
					case '"':
						sb.Append(ch);
						bool escaped = false;
						var index = i;
						while (index > 0 && str[--index] == '\\')
							escaped = !escaped;
						if (!escaped)
							quoted = !quoted;
						break;
					case ',':
						sb.Append(ch);
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						break;
					case ':':
						sb.Append(ch);
						if (!quoted)
							sb.Append(" ");
						break;
					default:
						if (quoted)
							sb.Append(ch);
						break;
				}
			}
			return sb.ToString();
		}

	}
}


static class Extensions
{
	public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
	{
		foreach (var i in ie)
		{
			action(i);
		}
	}
}
