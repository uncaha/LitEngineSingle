using System;
using System.Collections.Generic;
using System.IO;

namespace ExportTool
{
    public class TextWriter
    {
        private string mIndentStr = "    ";
        protected int mLineLevel;
        protected StreamWriter mWriter;
        public TextWriter(Stream _stream)
        {
            mWriter = new StreamWriter(_stream);
        }

        public TextWriter WriteLine(string _str)
        {
            for (int i = 0; i < mLineLevel; i++)
                _str = mIndentStr + _str;
            mWriter.WriteLine(_str);
            return this;
        }

        public TextWriter Write(string _str)
        {
            mWriter.Write(_str);
            return this;
        }

        public TextWriter Indent()
        {
            mLineLevel++;
            return this;
        }
        public TextWriter Outdent()
        {
            mLineLevel--;
            if (mLineLevel < 0)
                mLineLevel = 0;
            return this;
        }

        public void Close()
        {
            mWriter.Flush();
            mWriter.Close();
        }
    }
}

