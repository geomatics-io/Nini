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
using Nini.Config;
using NUnit.Framework;

namespace Nini.Test.Config
{
	[TestFixture]
	public class XmlConfigSourceTests
	{
		#region Tests
		[Test]
		public void GetConfig ()
		{
			StringWriter textWriter = new StringWriter ();
			XmlTextWriter writer = NiniWriter (textWriter);
			WriteSection (writer, "Pets");
			WriteKey (writer, "cat", "muffy");
			WriteKey (writer, "dog", "rover");
			WriteKey (writer, "bird", "tweety");
			writer.WriteEndDocument ();
			
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (textWriter.ToString ());

			XmlConfigSource source = new XmlConfigSource (doc);
			
			IConfig config = source.Configs["Pets"];
			Assert.AreEqual ("Pets", config.Name);
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual (source, config.ConfigSource);
		}
		
		[Test]
		public void GetString ()
		{
			StringWriter textWriter = new StringWriter ();
			XmlTextWriter writer = NiniWriter (textWriter);
			WriteSection (writer, "Pets");
			WriteKey (writer, "cat", "muffy");
			WriteKey (writer, "dog", "rover");
			WriteKey (writer, "bird", "tweety");
			writer.WriteEndDocument ();
			
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (textWriter.ToString ());

			XmlConfigSource source = new XmlConfigSource (doc);
			IConfig config = source.Configs["Pets"];
			
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
			StringWriter textWriter = new StringWriter ();
			XmlTextWriter writer = NiniWriter (textWriter);
			WriteSection (writer, "Pets");
			WriteKey (writer, "value 1", "49588");
			writer.WriteEndDocument ();
			
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (textWriter.ToString ());

			XmlConfigSource source = new XmlConfigSource (doc);

			IConfig config = source.Configs["Pets"];
			
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
		public void SetAndSave ()
		{
			string filePath = "Test.xml";

			StringWriter textWriter = new StringWriter ();
			XmlTextWriter writer = NiniWriter (textWriter);
			WriteSection (writer, "new section");
			WriteKey (writer, "dog", "Rover");
			WriteKey (writer, "cat", "Muffy");
			writer.WriteEndDocument ();
			
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (textWriter.ToString ());
			doc.Save (filePath);

			XmlConfigSource source = new XmlConfigSource (filePath);
			
			IConfig config = source.Configs["new section"];
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Muffy", config.Get ("cat"));
			
			config.Set ("dog", "Spots");
			config.Set ("cat", "Misha");
			config.Set ("DoesNotExist", "SomeValue");
			
			Assert.AreEqual ("Spots", config.Get ("dog"));
			Assert.AreEqual ("Misha", config.Get ("cat"));
			Assert.AreEqual ("SomeValue", config.Get ("DoesNotExist"));
			Assert.IsFalse (source.IsReadOnly);
			source.Save ();
			
			source = new XmlConfigSource (filePath);
			config = source.Configs["new section"];
			Assert.AreEqual ("Spots", config.Get ("dog"));
			Assert.AreEqual ("Misha", config.Get ("cat"));
			Assert.AreEqual ("SomeValue", config.Get ("DoesNotExist"));
			
			File.Delete (filePath);
		}
		
		[Test]
		public void MergeAndSave ()
		{
			string xmlFileName = "NiniConfig.xml";

			StreamWriter textWriter = new StreamWriter (xmlFileName);
			XmlTextWriter xmlWriter = NiniWriter (textWriter);
			WriteSection (xmlWriter, "Pets");
			WriteKey (xmlWriter, "cat", "Muffy");
			WriteKey (xmlWriter, "dog", "Rover");
			WriteKey (xmlWriter, "bird", "Tweety");
			xmlWriter.WriteEndDocument ();
			xmlWriter.Close ();
			
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine ("cat = Becky"); // overwrite
			writer.WriteLine ("lizard = Saurus"); // new
			writer.WriteLine ("[People]");
			writer.WriteLine (" woman = Jane");
			writer.WriteLine (" man = John");
			IniConfigSource iniSource = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			XmlConfigSource xmlSource = new XmlConfigSource (xmlFileName);

			xmlSource.Merge (iniSource);
			
			IConfig config = xmlSource.Configs["Pets"];
			Assert.AreEqual (4, config.GetKeys ().Length);
			Assert.AreEqual ("Becky", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Saurus", config.Get ("lizard"));
		
			config = xmlSource.Configs["People"];
			Assert.AreEqual (2, config.GetKeys ().Length);
			Assert.AreEqual ("Jane", config.Get ("woman"));
			Assert.AreEqual ("John", config.Get ("man"));
			
			config.Set ("woman", "Tara");
			config.Set ("man", "Quentin");
			
			xmlSource.Save ();
			
			xmlSource = new XmlConfigSource (xmlFileName);
			
			config = xmlSource.Configs["Pets"];
			Assert.AreEqual (4, config.GetKeys ().Length);
			Assert.AreEqual ("Becky", config.Get ("cat"));
			Assert.AreEqual ("Rover", config.Get ("dog"));
			Assert.AreEqual ("Saurus", config.Get ("lizard"));
			
			config = xmlSource.Configs["People"];
			Assert.AreEqual (2, config.GetKeys ().Length);
			Assert.AreEqual ("Tara", config.Get ("woman"));
			Assert.AreEqual ("Quentin", config.Get ("man"));
			
			File.Delete  (xmlFileName);
		}
		#endregion

		#region Private methods
		private XmlTextWriter NiniWriter (TextWriter writer)
		{
			XmlTextWriter result = new XmlTextWriter (writer);
			result.WriteStartDocument ();
			result.WriteStartElement ("Nini");
			
			return result;
		}
		
		private void WriteSection (XmlWriter writer, string sectionName)
		{
			writer.WriteStartElement ("Section");
			writer.WriteAttributeString ("Name", sectionName);
		}
		
		private void WriteKey (XmlWriter writer, string key, string value)
		{
			writer.WriteStartElement ("Key");
			writer.WriteAttributeString ("Name", key);
			writer.WriteAttributeString ("Value", value);
			writer.WriteEndElement ();
		}
		#endregion
	}
}