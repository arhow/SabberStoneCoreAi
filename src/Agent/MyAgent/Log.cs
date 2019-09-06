using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Agent.MyAgents
{
    class Log
    {
		string filePath = null;

		private static Log instance = null;
		private System.IO.StreamWriter file = null;

		private Log(string filePath, bool append=true)
		{
			this.filePath = filePath;
			file = new System.IO.StreamWriter(filePath, append);
		}

		public static Log Instance(string filePath = "log.txt", bool append=true)
		{
			if (instance == null)
				instance = new Log(filePath, append);
			return instance;
		}

        public static string[] ReadAllLines(string filePath = "log.txt")
        {
            return System.IO.File.ReadAllLines(filePath);
        }

        public void New(string filePath = "log.txt")
        {
            instance = new Log(filePath, false);
        }

		public void Append(string content)
		{
			// Example #4: Append new text to an existing file.
			// The using statement automatically flushes AND CLOSES the stream and calls 
			// IDisposable.Dispose on the stream object.
			
			file.WriteLine(content);
			file.Flush();
		}

		public void Close()
		{
			file.Close();
			file = null;
			instance = null;
		}
	}
}
