using System;
using System.Collections.Generic;
using System.Linq;

namespace Wexpr
{
	//
	/// <summary>
	/// A wexpr expression
	/// </summary>
	///
	/// An expression represents any specific type in Wexpr. It can be:
	/// - null/none - means the expression is invalid or nothing.
	/// - a value in the form of:
	///     alphanumeric characters: asdf
	///     a quoted string: "asdf"
	///     a number: 2.3
	/// - an array: #(a b c)
	/// - a map \@(key1 value1 key2 value2)
	/// - a binary data as Base64: \<SGlzdG9yeSBtYXkgbm90IHJlcGVhdCwgYnV0IGl0IHJoeW1lcy4=\>
	///
	/// Comments ;[endofline] or ;(--...--) are not stored and are stripped on import.
	/// References [asdf] *[asdf] are also only interpreted on import, and thrown away. (? we might be able to keep it if we're storing the tree anyways).
	//
	public sealed class Expression
	{
		// --- public Construction/Destruction

		//
		/// <summary>
		/// Creates an expression from a string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="flags">Flags about parsing</param>
		/// <returns>The created expression, or null if none.</returns>
		//
		public static Expression CreateFromString (string str, ParseFlags flags)
		{
			return CreateFromStringWithExternalReferenceTable(str, flags, null);
		}

		//
		/// <summary>
		/// Creates an expression from a string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="flags">Flags about parsin.</param>
		/// <param name="referenceTable">The table to use for pulling references in, after ones in the file.</param>
		/// <returns>The created expression, or null if none.</returns>
		//
		public static Expression CreateFromStringWithExternalReferenceTable (string str, ParseFlags flags, ReferenceTable referenceTable)
		{
			var expr = CreateInvalid();

			var parserState = new Internal.ParserState();

			parserState.ExternalReferenceMap = referenceTable;

			// we assume its UTF8 safe since csharp should take care of this

			// now start parsing
			var rest = expr.p_parseFromString(
				str, flags, parserState
			);

			var postRest = s_trimFrontOfString(rest, parserState);

			if (postRest.Length != 0)
			{
				throw new ExtraDataAfterParsingRootException(parserState.Line, parserState.Column, "Extra data after parsing the root expression");
			}

			if (expr.ExpressionType == ExpressionType.Invalid)
			{
				throw new EmptyStringException(parserState.Line, parserState.Column, "No expression found [remained invalid]");
			}

			return expr;
		}

		//
		/// <summary>
		/// Creates an expression from a binary chunk.
		/// </summary>
		/// <param name="data">The data</param>
		/// <returns>The created expression, or nullptr if none.</returns>
		//
		public static Expression CreateFromBinaryChunk (byte[] data)
		{
			var expr = CreateInvalid();
			/*unused - rest of buffer =*/ expr.p_parseFromBinaryChunk(data);

			return expr;
		}

		//
		/// <summary>
		/// Create an empty invalid expression.
		/// </summary>
		/// <returns>A newly created invalid expression.</returns>
		//
		public static Expression CreateInvalid ()
		{
			var expr = new Expression();
			expr.m_type = ExpressionType.Invalid;
			return expr;
		}

		//
		/// <summary>
		/// Creates an empty null expression.
		/// </summary>
		/// <returns>A newly created null expression.</returns>
		//
		public static Expression CreateNull ()
		{
			var expr = new Expression();
			expr.m_type = ExpressionType.Null;
			return expr;
		}

		//
		/// <summary>
		/// Create a value expression from the given string.
		/// </summary>
		/// <param name="val">The string to turn into a value.</param>
		/// <returns>The newly created expression.</returns>
		//
		public static Expression CreateValue(string val)
		{
			var expr = CreateNull();
			if (expr != null)
			{
				expr.ChangeType(ExpressionType.Value);
				expr.Value = val;
			}

			return expr;
		}

		//
		/// <summary>
		/// Create a copy of the expression. Deep copy.
		/// </summary>
		/// <returns>The newly created expression.</returns>
		//
		public Expression Copy()
		{
			var expr = CreateNull();
			expr.p_copyFrom(this);
			return expr;
		}

		// --- public Information

		//
		/// <summary>
		/// Return the type of the expression.
		/// </summary>
		//
		public ExpressionType ExpressionType
		{ get { return m_type; } }

		//
		/// <summary>
		///  Change the type of the expression. Invalidates all data currently in the expression.
		/// </summary>
		/// <param name="type">The new type of the expression.</param>
		//
		public void ChangeType(ExpressionType type)
		{
			// set and recreate our new type
			m_type = type;

			if (m_type == ExpressionType.Value)
			{
				m_value = new ExpressionPrivateValue();
				m_value.Init();
			}

			else if (m_type == ExpressionType.BinaryData)
			{
				m_binaryData = new ExpressionPrivateBinaryData();
				m_binaryData.Init();
			}

			else if (m_type == ExpressionType.Array)
			{
				m_array = new ExpressionPrivateArray();
				m_array.Init();
			}

			else if (m_type == ExpressionType.Map)
			{
				m_map = new ExpressionPrivateMap();
				m_map.Init();
			}
		}

		//
		/// <summary>
		/// Creates a string which represents the expression.
		/// </summary>
		/// <param name="indent">The starting indent level, generally 0. Will use tabs to indent.</param>
		/// <param name="flags">Flags to use when writing the string.</param>
		/// <returns>String with teh representation in Wexpr text format.</returns>
		//
		public string CreateStringRepresentation (int indent, WriteFlags flags)
		{
			var str = p_appendStringRepresentationToBuffer(
				flags, indent, ""
			);

			return str;
		}

		//
		/// <summary>
		/// Creates binary data which represents the expression. This contains an expression chunk
		/// and all of its child chunks.
		/// </summary>
		/// <returns>Binary chunk in bwexpr format.</returns>
		//
		public byte[] CreateBinaryRepresenation ()
		{
			throw new NotImplementedException();
		}

		// --- public Values

		//
		/// <summary>
		/// Return the value of a value expression.
		/// </summary>
		//
		public string Value
		{
			get {
				if (m_type != ExpressionType.Value)
					throw new InvalidExpressionTypeException("Expected Value");

				return m_value.data;
			}

			set {
				if (m_type != ExpressionType.Value)
					throw new InvalidExpressionTypeException("Expected Value");

				m_value.data = value;
			}
		}

		// --- public Binary Data

		//
		/// <summary>
		/// Return the binary data of a binary data expression.
		/// </summary>
		//
		public byte[] BinaryData
		{
			get {
				if (m_type != ExpressionType.BinaryData)
					throw new InvalidExpressionTypeException("Expected BinaryData");

				return m_binaryData.data;
			}

			set {
				if (m_type != ExpressionType.BinaryData)
					throw new InvalidExpressionTypeException("Expected BinaryData");

				m_binaryData.data = value;
			}
		}

		// --- public Array

		//
		/// <summary>
		/// Return the number of expression in the array.
		/// </summary>
		//
		public int ArrayCount
		{
			get {
				if (m_type != ExpressionType.Array)
					throw new InvalidExpressionTypeException("Expected Array");

				return m_array.list.Count;
			}
		}

		//
		/// <summary>
		/// Return the expression at the given index [0 .. arraycount-1].
		/// </summary>
		/// <param name="index">The index in the array to fetch.</param>
		/// <returns></returns>
		//
		public Expression ArrayAt (int index)
		{
			if (m_type != ExpressionType.Array)
				throw new InvalidExpressionTypeException("Expected Array");

			return m_array.list[index];
		}

		//
		/// <summary>
		/// Add an element to the end of the array.
		/// </summary>
		/// <param name="element">The element to add.</param>
		//
		public void ArrayAddElementToEnd (Expression element)
		{
			if (m_type != ExpressionType.Array)
				throw new InvalidExpressionTypeException("Expected Array");

			m_array.list.Add(element);
		}

		// --- public Map

		//
		/// <summary>
		/// Return the number of key-value pairs in the map.
		/// </summary>
		//
		public int MapCount
		{
			get {
				if (m_type != ExpressionType.Map)
					throw new InvalidExpressionTypeException("Expected Map");

				return m_map.hash.Count;
			}
		}

		//
		/// <summary>
		/// Return the key at a given index within the map.
		/// </summary>
		/// <param name="index">The index in the map to fetch the key of.</param>
		/// <returns>The key at the given index.</returns>
		//
		public string MapKeyAt (int index)
		{
			if (m_type != ExpressionType.Map)
				throw new InvalidExpressionTypeException("Expected Map");

			return m_map.hash.Keys.ElementAt(index);
		}

		//
		/// <summary>
		/// Return the value at a given index within the map.
		/// </summary>
		/// <param name="index">The index in the map to fetch the value of.</param>
		/// <returns>The value at the given index.</returns>
		//
		public Expression MapValueAt (int index)
		{
			if (m_type != ExpressionType.Map)
				throw new InvalidExpressionTypeException("Expected Map");

			return m_map.hash.Values.ElementAt(index);
		}

		//
		/// <summary>
		/// Return the value for a given key within the map, or null if not found.
		/// </summary>
		/// <param name="key">The key to fetch the value of.</param>
		/// <returns>The value, or null if not found</returns>
		//
		public Expression MapValueForKey (string key)
		{
			if (m_type != ExpressionType.Map)
				throw new InvalidExpressionTypeException("Expected Map");

			if (!m_map.hash.ContainsKey(key))
				return null;

			return m_map.hash[key];
		}

		//
		/// <summary>
		/// Set the value for a given key in the map.
		/// </summary>
		/// <param name="key">The key to assign the value to.</param>
		/// <param name="value">The value to use.</param>
		//
		public void MapSetValueForKey (string key, Expression value)
		{
			if (m_type != ExpressionType.Map)
				throw new InvalidExpressionTypeException("Expected Map");

			m_map.hash[key] = value;
		}


		// --- private Types

		private struct ExpressionPrivateValue
		{
			public void Init ()
			{
				data = "";
			}

			public string data;
		}

		private struct ExpressionPrivateMap
		{
			public void Init ()
			{
				hash = new Dictionary<string, Expression>();
			}

			public Dictionary<string, Expression> hash;
		}

		private struct ExpressionPrivateArray
		{
			public void Init()
			{
				list = new List<Expression>();
			}

			public List<Expression> list;
		}

		private struct ExpressionPrivateBinaryData
		{
			public void Init()
			{
				data = null;
			}

			public byte[] data;
		}

		private struct PrivateWexprStringValue
		{
			public string value; // The value parser
			public int endIndex; // index the end was found (past the value)
		}

		private struct PrivateWexprValueStringProperties
		{
			public bool isBarewordSafe;
			public bool needsEscaping;
			public int writeByteSize; // not counting quotes if not bareword safe, but counting escapes
		}
		// --- private

		//
		/// <summary>
		/// Default constructor. Private so you use the public static versions.
		/// </summary>
		//
		private Expression()
		{ }

		private void p_copyFrom (Expression rhs)
		{
			// copy recursively
			switch (rhs.ExpressionType)
			{
				case ExpressionType.Value:
				{
					m_type = ExpressionType.Value;
					m_value = new ExpressionPrivateValue();
					m_value.Init();
					m_value.data = rhs.m_value.data;
					break;
				}

				case ExpressionType.BinaryData:
				{
					m_type = ExpressionType.BinaryData;
					m_binaryData = new ExpressionPrivateBinaryData();
					m_binaryData.Init();
					m_binaryData.data = rhs.m_binaryData.data;
					break;
				 }

				case ExpressionType.Array:
				{
					m_type = ExpressionType.Array;
					m_array = new ExpressionPrivateArray();
					m_array.Init();

					for (int i=0; i < rhs.m_array.list.Count; ++i)
					{
						var child = rhs.ArrayAt(i);
						var childCopy = child.Copy();

						// add to our array
						ArrayAddElementToEnd(childCopy);
					}
					break;
				}

				case ExpressionType.Map:
				{
					m_type = ExpressionType.Map;
					m_map = new ExpressionPrivateMap();
					m_map.Init();
					for (var i=0; i < rhs.MapCount; ++i)
					{
						MapSetValueForKey(
							rhs.MapKeyAt(i),
							rhs.MapValueAt(i).Copy()
						);
					}

					break;
				}

				default:
				{ break; } // ignore
			}
		}

		// returns the part of the buffer remaining
		// will load into self, setting up everything. Assumes we're empty/null to start.
		private byte[] p_parseFromBinaryChunk (byte[] data)
		{
			if (data.Length < (1 + sizeof(byte))) // minimum of 1
			{
				throw new BinaryChunkNotBigEnoughException("Chunk not big enough for header");
			}

			byte[] buf = data;

			UInt64 usize = 0;
			var dataNewPos = UVLQ64.Read(
				buf, out usize
			);
			int size = (int)usize;

			var sizeSize = dataNewPos;
			var chunkType = buf[sizeSize];

			int readAmount = (int)sizeSize + sizeof(byte);

			if (chunkType == (byte)ExpressionType.Null)
			{
				// nothing more to do
				ChangeType(ExpressionType.Null);

				return data.Skip(readAmount).ToArray();
			}

			else if (chunkType == (byte)ExpressionType.Value)
			{
				// data is the entire binary data
				ChangeType(ExpressionType.Value);

				Value = System.Text.UTF8Encoding.UTF8.GetString(buf, readAmount, size);
				readAmount += size;

				return data.Skip(readAmount).ToArray();
			}

			else if (chunkType == (byte)ExpressionType.Array)
			{
				// data is child chunks
				ChangeType(ExpressionType.Array);

				int curPos = 0;

				// build children as needed
				while (curPos < size)
				{
					// read a new element
					int startSize = size - curPos;
					var inBuf = data.Skip(readAmount+curPos).Take(startSize).ToArray();

					var childExpr = CreateInvalid();
					var remaining = childExpr.p_parseFromBinaryChunk(
						inBuf
					);

					curPos += (startSize - remaining.Length);

					// otherwise, add it
					ArrayAddElementToEnd(childExpr);
				}

				readAmount += curPos;
				return data.Skip(readAmount).ToArray();
			}

			else if (chunkType == (byte)ExpressionType.Map)
			{
				// data is key, value chunks
				ChangeType(ExpressionType.Map);
				int curPos = 0;

				// build children as needed
				while (curPos < size)
				{
					// read a new key
					int startSize = size - curPos;
					var inBuf = data.Skip(readAmount+curPos).Take(startSize).ToArray();
					var keyExpression = Expression.CreateInvalid();
					var remaining = keyExpression.p_parseFromBinaryChunk(
						inBuf
					);

					var keySize = (startSize - remaining.Length);
					curPos += keySize;

					// now parse the value
					var valueExpression = Expression.CreateInvalid();
					remaining = valueExpression.p_parseFromBinaryChunk(
						remaining
					);

					curPos += (startSize - remaining.Length - keySize);

					// now add it
					MapSetValueForKey(keyExpression.Value, valueExpression);
				}

				readAmount += curPos;
				return data.Skip(readAmount).ToArray();
			}

			else if (chunkType == (byte)ExpressionType.BinaryData)
			{
				// data is the entire binary data
				// first byte is the compression
				byte compression = buf[readAmount];

				if (compression != 0x00)
				{
					throw new BinaryUnknownCompressionException("Unknown compresion method to use");
				}

				// raw compression
				ChangeType(ExpressionType.BinaryData);
				BinaryData = data.Skip(readAmount+1).Take(size-1).ToArray();

				readAmount += size;

				return data.Skip(readAmount).ToArray();
			}

			else
			{
				throw new BinaryChunkNotBigEnoughException("Unknown chunk type to read");
			}

			// TODO
			throw new NotImplementedException();
		}

		// returns the part of the string remaining
		// will load into self, setting up everything. Assumes we're mpty/null to start.
		private string p_parseFromString (string str, ParseFlags parseFlags, Internal.ParserState parserState)
		{
			if (str.Length == 0)
			{
				throw new EmptyStringException(parserState.Line, parserState.Column, "Was told to parse an empty string");
			}

			str = s_trimFrontOfString(str, parserState);
			if (str.Length == 0)
			{
				return "";
			}

			// start parsing types:
			// if first two characters are #(, we're an array.
			// if @( we're a map.
			// if [] we're a ref.
			// if < we're a binary string
			// otherwise, we're a value.

			if (str.Length >= 2 && str.Substring(0, 2) == "#(")
			{
				// We're an array
				m_type = ExpressionType.Array;
				m_array.Init();

				// move our string forward
				str = str.Substring(2);
				parserState.Column += 2;

				// continue building children as needed
				while (true)
				{
					str = s_trimFrontOfString(str, parserState);

					if (str.Length == 0)
					{
						throw new ArrayMissingEndParenException(parserState.Line, parserState.Column, "An Array was missing its ending paren");
					}

					if (str.Substring(0, 1) == ")") // end array
					{
						break; // done
					}
					else
					{
						// parse as a new expression
						var newExpression = CreateNull();
						str = newExpression.p_parseFromString(str, parseFlags, parserState);

						// add it to our array
						ArrayAddElementToEnd(newExpression);
					}
				}

				str = str.Substring(1); // remove the end array
				parserState.Column += 1;

				// done with array
				return str;
			}

			else if (str.Length >= 2 && str.Substring(0, 2) == "@(")
			{
				// We're a map
				m_type = ExpressionType.Map;
				m_map.Init();

				// move our string accordingly
				str = str.Substring(2);
				parserState.Column += 2;

				// build our children as needed
				while (true)
				{
					str = s_trimFrontOfString(str, parserState);

					if (str.Length == 0)
					{
						throw new MapMissingEndParenException(parserState.Line, parserState.Column, "A Map was missing its ending paren");
					}

					if (str.Length >= 1 && str.Substring(0, 1) == ")") // end map
					{
						break;
					}
					else
					{
						// parse as a new expression - we'll alternate keys and values
						// keep our previous position just in case the value is bad.
						int prevLine = parserState.Line;
						int prevColumn = parserState.Column;

						var keyExpression = CreateNull();
						str = keyExpression.p_parseFromString(str, parseFlags, parserState);

						if (keyExpression.ExpressionType != ExpressionType.Value)
						{
							throw new MapKeyMustBeAValueException(prevLine, prevColumn, "Map keys must be a value");
						}

						var valueExpression = CreateInvalid();
						try
						{
							str = valueExpression.p_parseFromString(str, parseFlags, parserState);
						}
						catch (EmptyStringException)
						{
							// we ignore ESEs because we want to consider that no value for a better error.
							// if not, the exception is good.
						}

						if (valueExpression.ExpressionType == ExpressionType.Invalid)
						{
							throw new MapNoValueException(prevLine, prevColumn, "Map key must have a value");
						}

						// ok now we have the key and the value
						// both malloc so we can free later
						MapSetValueForKey(keyExpression.Value, valueExpression);
					}
				}

				// remove the end map
				str = str.Substring(1);
				parserState.Column += 1;

				// done with map
				return str;
			}

			else if (str.Length >= 1 && str.Substring(0, 1) == "[")
			{
				// the current expression being processed is the one the attribute will be linked to.

				// process till the closing ]
				var endingBracketIndex = str.IndexOf(']');
				if (endingBracketIndex == -1)
				{
					throw new ReferenceMissingEndBracketException(parserState.Line, parserState.Column, "A reference [] is missing its ending bracket");
				}

				var refName = str.Substring(1, endingBracketIndex - 1);

				// validate the contents
				var invalidName = false;
				for (int i = 0; i < refName.Length; ++i)
				{
					char v = refName[i];

					bool isAlpha = (v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z');
					bool isNumber = (v >= '0' && v <= '9');
					bool isUnder = (v == '_');

					if (i == 0 && (isAlpha || isUnder))
					{ }
					else if (i != 0 && (isAlpha || isNumber || isUnder))
					{ }
					else
					{
						invalidName = true;
						break;
					}
				}

				if (invalidName)
				{
					throw new ReferenceInvalidNameException(parserState.Line, parserState.Column, "A reference doesn't have a valid name");
				}

				// move forward
				parserState.MoveForwardBasedOnString(str.Substring(0, endingBracketIndex + 1));
				str = str.Substring(endingBracketIndex + 1);

				// continue parsing at the same level : stored the reference name
				var resultString = p_parseFromString(str, parseFlags, parserState);

				// now bind the ref - creating a copy of what was made. This will be used for the template.
				parserState.InternalReferenceMap.SetExpressionForKey(refName, Copy());

				// and continue
				return resultString;
			}

			else if (str.Length >= 2 && str.Substring(0, 2) == "*[")
			{
				// parse the reference name
				var endingBracketIndex = str.IndexOf(']');
				if (endingBracketIndex == -1)
				{
					throw new ReferenceInsertMissingEndBracketException(parserState.Line, parserState.Column, "A reference insert *[] is missing its ending bracket");
				}

				var refName = str.Substring(2, endingBracketIndex - 2);

				// move forward
				parserState.MoveForwardBasedOnString(str.Substring(0, endingBracketIndex + 1));
				str = str.Substring(endingBracketIndex + 1);

				var referenceExpr = parserState.InternalReferenceMap.ExpressionForKey(refName);
				if (referenceExpr == null)
				{
					// try again with the external if we have it
					if (parserState.ExternalReferenceMap != null)
					{
						referenceExpr = parserState.ExternalReferenceMap.ExpressionForKey(refName);
					}
				}

				if (referenceExpr == null)
				{
					// not found
					throw new ReferenceUnknownReferenceException(parserState.Line, parserState.Column, "Tried to insert a reference, but couldn't find it.");
				}

				// copy this into ourself
				p_copyFrom(referenceExpr);

				return str;
			}

			// null expressions will be treated as a value, and then parsed seperately.

			else if (
				str.Length >= 1 && str.Substring(0, 1) == "<"
			)
			{
				// look for the ending <
				var endingQuote = str.IndexOf('>');
				if (endingQuote == -1)
				{
					// not found
					throw new BinaryDataNoEndingException(parserState.Line, parserState.Column, "Tried to find the ending > for binary data, but not found.");
				}

				byte[] data;
				try
				{
					data = System.Convert.FromBase64String(str.Substring(1, endingQuote - 1)); // -1 for starting quote. ending was not part.
				}
				catch (Exception)
				{
					throw new BinaryDataInvalidBase64Exception(parserState.Line, parserState.Column, "Unable to decode the base64 data.");
				}

				m_type = ExpressionType.BinaryData;
				m_binaryData.data = data;

				parserState.MoveForwardBasedOnString(str.Substring(0, endingQuote + 1));

				return str.Substring(endingQuote + 1);
			}

			else if (str.Length >= 1) // its a value: must be at least one character
			{
				var val = s_createValueOfString(str, parserState);

				// was it a null/nil string?
				if (val.value == "nil" || val.value == "null")
				{
					m_type = ExpressionType.Null;
				}
				else
				{
					m_type = ExpressionType.Value;
					m_value.data = val.value;
				}

				parserState.MoveForwardBasedOnString(str.Substring(0, val.endIndex));

				return str.Substring(val.endIndex);
			}

			// otherwise we have no idea what happened
			return "";
		}

		// Human Readable notes:
		// even though you pass an indent, we assume you're already indented for the start of the object
		// we assume this so that an object for example as a key-value will be writen in the correct spot.
		// if it writes multiple lines, we will use the given indent to predict.
		// it will end after writing all data, no newline generally at the end.
		string p_appendStringRepresentationToBuffer (WriteFlags flags, int indent, string strBuffer)
		{
			bool writeHumanReadable = (flags & WriteFlags.HumanReadable) == WriteFlags.HumanReadable;
			var type = ExpressionType;

			var buffer = strBuffer;

			if (type == ExpressionType.Null)
			{
				buffer += "null";
				return buffer;
			}

			else if (type == ExpressionType.Value)
			{
				// value - always write directly
				var value = Value;

				var props = s_wexprValueStringProperties(value);

				buffer += s_stringEscaped(value, props);
				return buffer;
			}

			else if (type == ExpressionType.BinaryData)
			{
				// binary data - encode as Base64
				byte[] buf = BinaryData;

				var str = System.Convert.ToBase64String(buf);

				buffer += "<";
				buffer += str;
				buffer += ">";

				return buffer;
			}

			else if (type == ExpressionType.Array)
			{
				var arraySize = ArrayCount;

				if (arraySize == 0)
				{
					// straightforward, always empty structure
					buffer += "#()";
					return buffer;
				}

				// otherwise, we have items

				// array : human readable we'll write each one on its own line.
				if (writeHumanReadable)
					buffer += "#(\n";
				else
					buffer += "#(";

				for (var i=0; i < arraySize; ++i)
				{
					var obj = ArrayAt(i);

					// if human readable, we need to indent the line, output the object, then add a newline
					if (writeHumanReadable)
					{
						buffer += s_indent(indent+1);

						// now add our normal
						buffer += obj.p_appendStringRepresentationToBuffer(flags, indent+1, "");

						// add the newline
						buffer += "\n";
					}

					// if not human readable, we just need to either output the object, or put a space then the object
					else
					{
						if (i > 0)
						{
							// need a space
							buffer += ' ';
						}

						// now add our normal
						buffer += obj.p_appendStringRepresentationToBuffer(flags, indent, "");
					}
				}

				// done with the core of the array
				// if human readable, indent and add the end array
				// otherwise just add the end array
				if (writeHumanReadable)
				{
					buffer += s_indent(indent);
				}

				buffer += ')';

				// and done
				return buffer;
			}

			else if (type == ExpressionType.Map)
			{
				var mapSize = MapCount;

				if (mapSize == 0)
				{
					// straightforward, always empty structure
					buffer += "@()";
					return buffer;
				}

				// otherwise, we have items

				// map : human readable we'll write each one on its own line
				if (writeHumanReadable)
					buffer += "@(\n";
				else
					buffer += "@(";

				for (int i=0; i < mapSize; ++i)
				{
					var key = MapKeyAt(i);
					if (key == null)
						continue; // we shouldnt ever get an empty key, but its possible currently in the case of dereffing in a key for some reason : @([a]a b *[a] c)

					var value = MapValueAt(i);

					var keyProps = s_wexprValueStringProperties(key);
					
					// if human reabale, indent the line, output the key, space, object, newline
					if (writeHumanReadable)
					{
						buffer += s_indent(indent + 1);
						buffer += s_stringEscaped(key, keyProps);
						buffer += ' ';
						buffer += value.p_appendStringRepresentationToBuffer(flags, indent + 1, "");

						// add the newline
						buffer += '\n';
					}

					// if not human readable, just output with spaces as needed
					else
					{
						if (i > 0)
						{
							buffer += ' ';
						}

						// now key, space, value
						buffer += s_stringEscaped(key, keyProps);
						buffer += ' ';
						buffer += value.p_appendStringRepresentationToBuffer(flags, indent + 1, "");
					}
				}

				// done with the core of the map
				// if human readable, indent and add the end map
				// otherwise, just add the end map
				if (writeHumanReadable)
				{
					buffer += s_indent(indent);
				}

				buffer += ')';

				// and done
				return buffer;
			}

			else
			{
				throw new NotImplementedException("Unknown type to generate string");
			}
		}

		// --- private static helpers

		private static string s_StartBlockComment = ";(--";
		private static string s_EndBlockComment = "--)";

		private static string s_indent (int index)
		{
			string s = "";
			for (int i = 0; i < index; ++i) { s += "\t"; }
			return s;
		}

		private static bool s_isNewline (char c)
		{
			return (c == '\n');
		}

		private static bool s_isWhitespace (char c)
		{
			// we put '\r' in whitespace and not newline so its counted as a colunmn instead of a line, cause windows.
			// we dont support classic macos style newlines properly as a side effect.
			return (c == ' ' || c == '\t' || c == '\r' || s_isNewline(c));
		}

		private static bool s_isNotBarewordSafe(char c)
		{
			return (c == '*'
				|| c == '#'
				|| c == '@'
				|| c == '(' || c == ')'
				|| c == '[' || c == ']'
				|| c == '^'
				|| c == '<' || c == '>'
				|| c == '"'
				|| c == ';'
				|| s_isWhitespace(c)
			);
		}

		private static bool s_isEscapeValid (char c)
		{
			return (c == '"' || c == 'r' || c == 'n' || c == 't' || c == '\\');
		}

		private static char s_valueForEscape (char c)
		{
			if (c == '"') return '"';
			if (c == 'r') return '\r';
			if (c == 'n') return '\n';
			if (c == 't') return '\t';
			if (c == '\\') return '\\';

			return '\0'; // invalid escape
		}

		private static bool s_requiresEscape (char c)
		{
			return (c == '"' || c == '\r' || c == '\n' || c == '\t' || c == '\\');
		}

		private static char s_escapeForValue (char c)
		{
			// only returns the escape part
			if (c == '"') return '"';
			if (c == '\r') return 'r';
			if (c == '\n') return 'n';
			if (c == '\t') return 't';
			if (c == '\\') return '\\';

			return '\0'; // invalid
		}

		// Trims the given string by removing whitespace or comments from the beginning of the string
		private static string s_trimFrontOfString (string str, Internal.ParserState parserState)
		{
			while (true)
			{
				if (str.Length == 0) // trimmed everything
					return str;

				char first = str[0];

				// skip whitespace
				if (s_isWhitespace(first))
				{
					str = str.Substring(1);

					if (s_isNewline(first))
					{
						parserState.Line += 1;
						parserState.Column = 1;
					}
					else
					{
						parserState.Column += 1;
					}
				}

				// comment
				else if (first == ';')
				{
					bool isTillNewline = true;

					if (str.Length >= 4)
					{
						if (str.Substring(0, 4) == s_StartBlockComment)
						{
							isTillNewline = false;
						}
					}

					int endIndex =
						(isTillNewline
							? str.IndexOf('\n') // end of line
							: str.IndexOf(s_EndBlockComment)
						);

					int lengthToSkip = isTillNewline ? 1 : s_EndBlockComment.Length;

					// Move forward columns/rows as needed
					parserState.MoveForwardBasedOnString(
						str.Substring(0, (endIndex == -1) ? str.Length : (endIndex + lengthToSkip))
					);

					if (endIndex == -1
						|| endIndex > str.Length - lengthToSkip)
					{
						str = ""; // dead
					}
					else // slice
					{
						str = str.Substring(endIndex + lengthToSkip); // skip the comment
					}
				}

				else
				{
					break;
				}
			}

			return str;
		}

		private static PrivateWexprStringValue s_createValueOfString(string str, Internal.ParserState parserState)
		{
			// two pass:
			// first pass, get the length of the size
			// second pass, store the buffer

			int bufferLength = 0;
			bool isQuotedString = false;
			bool isEscaped = false;
			int pos = 0; // position we're parsing at

			if (str[0] == '"')
			{
				isQuotedString = true;
				++pos;
			}

			while (pos < str.Length)
			{
				char c = str[pos];

				if (isQuotedString)
				{
					if (isEscaped)
					{
						// we're in an escape. Is it valid?
						if (s_isEscapeValid(c))
						{
							++bufferLength; // counts
							isEscaped = false; // escape ended
						}
						else
						{
							throw new InvalidStringEscapeException(parserState.Line, parserState.Column, "Invalid escape found in teh string");
						}
					}
					else
					{
						if (c == '"')
						{
							// end quote - part of us
							++pos;
							break;
						}
						else if (c == '\\')
						{
							// we're escaping
							isEscaped = true;
						}
						else
						{
							// otherwise it's a character
							++bufferLength;
						}
					}
				}
				else
				{
					// have we neded the word?
					if (s_isNotBarewordSafe(c))
					{
						// ended - not part of us
						break;
					}

					// otherwise, it's a character
					++bufferLength;
				}

				++pos;
			}

			if (bufferLength == 0 && !isQuotedString) // cannot have an empty barewords string
			{
				throw new EmptyStringException(parserState.Line, parserState.Column, "Was told to parse an empty string");
			}

			int end = pos;

			// we now know our buffer size and the string has been checked
			string buffer = "";
			int writePos = 0;
			pos = 0;
			if (isQuotedString) pos = 1;

			while (writePos < bufferLength)
			{
				char c = str[pos];

				if (isQuotedString)
				{
					if (isEscaped)
					{
						char escapedValue = s_valueForEscape(c);
						buffer += escapedValue;
						++writePos;
						isEscaped = false;
					}
					else
					{
						if (c == '\\')
						{
							// we're escaping
							isEscaped = true;
						}
						else
						{
							// otherwise it's a character
							buffer += c;
							++writePos;
						}
					}
				}
				else
				{
					// its a character
					buffer += c;
					++writePos;
				}

				// next character
				++pos;
			}

			var ret = new PrivateWexprStringValue();
			ret.value = buffer;
			ret.endIndex = end;

			return ret;
		}

		private PrivateWexprValueStringProperties s_wexprValueStringProperties (string str)
		{
			var props = new PrivateWexprValueStringProperties();

			props.isBarewordSafe = true; // default to being safe
			props.needsEscaping = false; // but we dont need escaping
			props.writeByteSize = 0;

			var len = str.Length;

			for (var i=0; i < len; ++i)
			{
				// For now we cant escape so that stays false.
				// bareword safe we'll just check for a few symbols
				char c = str[i];

				// see any symbols that makes it not bareword safe?
				if (s_isNotBarewordSafe(c))
				{
					props.isBarewordSafe = false;
				}

				// we at least write the character
				props.writeByteSize += 1;

				// does it need to be escaped?
				if (c == '"')
					props.writeByteSize += 1; // needs the escape
			}

			if (len == 0)
				props.isBarewordSafe = false; // empty string is not safe, since that will be nothing

			return props;
		}

		private string s_stringEscaped (string str, PrivateWexprValueStringProperties props)
		{
			var buf = "";

			if (!props.isBarewordSafe)
			{
				buf += '\"';
			}

			for (int i=0; i < str.Length; ++i)
			{
				char c = str[i];

				if (s_requiresEscape(c))
				{
					// write it out as an escape
					buf += '\\';
					buf += s_escapeForValue(c);
				}
				else
				{
					buf += c;
				}
			}

			if (!props.isBarewordSafe)
			{
				// add quotes
				buf += '\"';
			}

			return buf;
		}
		// --- private Members

		//
		/// <summary>
		/// The expression type
		/// </summary>
		//
		private ExpressionType m_type = ExpressionType.Invalid;

		//
		/// <summary>
		/// Our internal data as a value.
		/// </summary>
		//
		private ExpressionPrivateValue m_value;

		//
		/// <summary>
		/// Our internal data as a map.
		/// </summary>
		//
		private ExpressionPrivateMap m_map;

		//
		/// <summary>
		/// Our internal data as a array.
		/// </summary>
		//
		private ExpressionPrivateArray m_array;

		//
		/// <summary>
		/// Our internal data as a binary data.
		/// </summary>
		//
		private ExpressionPrivateBinaryData m_binaryData;
	}
}
