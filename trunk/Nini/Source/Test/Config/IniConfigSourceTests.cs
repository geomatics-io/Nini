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
using Nini.Config;
using NUnit.Framework;

namespace Nini.Test.Config
{
	[TestFixture]
	public class IniConfigSourceTests
	{
		[Test]
		public void GetConfig ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			
			IConfig config = source.Configs["Pets"];
			Assert.AreEqual ("Pets", config.Name);
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual (source, config.ConfigSource);
		}
		
		[Test]
		public void GetString ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			IniConfigSource source = 
				new IniConfigSource (new StringReader (writer.ToString ()));
			IConfig config = source.Configs["Test"];
			
			Assert.AreEqual ("muffy", config.Get ("cat"));
			Assert.AreEqual ("rover", config.Get ("dog"));
			Assert.AreEqual ("muffy", config.GetString ("cat"));
			Assert.AreEqual ("rover", config.GetString ("dog"));
			Assert.AreEqual ("my default", config.Get ("Not Here", "my default"));
			Assert.IsNull (config.Get ("Not Here 2"));
		}
		
		[Test]
		public void GetInt ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 49588");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			IConfig config = source.Configs["Test"];
			
			Assert.AreEqual (49588, config.GetInt ("value 1"));
			Assert.AreEqual (12345, config.GetInt ("Not Here", 12345));
			
			try
			{
				config.GetInt ("Not Here Also");
				Assert.Fail ();
			}
			catch
			{
			}
		}
		
		[Test]
		public void GetLong ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 4000000000");
			IniConfigSource source = new IniConfigSource 
										(new StringReader (writer.ToString ()));
			IConfig config = source.Configs["Test"];
			
			Assert.AreEqual (4000000000, config.GetLong ("value 1"));
			Assert.AreEqual (5000000000, config.GetLong ("Not Here", 5000000000));
			
			try
			{
				config.GetLong ("Not Here Also");
				Assert.Fail ();
			}
			catch
			{
			}
		}
		
		[Test]
		public void GetFloat ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 494.59");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			IConfig config = source.Configs["Test"];
			
			Assert.AreEqual (494.59, config.GetFloat ("value 1"));
			Assert.AreEqual ((float)5656.2853, 
							config.GetFloat ("Not Here", (float)5656.2853));
		}

		[Test]
		public void BooleanAlias ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			writer.WriteLine (" bool 3 = ON");
			writer.WriteLine (" bool 4 = OfF");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			IConfig config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			config.Alias.AddAlias ("on", true);
			config.Alias.AddAlias ("off", false);
			
			Assert.IsTrue (config.GetBoolean ("bool 1"));
			Assert.IsFalse (config.GetBoolean ("bool 2"));
			Assert.IsTrue (config.GetBoolean ("bool 3"));
			Assert.IsFalse (config.GetBoolean ("bool 4"));
			Assert.IsTrue (config.GetBoolean ("Not Here", true));
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BooleanAliasNoDefault ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			IniConfigSource source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			IConfig config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			
			Assert.IsTrue (config.GetBoolean ("Not Here", true));
			Assert.IsFalse (config.GetBoolean ("Not Here Also"));
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NonBooleanParameter ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = not boolean");
			IniConfigSource source = new IniConfigSource (new StringReader (writer.ToString ()));

			IConfig config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			
			Assert.IsTrue (config.GetBoolean ("bool 1"));
		}
		
		[Test]
		public void GetIntAlias ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" node type = TEXT");
			writer.WriteLine (" error code = WARN");
			IniConfigSource source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			const int WARN = 100, ERROR = 200;
			IConfig config = source.Configs["Test"];
			config.Alias.AddAlias ("error code", "waRn", WARN);
			config.Alias.AddAlias ("error code", "eRRor", ERROR);
			config.Alias.AddAlias ("node type", new System.Xml.XmlNodeType ());
			config.Alias.AddAlias ("default", "age", 31);
			
			Assert.AreEqual (WARN, config.GetInt ("error code", true));
			Assert.AreEqual ((int)System.Xml.XmlNodeType.Text, 
							 config.GetInt ("node type", true));
			Assert.AreEqual (31, config.GetInt ("default", 31, true));
		}
		
		[Test]
		public void GetKeys ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			writer.WriteLine (" bool 3 = ON");
			IniConfigSource source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			IConfig config = source.Configs["Test"];
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual ("bool 1", config.GetKeys ()[0]);
			Assert.AreEqual ("bool 2", config.GetKeys ()[1]);
			Assert.AreEqual ("bool 3", config.GetKeys ()[2]);
		}

		[Test]
		public void GetValues ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" key 1 = value 1");
			writer.WriteLine (" key 2 = value 2");
			writer.WriteLine (" key 3 = value 3");
			IniConfigSource source = 
					new IniConfigSource (new StringReader (writer.ToString ()));
			
			IConfig config = source.Configs["Test"];
			Assert.AreEqual (3, config.GetValues ().Length);
			Assert.AreEqual ("value 1", config.GetValues ()[0]);
			Assert.AreEqual ("value 2", config.GetValues ()[1]);
			Assert.AreEqual ("value 3", config.GetValues ()[2]);
		}
		
		[Test]
		public void SetAndSave ()
		{
			string filePath = "Test.ini";

			StreamWriter writer = new StreamWriter (filePath);
			writer.WriteLine ("; some comment");
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = Rover");
			writer.WriteLine (""); // empty line
			writer.WriteLine ("; a comment");
			writer.WriteLine (" cat = Muffy");
			writer.Close ();
			
			IniConfigSource source = new IniConfigSource (filePath);
			IConfig config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			config.Set ("dog", "Spots");
			config.Set ("cat", "Misha");
			config.Set ("DoesNotExist", "SomeValue");
			
			Assert.AreEqual ("Spots", config.Get ("dog"));
			Assert.AreEqual ("Misha", config.Get ("cat"));
			Assert.AreEqual ("SomeValue", config.Get ("DoesNotExist"));
			source.Save ();
			
			source = new IniConfigSource (filePath);
			config = source.Configs["new section"];
			Assert.AreEqual ("Spots", config.Get ("dog"));
			Assert.AreEqual ("Misha", config.Get ("cat"));
			Assert.AreEqual ("SomeValue", config.Get ("DoesNotExist"));
			
			File.Delete (filePath);
		}
		
		[Test]
		public void MergeAndSave ()
		{
			string fileName = "NiniConfig.ini";

			StreamWriter fileWriter = new StreamWriter (fileName);
			fileWriter.WriteLine ("[Pets]");
			fileWriter.WriteLine ("cat = Muffy"); // overwrite
			fileWriter.WriteLine ("dog = Rover"); // new
			fileWriter.WriteLine ("bird = Tweety");
			fileWriter.Close ();
			
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine ("cat = Becky"); // overwrite
			writer.WriteLine ("lizard = Saurus"); // new
			writer.WriteLine ("[People]");
			writer.WriteLine (" woman = Jane");
			writer.WriteLine (" man = John");
			IniConfigSource iniSource = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			IniConfigSource source = new IniConfigSource (fileName);

			source.Merge (iniSource);
			
			IConfig config = source.Configs["Pets"];
			Assert.AreEqual (4, config.GetKeys ().Length);
			Assert.AreEqual ("Becky", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Saurus", config.Get ("lizard"));
		
			config = source.Configs["People"];
			Assert.AreEqual (2, config.GetKeys ().Length);
			Assert.AreEqual ("Jane", config.Get ("woman"));
			Assert.AreEqual ("John", config.Get ("man"));
			
			config.Set ("woman", "Tara");
			config.Set ("man", "Quentin");
			
			source.Save ();
			
			source = new IniConfigSource (fileName);
			
			config = source.Configs["Pets"];
			Assert.AreEqual (4, config.GetKeys ().Length);
			Assert.AreEqual ("Becky", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Saurus", config.Get ("lizard"));
			
			config = source.Configs["People"];
			Assert.AreEqual (2, config.GetKeys ().Length);
			Assert.AreEqual ("Tara", config.Get ("woman"));
			Assert.AreEqual ("Quentin", config.Get ("man"));
			
			File.Delete  (fileName);
		}
		
		[Test]
		public void SetAndRemove ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			
			IConfig config = source.Configs["Pets"];
			Assert.AreEqual ("Pets", config.Name);
			Assert.AreEqual (3, config.GetKeys ().Length);
			
			config.Set ("snake", "cobra");
			Assert.AreEqual (4, config.GetKeys ().Length);

			// Test removing			
			Assert.IsNotNull (config.Get ("dog"));
			config.Remove ("dog");
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.IsNull (config.Get ("dog"));
			Assert.IsNotNull (config.Get ("snake"));
		}
		
		[Test]
		public void SaveToNewPath ()
		{
			string filePath = "Test.ini";
			string newPath = "TestNew.ini";

			StreamWriter writer = new StreamWriter (filePath);
			writer.WriteLine ("; some comment");
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = Rover");
			writer.WriteLine (" cat = Muffy");
			writer.Close ();
			
			IniConfigSource source = new IniConfigSource (filePath);
			IConfig config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			source.Save (newPath);
			
			source = new IniConfigSource (newPath);
			config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			File.Delete (filePath);
			File.Delete (newPath);
		}
		
		[Test]
		public void SaveToWriter ()
		{
			string newPath = "TestNew.ini";

			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; some comment");
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = Rover");
			writer.WriteLine (" cat = Muffy");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			Assert.IsNull (source.SavePath);
			IConfig config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			StreamWriter textWriter = new StreamWriter (newPath);
			source.Save (textWriter);
			textWriter.Close (); // save to disk
			
			source = new IniConfigSource (newPath);
			Assert.AreEqual (newPath, source.SavePath);
			config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			File.Delete (newPath);
		}
		
		[Test]
		public void SaveAfterTextWriter ()
		{
			string filePath = "Test.ini";

			StreamWriter writer = new StreamWriter (filePath);
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = Rover");
			writer.Close ();

			IniConfigSource source = new IniConfigSource (filePath);
			Assert.AreEqual (filePath, source.SavePath);
			StringWriter textWriter = new StringWriter ();
			source.Save (textWriter);
			Assert.IsNull (source.SavePath);

			File.Delete (filePath);
		}
		
		[Test]
		public void SaveNewSection ()
		{
			string filePath = "Test.xml";

			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; some comment");
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = Rover");
			writer.WriteLine (" cat = Muffy");
			IniConfigSource source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			
			IConfig config = source.AddConfig ("test");
			Assert.IsNotNull (source.Configs["test"]);
			source.Save (filePath);
			
			source = new IniConfigSource (filePath);
			config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			Assert.IsNotNull (source.Configs["test"]);
			
			File.Delete (filePath);
		}
		
		[Test]
		public void RemoveConfigAndKeyFromFile ()
		{
			string filePath = "Test.ini";

			StreamWriter writer = new StreamWriter (filePath);
			writer.WriteLine ("[test 1]");
			writer.WriteLine (" dog = Rover");
			writer.WriteLine ("[test 2]");
			writer.WriteLine (" cat = Muffy");
			writer.WriteLine (" lizard = Lizzy");
			writer.Close ();

			IniConfigSource source = new IniConfigSource (filePath);
			Assert.IsNotNull (source.Configs["test 1"]);
			Assert.IsNotNull (source.Configs["test 2"]);
			Assert.IsNotNull (source.Configs["test 2"].Get ("cat"));
			
			source.Configs.Remove (source.Configs["test 1"]);
			source.Configs["test 2"].Remove ("cat");
			source.AddConfig ("cause error");
			source.Save ();

			source = new IniConfigSource (filePath);
			Assert.IsNull (source.Configs["test 1"]);
			Assert.IsNotNull (source.Configs["test 2"]);
			Assert.IsNull (source.Configs["test 2"].Get ("cat"));

			File.Delete (filePath);
		}

		[Test]
		public void ToStringTest ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			IniConfigSource source = 
				new IniConfigSource (new StringReader (writer.ToString ()));

			string eol = Environment.NewLine;

			string compare = "[Test]" + eol
							 + "cat = muffy" + eol
							 + "dog = rover" + eol
							 + "bird = tweety" + eol;
			Assert.AreEqual (compare, source.ToString ());
		}

		[Test]
		public void EmptyConstructor ()
		{
			string filePath = "EmptyConstructor.ini";
			IniConfigSource source = new IniConfigSource ();

			IConfig config = source.AddConfig ("Pets");
			config.Set ("cat", "Muffy");
			config.Set ("dog", "Rover");
			config.Set ("bird", "Tweety");
			source.Save (filePath);

			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Tweety", config.Get ("bird"));

			source = new IniConfigSource (filePath);
			config = source.Configs["Pets"];
			
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Tweety", config.Get ("bird"));

			File.Delete (filePath);
		}

		[Test]
		public void Reload ()
		{
			string filePath = "Reload.ini";

			// Create the original source file
			IniConfigSource source = new IniConfigSource ();

			IConfig petConfig = source.AddConfig ("Pets");
			petConfig.Set ("cat", "Muffy");
			petConfig.Set ("dog", "Rover");
			IConfig weatherConfig = source.AddConfig ("Weather");
			weatherConfig.Set ("skies", "cloudy");
			weatherConfig.Set ("precipitation", "rain");
			source.Save (filePath);

			Assert.AreEqual (2, petConfig.GetKeys ().Length);
			Assert.AreEqual ("Muffy", petConfig.Get ("cat"));
			Assert.AreEqual (2, source.Configs.Count);

			// Create another source file to set values and reload
			IniConfigSource newSource = new IniConfigSource (filePath);

			IConfig compareConfig = newSource.Configs["Pets"];
			Assert.AreEqual (2, compareConfig.GetKeys ().Length);
			Assert.AreEqual ("Muffy", compareConfig.Get ("cat"));
			Assert.IsTrue (compareConfig == newSource.Configs["Pets"],
							"References before are not equal");

			// Set the new values to source
			source.Configs["Pets"].Set ("cat", "Misha");
			source.Configs["Pets"].Set ("lizard", "Lizzy");
			source.Configs["Pets"].Set ("hampster", "Surly");
			source.Configs["Pets"].Remove ("dog");
			source.Configs.Remove (weatherConfig);
			source.Save (); // saves new value

			// Reload the new source and check for changes
			newSource.Reload ();
			Assert.IsTrue (compareConfig == newSource.Configs["Pets"],
							"References after are not equal");
			Assert.AreEqual (1, newSource.Configs.Count);
			Assert.AreEqual (3, newSource.Configs["Pets"].GetKeys ().Length);
			Assert.AreEqual ("Lizzy", newSource.Configs["Pets"].Get ("lizard"));
			Assert.AreEqual ("Misha", newSource.Configs["Pets"].Get ("cat"));
			Assert.IsNull (newSource.Configs["Pets"].Get ("dog"));

			File.Delete (filePath);
		}
	}
}