#region Copyright
//
// Nini Configuration Project.
// Copyright (C) 2004 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.IO;
using System.Xml;
using System.Text;
using Nini.Config;
using NUnit.Framework;

namespace Nini.Test.Config
{
	[TestFixture]
	public class ArgvConfigSourceTests
	{
		#region Tests
		[Test]
		public void AddSwitch ()
		{
			string[] arguments = new string[] { "--help", "-d", "doc.xml", 
												"/pet:cat"};
			ArgvConfigSource source = new ArgvConfigSource (arguments);

			source.AddSwitch ("Base", "help", "h");
			source.AddSwitch ("Base", "doc", "d");

			IConfig config = source.Configs["Base"];
			Assert.IsNotNull (config.Get ("help"));
			Assert.IsNull (config.Get ("h"));
			Assert.IsNull (config.Get ("not here"));
			Assert.IsNull (config.Get ("pets"));
			Assert.AreEqual ("doc.xml", config.Get ("doc"));
			
			source.AddSwitch ("Pets", "pet", "p");
			config = source.Configs["Pets"];
			Assert.IsNotNull (config.Get ("pet"));
			Assert.AreEqual ("cat", config.Get ("pet"));
		}
		
		[Test]
		public void AddSwitchCase ()
		{
			string[] arguments = new string[] { "-H" };
			ArgvConfigSource source = new ArgvConfigSource (arguments);

			source.AddSwitch ("Base", "help", "h");
			source.AddSwitch ("Base", "heat", "H");

			IConfig config = source.Configs["Base"];
			Assert.IsNull (config.Get ("nothere"));
			Assert.IsNull (config.Get ("help"));
			Assert.IsNotNull (config.Get ("heat"));
		}
		#endregion

		#region Private methods
		#endregion
	}
}