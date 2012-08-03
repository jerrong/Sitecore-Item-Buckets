using System;

namespace Sitecore.ItemBucket.Kernel.Util
{
	public struct ShortGuid
	{
		#region Static
		
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

		#endregion

		#region Fields

		Guid _guid;
		string _value;

		#endregion

		#region Contructors

	    public ShortGuid(string value)
		{
			_value = value;
			_guid = Decode(value);
		}

	    public ShortGuid(Guid guid)
		{
			_value = Encode(guid);
			_guid = guid;
		}

		#endregion

		#region Properties

		public Guid Guid
		{
			get { return _guid; }
			set
			{
				if (value != _guid)
				{
					_guid = value;
					_value = Encode(value);
				}
			}
		}

		public string Value
		{
			get { return _value; }
			set
			{
				if (value != _value)
				{
					_value = value;
					_guid = Decode(value);
				}
			}
		}

		#endregion

		#region ToString

	
		public override string ToString()
		{
			return _value;
		}

		#endregion

		#region Equals

	
		public override bool Equals(object obj)
		{
			if (obj is ShortGuid)
				return _guid.Equals(((ShortGuid)obj)._guid);
			if (obj is Guid)
				return _guid.Equals((Guid)obj);
			if (obj is string)
				return _guid.Equals(((ShortGuid)obj)._guid);
			return false;
		}

		#endregion

		#region GetHashCode

	
		public override int GetHashCode()
		{
			return _guid.GetHashCode();
		}

		#endregion

		#region NewGuid


		public static ShortGuid NewGuid()
		{
			return new ShortGuid(Guid.NewGuid());
		}

		#endregion

		#region Encode

	
		public static string Encode(string value)
		{
			var guid = new Guid(value);
			return Encode(guid);
		}

	
		public static string Encode(Guid guid)
		{
			string encoded = System.Convert.ToBase64String(guid.ToByteArray());
			encoded = encoded
				.Replace("/", "_")
				.Replace("+", "-");
			return encoded.Substring(0, 22);
		}

		#endregion

		#region Decode


		public static Guid Decode(string value)
		{
			value = value
				.Replace("_", "/")
				.Replace("-", "+");
			byte[] buffer = System.Convert.FromBase64String(value + "==");
			return new Guid(buffer);
		}

		#endregion

		#region Operators

	    public static bool operator ==(ShortGuid x, ShortGuid y)
		{
	        return x._guid == y._guid;
		}

		public static bool operator !=(ShortGuid x, ShortGuid y)
		{
			return !(x == y);
		}

	
		public static implicit operator string(ShortGuid shortGuid)
		{
			return shortGuid._value;
		}

		
		public static implicit operator Guid(ShortGuid shortGuid)
		{
			return shortGuid._guid;
		}
	
		public static implicit operator ShortGuid(string shortGuid)
		{
			return new ShortGuid(shortGuid);
		}

		public static implicit operator ShortGuid(Guid guid)
		{
			return new ShortGuid(guid);
		}

		#endregion
	}
}
