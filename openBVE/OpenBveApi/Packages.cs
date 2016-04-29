﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Writer;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;

namespace OpenBveApi.Packages
{
	

	/// <summary>Defines an OpenBVE Package</summary>
	[XmlType("Package")]
	public class Package
	{
		/// <summary>The package version</summary>
		[XmlIgnore] 
		public Version PackageVersion;
		/// <summary>The on-disk file of the package (Used during creation)</summary>
		[XmlIgnore]
		public string FileName;
		/// <summary>The package version represented in string format</summary>
		[XmlElement(ElementName = "PackageVersion")]
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		public string Version
		{
			get
			{
				if (this.PackageVersion == null)
					return string.Empty;
				else
					return this.PackageVersion.ToString();
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
					this.PackageVersion = new Version(value);
			}
		}
		/// <summary>The package name</summary>
		public string Name;
		/// <summary>The package author</summary>
		public string Author;
		/// <summary>The package website</summary>
		public string Website;
		/// <summary>The GUID for this package</summary>
		public string GUID;
		/// <summary>Stores the package type- 0 for routes and 1 for trains</summary>
		public PackageType PackageType;
		/// <summary>The file this package was installed from</summary>
		public string PackageFile;
		/// <summary>The package description</summary>
		public string Description;
		/// <summary>The image for this package</summary>
		[XmlIgnore]
		public Image PackageImage;
		/// <summary>The list of dependancies for this package</summary>
		public List<Package> Dependancies;
		/// <summary>The list of packages that this package reccomends you also install</summary>
		public List<Package> Reccomendations;
		/*
		 * These values are used by dependancies
		 * They need to live in the base Package class to save creating another.....
		 */
		/// <summary>The minimum package version</summary>
		[XmlIgnore]
		public Version MinimumVersion;
		[XmlElement(ElementName = "MinimumVersion")]
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		public string MinVersion
		{
			get
			{
				if (this.MinimumVersion == null)
					return string.Empty;
				else
					return this.MinimumVersion.ToString();
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
					this.MinimumVersion = new Version(value);
			}
		}
		/// <summary>The maximum package version</summary>
		[XmlIgnore]
		public Version MaximumVersion;
		[XmlElement(ElementName = "MaximumVersion")]
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		public string MaxVersion
		{
			get
			{
				if (this.MaximumVersion == null)
					return string.Empty;
				else
					return this.MaximumVersion.ToString();
			}
			set
			{
				if (!String.IsNullOrEmpty(value))
					this.MaximumVersion = new Version(value);
			}
		}
	}

	[XmlType("openBVE")]
	public class SerializedPackage
	{
		public Package Package;
	}

	/// <summary>Provides the possible states of a version</summary>
	public enum VersionInformation
	{
		/// <summary>The version was not found in the database</summary>
		NotFound = 0,
		/// <summary>The version is a newer version than that currently installed</summary>
		NewerVersion = 1,
		/// <summary>The version is an older version than that currently installed</summary>
		OlderVersion = 2,
		/// <summary>The version is the same version as that currently installed</summary>
		SameVersion = 3,
	}

	/// <summary>Defines the possible package types</summary>
	public enum PackageType
	{
		/// <summary>The type of package was not found/ undefined</summary>
		NotFound = 0,
		/// <summary>The package is a route</summary>
		Route = 1,
		/// <summary>The package is a train</summary>
		Train = 2,
		/// <summary>The package is a route, utility etc.</summary>
		Other = 3,
	}

	/// <summary>Holds the properties of a file, used during creation of a package.</summary>
	public class PackageFile
	{
		/// <summary>The absolute on-disk path to the file.</summary>
		public string absolutePath;
		/// <summary>The relative path to the file.</summary>
		public string relativePath;
	}


	/// <summary>Provides functions for manipulating OpenBVE packages</summary>
	public static class Manipulation
	{
		/// <summary>This extracts a package, and returns the list of extracted files</summary>
		/// <param name="currentPackage">The package to extract</param>
		/// <param name="extractionDirectory">The directory to extract to</param>
		/// <param name="databaseFolder">The root package database folder</param>
		/// <param name="packageFiles">Returns via 'ref' a string containing a list of files in the package (Used to update the dialog)</param>
		public static void ExtractPackage(Package currentPackage, string extractionDirectory, string databaseFolder, ref string packageFiles)
		{
			using (Stream stream = File.OpenRead(currentPackage.PackageFile))
			{

				var reader = ReaderFactory.Open(stream);
				List<string> PackageFiles = new List<string>();
				while (reader.MoveToNextEntry())
				{
					if (reader.Entry.Key.ToLowerInvariant() == "package.xml" || reader.Entry.Key.ToLowerInvariant() == "package.png" || reader.Entry.Key.ToLowerInvariant() == "package.rtf")
					{
						//Skip package information files
					}
					else
					{
						//Extract everything else, preserving directory structure
						reader.WriteEntryToDirectory(extractionDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						//We don't want to add directories to the list of files
						if (!reader.Entry.IsDirectory)
						{
							PackageFiles.Add(OpenBveApi.Path.CombineFile(extractionDirectory , reader.Entry.Key));
						}
					}
				}
				string Text = "";
				foreach (var FileName in PackageFiles)
				{
					Text += FileName + "\r\n";
				}
				packageFiles = Text;
				//Write out the package file list
				var fileListDirectory = OpenBveApi.Path.CombineDirectory(databaseFolder, "Installed");
				if (!Directory.Exists(fileListDirectory))
				{
					Directory.CreateDirectory(fileListDirectory);
				}
				var fileList = OpenBveApi.Path.CombineFile(fileListDirectory, currentPackage.GUID.ToUpper() + ".xml");
				using (StreamWriter sw = new StreamWriter(fileList))
				{
					XmlSerializer listWriter = new XmlSerializer(typeof(List<string>));
					listWriter.Serialize(sw, PackageFiles);
				}
			}
		}

		/// <summary>Creates a new packaged archive</summary>
		/// <param name="currentPackage">The package data we wish to compress into an archive</param>
		/// <param name="packageFile">The filename to save the package as</param>
		/// <param name="packageImage">The path to the image for this package, if applicable</param>
		/// <param name="packageFiles">The list of files to save within the package</param>
		public static void CreatePackage(Package currentPackage, string packageFile, string packageImage, List<PackageFile> packageFiles)
		{
			//TEMP
			File.Delete(packageFile);

			using (var zip = File.OpenWrite(packageFile))
			{
				using (var zipWriter = WriterFactory.Open(zip, SharpCompress.Common.ArchiveType.Zip, CompressionType.LZMA))
				{
					foreach (PackageFile currentFile in packageFiles)
					{
						//Add file to archive
						zipWriter.Write(currentFile.relativePath, currentFile.absolutePath);
					}
					//Create temp directory and XML file
					var tempXML = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName() + "package.xml";
					Directory.CreateDirectory(System.IO.Path.GetDirectoryName(tempXML));
					using (StreamWriter sw = new StreamWriter(tempXML))
					{
						//TODO: Let's see if we can get the serializer working everywhere in the solution.....
						//Haven't checked whether these are read by the reader yet.
						XmlSerializer listWriter = new XmlSerializer(typeof(SerializedPackage));
						listWriter.Serialize(sw, new SerializedPackage{Package = currentPackage});
					}
					//Write out XML
					zipWriter.Write("Package.xml",tempXML);
					//Write out image
					if (System.IO.File.Exists(packageImage))
					{
						zipWriter.Write("Package.png", packageImage);
					}
				}
			}
		}

		/// <summary>Uninstalls a package</summary>
		/// <param name="currentPackage">The package to uninstall</param>
		/// <param name="databaseFolder">The package database folder</param>
		/// <param name="PackageFiles">Returns via 'ref' a list of files uninstalled</param>
		/// <returns>True if uninstall succeeded with no errors, false otherwise</returns>
		public static bool UninstallPackage(Package currentPackage, string databaseFolder, ref string PackageFiles)
		{
			var fileList = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(databaseFolder, "Installed"), currentPackage.GUID + ".xml");
			if (!File.Exists(fileList))
			{
				PackageFiles = null;
				//The list of files installed by this package is missing
				return false;
			}
			XmlSerializer listReader = new XmlSerializer(typeof(List<string>));
			List<string> filesToDelete;
			using (FileStream readFileStream = new FileStream(fileList, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				filesToDelete = (List<string>)listReader.Deserialize(readFileStream);
			}
			File.Delete(fileList);
			bool noErrors = true;
			int errorCount = 0;
			int deletionCount = 0;
			string Result = "";
			foreach (var String in filesToDelete)
			{
				try
				{
					File.Delete(String);
					Result += String + " deleted successfully. \r\n ";
					deletionCount++;
				}
				catch (Exception ex)
				{
					//We have caught an error....
					//Set the return type to false, and add the exception to the results string
					noErrors = false;
					Result += String + "\r\n";
					Result += ex.Message + "\r\n";
					errorCount++;
				}
			}
			//Set the final results string to display
			PackageFiles = deletionCount + " files deleted successfully. \r\n" + errorCount + " errors were encountered. \r\n \r\n \r\n" + Result;
			return noErrors;
		}

		/// <summary>Reads the information of the selected package</summary>
		public static Package ReadPackage(string packageFile)
		{
			bool InfoFound = false;
			XmlDocument CurrentXML = new XmlDocument();
			//Create a random temp directory
			string TempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

			Package currentPackage = new Package();
			Directory.CreateDirectory(TempDirectory);
			//Load the selected package file into a stream
			using (Stream stream = File.OpenRead(packageFile))
			{
				
				var reader = ReaderFactory.Open(stream);
				while (reader.MoveToNextEntry())
				{

					//Search for the package.xml file- This must be located in the archive root
					if (reader.Entry.Key.ToLowerInvariant() == "package.xml")
					{
						reader.WriteEntryToDirectory(TempDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						//Load the XML file
						InfoFound = true;
						XmlSerializer listReader = new XmlSerializer(typeof(SerializedPackage));
						SerializedPackage newPackage = (SerializedPackage)listReader.Deserialize(XmlReader.Create(OpenBveApi.Path.CombineFile(TempDirectory, "package.xml")));
						currentPackage = newPackage.Package;
					}
					if (reader.Entry.Key.ToLowerInvariant() == "package.png")
					{
						//Extract the package.png to the uniquely assigned temp directory
						reader.WriteEntryToDirectory(TempDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						try
						{
							currentPackage.PackageImage = Image.FromFile(Path.CombineFile(TempDirectory, "package.png"));
						}
						catch
						{
							//Image loading failed
							currentPackage.PackageImage = null;
						}
					}
					/*
					 * May have to change to plaintext-
					 * No way of easily storing a RTF object....
					 * 
					 */
					if (reader.Entry.Key.ToLowerInvariant() == "package.rtf")
					{
						//Extract the package.rtf description file to the uniquely assigned temp directory
						reader.WriteEntryToDirectory(TempDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						//PackageDescription.LoadFile(OpenBveApi.Path.CombineFile(TempDirectory, "package.rtf"));
					}

				}
			}
			if (!InfoFound)
			{
				//No info found, return null.....
				return null;
			}
			//Read the info

			if (currentPackage.Equals(new Package()))
			{
				//Somewhat hacky way to quickly check if all elements are null....
				return null;
			}
			currentPackage.PackageFile = packageFile;
			return currentPackage;
		}
	}

	/// <summary>Provides information functions for OpenBVE packages</summary>
	public static class Information
	{

		/// <summary>Checks to see if this package is currently installed, and if so whether there is another version installed</summary>
		/// <param name="currentPackage">The package to check</param>
		/// <param name="packageList">The list of currently installed packages</param>
		/// <param name="oldPackage">Returns via 'ref' the current package installed</param>
		/// <returns>Whether the package to check is installed, and if so whether it is an older, newer or identical version</returns>
		public static VersionInformation CheckVersion(Package currentPackage, List<Package> packageList, ref Package oldPackage)
		{
			if (packageList == null)
			{
				//List is null, so we can't possibly be in it
				return VersionInformation.NotFound;
			}
			foreach (var Package in packageList)
			{
				//Check GUID
				if (currentPackage.GUID == Package.GUID)
				{
					oldPackage = currentPackage;
					//GUID found, check versions
					if (currentPackage.PackageVersion == Package.PackageVersion)
					{
						//The versions are the same
						return VersionInformation.SameVersion;
					}
					if (currentPackage.PackageVersion > Package.PackageVersion)
					{
						//This is an older version, so update the ref with the found version number
						return VersionInformation.OlderVersion;
					}
					if (currentPackage.PackageVersion < Package.PackageVersion)
					{
						//This is a newer version, but it's good manners to point out that this will be replacing an older version
						return VersionInformation.NewerVersion;
					}
					
				}
			}
			//We didn't find our package as currently installed
			return VersionInformation.NotFound;
		}

		/// <summary>Checks to see if this package's dependancies are installed</summary>
		public static List<Package> CheckDependancies(Package currentPackage, List<Package> installedRoutes, List<Package> installedTrains)
		{
			foreach (Package currentDependancy in currentPackage.Dependancies.ToList())
			{
				//Itinerate through the routes list
				if (currentDependancy.PackageType == PackageType.Route)
				{
					if (installedRoutes != null)
					{
						foreach (Package Package in installedRoutes)
						{
							//Check GUID
							if (Package.GUID == currentDependancy.GUID)
							{
								if ((currentDependancy.MinimumVersion == null || currentDependancy.MinimumVersion >= Package.PackageVersion) &&
									(currentDependancy.MaximumVersion == null || currentDependancy.MaximumVersion <= Package.PackageVersion))
								{
									//If the version is OK, remove
									currentPackage.Dependancies.Remove(currentDependancy);
								}
							}
						}
					}
				}
				if (currentDependancy.PackageType == PackageType.Train)
				{
					//Itinerate through the trains list
					if (installedTrains != null)
					{
						foreach (Package Package in installedTrains)
						{
							//Check GUID
							if (Package.GUID == currentDependancy.GUID)
							{
								if ((currentDependancy.MinimumVersion == null || currentDependancy.MinimumVersion >= Package.PackageVersion) &&
								    (currentDependancy.MaximumVersion == null || currentDependancy.MaximumVersion <= Package.PackageVersion))
								{
									//If the version is OK, remove
									currentPackage.Dependancies.Remove(currentDependancy);
								}
							}
						}
					}
				}
			}
			if (currentPackage.Dependancies.Count == 0)
			{
				//Return null if there are no unmet dependancies
				return null;
			}
			return currentPackage.Dependancies;
		}

		/// <summary>Checks to see if upgrading or downgrading this package will break any dependancies</summary>
		public static List<Package> UpgradeDowngradeDependancies(Package currentPackage, List<Package> installedRoutes, List<Package> installedTrains)
		{
			List<Package> Dependancies = new List<Package>();
			if (installedRoutes != null)
			{
				foreach (Package routePackage in installedRoutes)
				{
					//Itinerate through the routes list
					foreach (Package Package in routePackage.Dependancies)
					{
						if (Package.GUID == currentPackage.GUID)
						{
							if (Package.MinimumVersion > currentPackage.PackageVersion ||
							    Package.MaximumVersion < currentPackage.PackageVersion)
							{
								Dependancies.Add(Package);
							}
						}
					}

				}
			}
			if (installedTrains != null)
			{
				foreach (Package trainPackage in installedTrains)
				{
					//Itinerate through the routes list
					foreach (Package Package in trainPackage.Dependancies)
					{
						if (Package.GUID == currentPackage.GUID)
						{
							if (Package.MinimumVersion > currentPackage.PackageVersion ||
							    Package.MaximumVersion < currentPackage.PackageVersion)
							{
								Dependancies.Add(Package);
							}
						}
					}

				}
			}
			if (Dependancies.Count == 0)
			{
				//Return null if there are no unmet dependancies
				return null;
			}
			return Dependancies;
		}
	}
}