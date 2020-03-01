using System;

namespace Wexpr
{
	//
	/// <summary>
	/// Endian Helpers
	/// </summary>
	//
	public static class Endian
	{
		//
		/// <summary>
		/// Swap a UInt16 bytes
		/// </summary>
		static public UInt16 UInt16Swap (UInt16 v)
		{
			var bytes = BitConverter.GetBytes(v);
			byte[] bytes2 = { bytes[1], bytes[0] };
			return BitConverter.ToUInt16(bytes2, 0);
		}

		//
		/// <summary>
		/// Convert a native UInt16 to big
		/// </summary>
		//
		static public UInt16 UInt16ToBig (UInt16 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt16Swap(v);
			else
				return v;
		}

		//
		/// <summary>
		/// Convert a big uint16 to native
		/// </sumamry>
		//
		static public UInt16 BigUInt16ToNative (UInt16 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt16Swap(v);
			else
				return v;
		}

		//
		/// <summary>
		/// Swap a UInt32 bytes
		/// </summary>
		//
		static public UInt32 UInt32Swap (UInt32 v)
		{
			// C method
			return
				( v >> 24) |
				((v << 8) & 0x00FF0000) |
				((v >> 8) & 0x0000FF00) |
				( v << 24)
			;
		}

		//
		/// <summary>
		/// Convert a native UInt32 to big
		/// </summary>
		//
		static public UInt32 UInt32ToBig (UInt32 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt32Swap(v);
			else
				return v;
		}

		//
		/// <summary>
		/// Convert a big UInt32 to native
		/// </sumamry>
		//
		static public UInt32 BigUInt32ToNative (UInt32 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt32Swap(v);
			else
				return v;
		}

		//
		/// <summary>
		/// Swap a UInt64 bytes
		/// </summary>
		//
		static public UInt64 UInt64Swap (UInt64 v)
		{
			// C method
			return
				( v >> 56) |
				((v << 40) & 0x00FF000000000000ul) |
				((v << 24) & 0x0000FF0000000000ul) |
				((v << 8 ) & 0x000000FF00000000ul) |
				((v >> 8 ) & 0x00000000FF000000ul) |
				((v >> 24) & 0x0000000000FF0000ul) |
				((v >> 40) & 0x000000000000FF00ul) |
				( v << 56)
			;
		}

		//
		/// <summary>
		/// Convert a native UInt64 to big
		/// </summary>
		//
		static public UInt64 UInt64ToBig (UInt64 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt64Swap(v);
			else
				return v;
		}

		//
		/// <summary>
		/// Convert a big UInt64 to native
		/// </sumamry>
		//
		static public UInt64 BigUInt64ToNative (UInt64 v)
		{
			if (BitConverter.IsLittleEndian)
				return UInt64Swap(v);
			else
				return v;
		}
	}
}