using Wexpr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wexpr.WexprTool
{
	class Program
	{
		const int ExitFailure = 1;
		const int ExitSuccess = 0;
		const UInt32 VersionHandled = 0x00001000; // 0.1.0
		
		static int Main(string[] args)
		{
			var results = CommandLineParser.Parse(args);

			if (results.Version)
			{
				Console.WriteLine($"WexprTool {Wexpr.Version.Major}.{Wexpr.Version.Minor}.{Wexpr.Version.Patch}");
				return ExitSuccess;
			}

			if (results.Help)
			{
				CommandLineParser.DisplayHelp(args);
				return ExitSuccess;
			}

			/// normal flow
			if (results.Command == CommandLineParser.Command.HumanReadable ||
				results.Command == CommandLineParser.Command.Validate ||
				results.Command == CommandLineParser.Command.Mini ||
				results.Command == CommandLineParser.Command.Binary
			)
			{
				bool isValidate = (results.Command == CommandLineParser.Command.Validate);
				byte[] inputBytes = s_readAllInputFrom(results.InputPath);
				
				// determine if binary or not
				// if so, strip the header and do the chunk
				Expression expr = null;
				Wexpr.Exception err = null;

				do { // so we can break back to here
					if (inputBytes.Length >= 1 && inputBytes[0] == 0x83)
					{
						if (inputBytes.Length < 20)
						{
							err = new BinaryInvalidHeaderException("Invalid binary header - not big enough");
						}

						byte[] magic = {
							0x83, (byte)'B', (byte)'W', (byte)'E', (byte)'X', (byte)'P', (byte)'R', 0x0A
						};

						if (!inputBytes.Take(8).SequenceEqual(magic))
						{
							err = new BinaryInvalidHeaderException("Invalid binary header - invalid magic");
							break;
						}

						var bytes = inputBytes;
						if (BitConverter.ToUInt32(inputBytes, 8) != Endian.UInt32ToBig(VersionHandled))
						{
							err = new BinaryUnknownVersionException("Invalid binary header - unknown version");
							break;
						}

						// make sure reserved is blank
						byte[] reserved = {
							0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
						};
						if (!inputBytes.Skip(12).Take(8).SequenceEqual(reserved))
						{
							err = new BinaryInvalidHeaderException("Invalid binary header - unknown reserved bits");
							break;
						}

						// header seems valid, skip it
						int curPos = 20;
						int endPos = inputBytes.Length;

						while (curPos < endPos)
						{
							// read the size and type
							UInt64 size = 0;
							var b = inputBytes.Skip(curPos).Take(endPos-curPos).ToArray();
							var restPos = UVLQ64.Read(
								b, out size
							);

							// TODO: if !restNewPos

							// dont move past the size yet
							var sizeSize = restPos;
							byte type = inputBytes[curPos+sizeSize];

							if (/*given: type >= 0x00 &&*/ type <= 0x04)
							{
								// cool parse it
								if (expr != null)
								{
									err = new BinaryMultipleExpressionsException("Found multiple expression chunk");
									break;
								}

								// hand it the entire chunk, including the size and the type
								var dataBuf = inputBytes.Skip(curPos).Take((int)size + (int)sizeSize + 1).ToArray();
								expr = Expression.CreateFromBinaryChunk(
									dataBuf
								);
							}
							else
							{
								Console.WriteLine($"Warning: Unknown chunk with type {type} at byte {curPos+sizeSize}");
							}

							// move forward : pass type, pass size
							curPos += 1 + (int)sizeSize;
							curPos += (int)size;
						}
					}
					else
					{
						// assume string
						try
						{
							string inputStr = System.Text.Encoding.UTF8.GetString(inputBytes);
							expr = Expression.CreateFromString(
								inputStr, ParseFlags.None
							);
						}
						catch (Wexpr.Exception e)
						{
							err = e;
						}
					}
				} while (false);

				if (err != null)
				{
					if (isValidate)
					{
						s_writeAllOutputTo(results.OutputPath, "false\n");
						return ExitFailure;
					}
					else
					{
						string input = results.InputPath;
						if (input == "-")
							input = "(stdin)";

						Console.Error.WriteLine($"WexprTool:  Error occurred with wexpr:");
						Console.Error.WriteLine($"WexprTool: {input}:{err.Line}:{err.Column}: {err.Message}");
						return ExitFailure;
					}
				}

				if (expr == null)
				{
					if (isValidate)
					{
						s_writeAllOutputTo(results.OutputPath, "false\n");
					}
					else
					{
						Console.Error.WriteLine($"WexprTool: Got an empty expression back");
					}

					return ExitFailure;
				}

				if (isValidate)
				{
					s_writeAllOutputTo(results.OutputPath, "true\n");
				}

				else if (results.Command == CommandLineParser.Command.HumanReadable)
				{
					var buffer = expr.CreateStringRepresentation(0, WriteFlags.HumanReadable);
					s_writeAllOutputTo(results.OutputPath, buffer);
				}

				else if (results.Command == CommandLineParser.Command.Mini)
				{
					var buffer = expr.CreateStringRepresentation(0, WriteFlags.None);
					s_writeAllOutputTo(results.OutputPath, buffer);
				}

				else if (results.Command == CommandLineParser.Command.Binary)
				{
					var binData = expr.CreateBinaryRepresenation();
					s_writeAllOutputWithFileHeaderTo(
						results.OutputPath,
						binData
					);
				}
			}
			else
			{
				Console.Error.WriteLine("WexprTool: Unknown command");
				return ExitFailure;
			}

			return ExitSuccess;
		}

		// --- private static

		static byte[] s_readAllInputFrom (string inputPath)
		{
			List<char> data = new List<char>();

			if (inputPath == "-")
			{
				while (true) {
					int curChar = Console.Read();
					if (curChar == -1)
						break;
					
					data.Add(Convert.ToChar(curChar));
				}

				char[] arr = data.ToArray();
				return System.Text.UnicodeEncoding.Unicode.GetBytes(arr);
			}
			else
			{
				var sr = new StreamReader(inputPath);

				using (var memstream = new MemoryStream())
				{
					sr.BaseStream.CopyTo(memstream);
					
					sr.Dispose();
					return memstream.ToArray();
				}
			}
		}

		static void s_writeAllOutputTo (string outputPath, string str)
		{
			System.IO.TextWriter f = Console.Out;
			if (outputPath != "-")
			{
				f = new StreamWriter(outputPath);
			}
			
			f.Write(str);
			f.Flush();
			f.Dispose();
		}

		static void s_writeAllOutputWithFileHeaderTo (string outputPath, byte[] buffer)
		{
			System.IO.TextWriter f = Console.Out;
			if (outputPath != "-")
			{
				f = new StreamWriter(outputPath);
			}

			// TODO: Move writing header to libWexpr since its part of the file format.
			byte[] header = new byte[20]{
				0x83,
				(byte)'B', (byte)'W', (byte)'E', (byte)'X', (byte)'P', (byte)'R', 0x0A,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
			};
			
			byte[] version = BitConverter.GetBytes(Endian.UInt32ToBig(VersionHandled));
			Array.Copy(version, 0, header, 8, 4);

			// reserved is 0

			f.Write(header);

			// currently we have no aux chunks

			//write main chunk
			f.Write(buffer);

			// flush to the stream
			f.Flush();
			f.Dispose();
		}

	}
}
