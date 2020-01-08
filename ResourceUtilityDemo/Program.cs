using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RU;
namespace ResourceUtilityDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Resource List:");
			var fqnames = ResourceUtility.FillNames();
			// could pass false above and then use GetFullyQualifiedName() but this is more efficient
			foreach(var name in fqnames)
			{
				Console.WriteLine(string.Format("  {0} ({1})",ResourceUtility.GetUnqualifiedName(name),name));
			}
			Console.WriteLine();

			var names = ResourceUtility.FillNames(null,false);
			for(int ic=names.Count,i=0;i<ic;++i)
			{
				Console.Write("Get resource by index " + i.ToString());
				var stm = ResourceUtility.GetStream(i);
				if(null!=stm)
				{
					Console.WriteLine(" succeeded.");
					stm.Close();
				} else
					Console.WriteLine(" failed.");
				Console.WriteLine("Fully qualified name of {0} is {1}", names[i], ResourceUtility.GetFullyQualifiedName(names[i]));
				Console.Write("Get resource by name " + names[i].ToString());
				stm = ResourceUtility.GetStream(names[i]);
				if (null != stm)
				{
					Console.WriteLine(" succeeded.");
					stm.Close();
				}
				else
					Console.WriteLine(" failed.");
			}
		}
	}
}
