using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

// scripts and pagination: Copyright (c) Microsoft

namespace NorthwindDataLayer.Helpers
{
	public class XmlToHtmlHelper
	{
		private StringBuilder _builder;
		private XElement _element;
		private string _currentPrefix = string.Empty;
		private XName _xmlns = XName.Get("xmlns");

		public void SetDocument(string document)
		{
			try { _element = XElement.Parse(document); }
			catch (Exception) { _element = null; }
		}

		public void SetDocument(XElement element)
		{
			_element = element;
		}

		public bool IsXml
		{
			get { return _element != null; }
		}

		public XmlToHtmlHelper(string element)
		{
			SetDocument(element);
		}

		#region Styles
		private string CreateStyles()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<style>");
			sb.AppendLine("BODY{font:x-small 'Verdana';margin-right:1.5em}");
			sb.AppendLine(".c{cursor:hand}");
			sb.AppendLine(".b{color:red;font-family:'Courier New';font-weight:bold;text-decoration:none}");
			sb.AppendLine(".e{margin-left:1em;text-indent:-1em;margin-right:1em}");
			sb.AppendLine(".k{margin-left:1em;text-indent:-1em;margin-right:1em}");
			sb.AppendLine(".t{color:#990000}");
			sb.AppendLine(".xt{color:#990099}");
			sb.AppendLine(".ns{color:red}");
			sb.AppendLine(".dt{color:green}");
			sb.AppendLine(".m{color:blue}");
			sb.AppendLine(".tx{font-weight:bold}");
			sb.AppendLine(".db{text-indent:0px;margin-left:1em;margin-top:0px;margin-bottom:0px;padding-left:.3em;border-left:1px solid #CCCCCC;font:small Courier}");
			sb.AppendLine(".di{font:small Courier}");
			sb.AppendLine(".d{color:blue}");
			sb.AppendLine(".pi{color:blue}");
			sb.AppendLine(".cb{text-indent:0px;margin-left:1em;margin-top:0px;margin-bottom:0px;padding-left:.3em;font:small Courier;color:#888888}");
			sb.AppendLine(".ci{font:small Courier;color:#888888}");
			sb.AppendLine("PRE{margin:0px;display:inline}");
			sb.AppendLine("</style>");
			return sb.ToString();
		}
		#endregion

		#region Scripts
		private string CreateScripts()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<script type=\"text/javascript\"><!--");
			sb.AppendLine("function f(e){");
			sb.AppendLine("if (e.className==\"ci\"){if (e.children(0).innerText.indexOf(\"\\n\")>0) fix(e,\"cb\");}");
			sb.AppendLine("if (e.className==\"di\"){if (e.children(0).innerText.indexOf(\"\\n\")>0) fix(e,\"db\");}");
			sb.AppendLine("e.id=\"\";");
			sb.AppendLine("}");
			sb.AppendLine("function fix(e,cl){");
			sb.AppendLine("e.className=cl;");
			sb.AppendLine("e.style.display=\"block\";");
			sb.AppendLine("j=e.parentElement.children(0);");
			sb.AppendLine("j.className=\"c\";");
			sb.AppendLine("k=j.children(0);");
			sb.AppendLine("k.style.visibility=\"visible\";");
			sb.AppendLine("k.href=\"#\";");
			sb.AppendLine("}");
			sb.AppendLine("function ch(e){");
			sb.AppendLine("mark=e.children(0).children(0);");
			sb.AppendLine("if (mark.innerText==\"+\"){");
			sb.AppendLine("mark.innerText=\"-\";");
			sb.AppendLine("for (var i=1;i<e.children.length;i++)");
			sb.AppendLine("e.children(i).style.display=\"block\";");
			sb.AppendLine("}");
			sb.AppendLine("else if (mark.innerText==\"-\"){");
			sb.AppendLine("mark.innerText=\"+\";");
			sb.AppendLine("for (var i=1;i<e.children.length;i++)");
			sb.AppendLine("e.children(i).style.display=\"none\";");
			sb.AppendLine("}}");
			sb.AppendLine("function ch2(e){");
			sb.AppendLine("mark=e.children(0).children(0);");
			sb.AppendLine("contents=e.children(1);");
			sb.AppendLine("if (mark.innerText==\"+\"){");
			sb.AppendLine("mark.innerText=\"-\";");
			sb.AppendLine("if (contents.className==\"db\"||contents.className==\"cb\")");
			sb.AppendLine("contents.style.display=\"block\";");
			sb.AppendLine("else contents.style.display=\"inline\";");
			sb.AppendLine("}");
			sb.AppendLine("else if (mark.innerText==\"-\"){");
			sb.AppendLine("mark.innerText=\"+\";");
			sb.AppendLine("contents.style.display=\"none\";");
			sb.AppendLine("}}");
			sb.AppendLine("function cl(){");
			sb.AppendLine("e=window.event.srcElement;");
			sb.AppendLine("if (e.className!=\"c\"){e=e.parentElement;if (e.className!=\"c\"){return;}}");
			sb.AppendLine("e=e.parentElement;");
			sb.AppendLine("if (e.className==\"e\") ch(e);");
			sb.AppendLine("if (e.className==\"k\") ch2(e);");
			sb.AppendLine("}");
			sb.AppendLine("function ex(){}");
			sb.AppendLine("function h(){window.status=\" \";}");
			sb.AppendLine("document.onclick=cl;");

			sb.AppendLine("--></script>");
			return sb.ToString();
		}
		#endregion

		/// <summary>
		/// Main entrypoint
		/// </summary>
		/// <returns></returns>
		public string Convert(string title)
		{
			_builder = new StringBuilder();
			_builder.Append("<html><head>");
			_builder.Append(CreateStyles());
			_builder.Append(CreateScripts());
			_builder.Append("<title>");
			_builder.Append(title);
			_builder.Append("</title>");
			_builder.Append("</head>");
			_builder.Append("<body class=\"st\">");
			_builder.Append("<div class=\"e\">");

			Convert(_element);

			_builder.Append("</div>");
			_builder.Append("</body>");
			_builder.Append("</html>");
			return _builder.ToString();

		}

		/// <summary>
		/// recursive function
		/// </summary>
		/// <param name="node"></param>
		private void Convert(XNode node)
		{
			switch (node.NodeType)
			{
				case XmlNodeType.ProcessingInstruction:
					GetProcessingInstruction(node as XProcessingInstruction);
					break;
				case XmlNodeType.Comment:
					GetComment(node as XComment);
					break;
				case XmlNodeType.Element:
					_builder.Append("<div class=\"e\">");
					GetElement(node as XElement);
					_builder.Append("</div>");
					break;
				case XmlNodeType.Text:
					var text = node as XText;
					_builder.AppendFormat("<span class=\"tx\">{0}</span>", text.Value);
					break;

				// TODO: other XmlNodeType goes here ...

				default:
					_builder.Append("<div>" + node.ToString() + "</div>");	// this is a bad formatter
					break;
			}
		}

		private string _lastId;
		private void GetElement(XElement xElement)
		{
			var xmlnsAttribute = xElement.Attributes(_xmlns).FirstOrDefault();
			if (xmlnsAttribute != null)
				_currentPrefix = xElement.GetPrefixOfNamespace(XNamespace.Get(xmlnsAttribute.Value));

			_builder.Append("<div ");
			if (xElement.HasElements)
				_builder.Append("class=\"c\" ");
			_builder.Append("style=\"margin-left:1em;text-indent:-2em\">");
			if (xElement.HasElements)
				_builder.Append("<a href=\"#\" onclick=\"return false\" onfocus=\"h()\" class=\"b\">-</a>");
			else
				_builder.Append("<span class=\"b\">&nbsp;</span>");

			_builder.Append("<span class=\"m\">&lt;</span>");
			string prefix = xElement.GetPrefixOfNamespace(xElement.Name.Namespace);
			StringBuilder elementNameBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(prefix) && prefix != _currentPrefix)
			{
				elementNameBuilder.AppendFormat("<span class=\"t\">{0}</span>", prefix);
				elementNameBuilder.AppendFormat("<span class=\"m\">:</span>", prefix);
			}
			elementNameBuilder.AppendFormat("<span class=\"t\">{0}</span>", xElement.Name.LocalName);
			string elementName = elementNameBuilder.ToString();
			_builder.Append(elementName);
			if (xElement.Name.LocalName == "id")
			{
				var firstNode = xElement.FirstNode;
				if (firstNode != null)
				{
					var xtext = firstNode as XText;
					if (xtext != null)
					{
						try
						{
							Uri uri = new Uri(xtext.Value);
							_lastId = string.Format("{0}://{1}{2}", 
								uri.Scheme,
								uri.Authority,
								string.Join("", uri.Segments, 0, uri.Segments.Length - 1));
						}
						catch (Exception){}
					}
				}
			}

			foreach (var attr in xElement.Attributes())
			{
				_builder.AppendLine();
				string prefixAttr = xElement.GetPrefixOfNamespace(attr.Name.Namespace);
				if (!string.IsNullOrEmpty(prefixAttr) && prefixAttr != _currentPrefix)
					_builder.AppendFormat("<span class=\"ns\">{0}:{1}</span>", prefixAttr, attr.Name.LocalName);
				else
					_builder.AppendFormat("<span class=\"ns\">{0}</span>", attr.Name.LocalName);
				_builder.AppendFormat("<span class=\"m\">=\"</span>");
				if (xElement.Name.LocalName == "link" && attr.Name.LocalName == "href")
				{
					var link = _lastId + attr.Value + "?$format=xmlhtml";
					_builder.AppendFormat("<a href={0}><b class=\"ns\">{1}</b></a>", link, attr.Value);
				}
				else
				{
					_builder.AppendFormat("<b class=\"ns\">{0}</b>", attr.Value);
				}
				_builder.AppendFormat("<span class=\"m\">\"</span>\r\n");
			}
			if (xElement.IsEmpty)
			{
				_builder.AppendFormat("<span class=\"m\">/&gt;</span>\r\n");
				_builder.Append("</div>");
			}
			else
			{
				_builder.AppendFormat("<span class=\"m\">&gt;</span>");

				if (xElement.HasElements)
				{
					_builder.Append("</div>");
					_builder.Append("<div>");
				}
				foreach (var node in xElement.Nodes())
				{
					Convert(node);
				}
				if (xElement.HasElements)
					_builder.Append("</div>");

				// close tag
				if (xElement.HasElements)
				{
					_builder.Append("<div>");
					_builder.Append("<span class=\"b\">&nbsp;</span>");
				}
				_builder.Append("<span class=\"m\">&lt;/</span>");
				_builder.Append(elementName);
				_builder.Append("<span class=\"m\">&gt;</span>");

				//if (xElement.HasElements)
				_builder.Append("</div>");	// if HasElements it close a div, if it's not it close another div
			}
		}

		private void GetComment(XComment comment)
		{
			_builder.Append(string.Format("<span class=\"dt\"><!-- {0} --></span>", comment.Value));
		}

		private void GetProcessingInstruction(XProcessingInstruction pi)
		{
			_builder.Append("<span class='b'>&nbsp;</span><span class='m'>&lt;?</span></div>");
			_builder.AppendFormat("<span class=\"pi\">{0} </span>", pi.Data);
			_builder.Append("<span class=\"m\">?&gt;</span>");
		}

	}
}
