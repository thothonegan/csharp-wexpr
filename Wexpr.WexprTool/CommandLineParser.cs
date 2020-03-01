using System;

namespace Wexpr.WexprTool
{
	//
	/// <summary>
	/// Parse commandline arguments
	/// </summary>
	//
	public static class CommandLineParser
	{
		public enum Command
		{
			Unknown,

			/// Make the wexpr human readable
			HumanReadable,

			/// Validate the wexpr, output 'true' or 'false'
			Validate,

			/// Minify the input
			Mini,

			/// Convert the wexpr to binary
			Binary
		}

		public class Results
		{
			public bool Help = false;
			public bool Version = false;
			public bool Validate = false;

			public Command Command = Command.HumanReadable;
			public string InputPath = "-";
			public string OutputPath = "-";
		}

		static public Results Parse (string[] args)
		{
			Results r = new Results();

			for (int argIndex=0; argIndex < args.Length; ++argIndex)
			{
				string arg = args[argIndex];

				if (arg == "-h" || arg == "--help")
					r.Help = true;
				else if (arg == "-v" || arg == "--version")
					r.Version = true;
				else if (arg == "-c" || arg == "--command")
				{
					if ( (argIndex+1) < args.Length)
					{
						r.Command = s_commandFromString(args[argIndex+1]);
						++argIndex;
					}
				}
				else if (arg == "-i" || arg == "--input")
				{
					if ((argIndex+1) < args.Length)
					{
						r.InputPath = args[argIndex+1];
					}
				}
				else if (arg == "-o" || arg == "--output")
				{
					if ((argIndex+1) < args.Length)
					{
						r.OutputPath = args[argIndex+1];
					}
				}
			}

			return r;
		}

		static public void DisplayHelp (string[] args)
		{
			// args doesnt contain it
			string arg = Environment.GetCommandLineArgs()[0];

			Console.WriteLine($"Usage: {arg} [OPTIONS]");
			Console.WriteLine($"Performs operations on wexpr data");
			Console.WriteLine($"");
			Console.WriteLine($"-c, --cmd     Perform the requested command");
			Console.WriteLine($"              humanReadable - [default] Makes the wexpr input human readable and outputs.");
			Console.WriteLine($"              validate      - Checks the wexpr. If valid outputs 'true' and returns 0, otherwise 'false' and 1.");
			Console.WriteLine($"              mini          - Minifies the wexpr output");
			Console.WriteLine($"              binary        - Write the wexpr out as binary");
			Console.WriteLine($"");
			Console.WriteLine($"-i, --input   The input file to read from (default is -, stdin).");
			Console.WriteLine($"-o, --output  The place to write the output (default is -, stdout).");
			Console.WriteLine($"-h, --help    Display this help and exit");
			Console.WriteLine($"-v, --version Output the version and exit");
		}

		// --- private static

		static public Command s_commandFromString (string str)
		{
			if (str == "humanReadable")
				return Command.HumanReadable;
			else if (str == "validate")
				return Command.Validate;
			else if (str == "mini")
				return Command.Mini;
			else if (str == "binary")
				return Command.Binary;
			
			return Command.Unknown;
		}
	}
}
