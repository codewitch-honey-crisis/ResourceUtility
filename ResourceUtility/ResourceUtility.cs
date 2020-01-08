using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RU 
{ 
	/// <summary>
	/// Provides some utility methods for accessing resources
	/// </summary>
	// if including at the source level this class will be
	// internal instead of public
#if RULIB
	public
#endif
	static partial class ResourceUtility
	{
		/// <summary>
		/// Fills a list with resource names
		/// </summary>
		/// <param name="result">The resulting list filled with the names of the resources or null to create a list</param>
		/// <param name="fullyQualified">True if the names returned should be fully qualified, otherwise false</param>
		/// <param name="assemblies">The assemblies to search or empty to search the default assemblies</param>
		/// <returns>The resulting list filled the names of the resources. This is the same list as <paramref name="result"/> unless it was null, in which case this is a new list</returns>
		public static IList<string> FillNames(IList<string> result = null, bool fullyQualified=true,params Assembly[] assemblies)
		{
			var asms = _GetAssemblies(assemblies);
			if (null == result)
				result = new List<string>();
			// we search whichever assemblies
			// are passed in
			foreach (var asm in asms)
			{
				// here we *want* duplicates so the caller 
				// can handle the scenario.
				var names = asm.GetManifestResourceNames();
				for (var i = 0; i < names.Length; i++)
				{
					var name = names[i];
					if (string.IsNullOrWhiteSpace(name))
						continue; // just in case (sanity check)
					if (!fullyQualified)
					{
						name = GetUnqualifiedName(name);
						if (null == name) continue;
						// need the name before we modded it 
					}
					result.Add(name);
				}
			}
			return result;
		}
		/// <summary>
		/// Returns an unqualified resource name for the specified name
		/// </summary>
		/// <param name="name">The name to unqualify</param>
		/// <returns>The unqualified name or null if the name passed in was invalid</returns>
		public static string GetUnqualifiedName(string name)
		{
			var result = name;
			if (string.IsNullOrWhiteSpace(result))
				return null;
			if (!result.Contains("."))
				return result;
			// check for trailing dots
			// probably never happen but just
			// in case (sanity check)
			while ('.' == result[result.Length-1])
				result = result.Substring(0, result.Length - 1);
			if (string.IsNullOrWhiteSpace(result))
				return null;
			var idx = result.LastIndexOf('.');
			if (0 > idx)
				return null;
			// need the name before we modded it 
			result = result.Substring(0,idx);
			if (string.IsNullOrWhiteSpace(result))
				return null;
			idx = result.LastIndexOf('.');
			if (0 > idx)
				return null;
			result = result.Substring(idx + 1);
			return result;
		}
		/// <summary>
		/// Retrieves the fully qualified name of <paramref name="name"/>
		/// </summary>
		/// <param name="name">The name to look for</param>
		/// <param name="assemblies">The assemblies to search or empty to search the default assemblies</param>
		/// <returns>The fully qualified name for the first instance of <paramref name="name"/> that occurs</returns>
		public static string GetFullyQualifiedName(string name,params Assembly[] assemblies)
		{
			var names =FillNames(null, true, assemblies);
			for(int ic=names.Count,i=0;i<ic;++i)
			{
				var namecmp = names[i];
				var unamecmp = GetUnqualifiedName(namecmp);
				if (null == unamecmp)
					continue;
				if (0 == string.Compare(unamecmp, name, StringComparison.InvariantCulture))
					return namecmp;
			}
			return null;
		}
		/// <summary>
		/// Retrieves a resource stream by index. 
		/// </summary>
		/// <param name="index">The index of the name within FillNames()</param>
		/// <param name="assemblies">The assemblies to search or empty to search the default assemblies</param>
		/// <returns>The stream at the specified index</returns>
		/// <remarks>Make sure to dispose of the stream</remarks>
		public static Stream GetStream(int index,params Assembly[] assemblies)
		{
			if (0 > index)
				throw new ArgumentOutOfRangeException(nameof(index));
			var names = FillNames(null, true, assemblies);
			if (names.Count <= index)
				throw new ArgumentOutOfRangeException(nameof(index));
			return _GetStream(names[index],assemblies);
		}
		/// <summary>
		/// Gets a stream by partial or fully qualified name
		/// </summary>
		/// <param name="name">The name of the stream to fetch</param>
		/// <param name="assemblies">The assemblies to search, or empty to search the default assemblies</param>
		/// <returns>A resource stream or null if it could not be found</returns>
		/// <remarks>Make sure to dispose of the stream</remarks>
		public static Stream GetStream(string name, params Assembly[] assemblies)
		{
			name = GetFullyQualifiedName(name, assemblies);
			return _GetStream(name, assemblies);
		}
		// expects name to be fully qualified
		static Stream _GetStream(string name,Assembly[] assemblies)
		{
			var asms = _GetAssemblies(assemblies);
			// search the assemblies
			foreach(var asm in asms)
			{
				if(-1<Array.IndexOf(asm.GetManifestResourceNames(),name))
				{
					return asm.GetManifestResourceStream(name);
				}
			}
			return null;
		}
		static HashSet<Assembly> _GetAssemblies(Assembly[] assemblies)
		{
			var asms = new HashSet<Assembly>(assemblies);
			// if the caller didn't pass any assemblies
			// we give it some sane defaults:
			// the caller's asm, the entry point
			// asm, and if this is included at the
			// source level, this asm
			if (0 == asms.Count)
			{
				// in case ResourceUtility.cs is included at the source level:
#if !RULIB
				asms.Add(Assembly.GetExecutingAssembly());
#endif
				asms.Add(Assembly.GetEntryAssembly());
				asms.Add(Assembly.GetCallingAssembly());
			}

			return asms;
		}
	}
}
