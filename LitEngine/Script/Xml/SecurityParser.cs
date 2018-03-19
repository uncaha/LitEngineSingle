using System;
using System.Collections;
using System.IO;
using System.Security;
namespace LitEngine
{
    namespace XmlLoad
    {

        // convert an XML document into SecurityElement objects
        public class SecurityParser : SmallXmlParser, SmallXmlParser.IContentHandler
        {

            private SecurityElement root;

            public SecurityParser() : base()
            {
                stack = new Stack();
            }

            public void LoadXml(string xml)
            {
                root = null;
#if CF_1_0
			stack = new Stack ();
#else
                stack.Clear();
#endif
                Parse(new StringReader(xml), this);
            }

            public SecurityElement ToXml()
            {
                return root;
            }

            // IContentHandler

            private SecurityElement current;
            private Stack stack;

            public void OnStartParsing(SmallXmlParser parser) { }

            public void OnProcessingInstruction(string name, string text) { }

            public void OnIgnorableWhitespace(string s) { }

            public void OnStartElement(string name, SmallXmlParser.IAttrList attrs)
            {
                SecurityElement newel = new SecurityElement(name);
                if (root == null)
                {
                    root = newel;
                    current = newel;
                }
                else
                {
                    SecurityElement parent = (SecurityElement)stack.Peek();
                    parent.AddChild(newel);
                }
                stack.Push(newel);
                current = newel;
                // attributes
                int n = attrs.Length;
                for (int i = 0; i < n; i++)
                    current.AddAttribute(attrs.GetName(i), attrs.GetValue(i));
            }

            public void OnEndElement(string name)
            {
                current = (SecurityElement)stack.Pop();
            }

            public void OnChars(string ch)
            {
                current.Text = ch;
            }

            public void OnEndParsing(SmallXmlParser parser) { }
        }
    }
}


