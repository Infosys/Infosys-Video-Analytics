/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Packaging;
using System.IO;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class Packaging
    {
        private const long BUFFER_SIZE = 4096;
        private static string startingFolder = "";
        static Package zip1;      

       
        public static Result Package(string pyFullyqualifiedFile, string packageFileExtension = "iapd")    
        {
            Result result = null;
            
            if (File.Exists(pyFullyqualifiedFile))
            {
                string folder = Path.GetDirectoryName(pyFullyqualifiedFile);
                startingFolder = folder;
                string packageName = Path.GetFileNameWithoutExtension(pyFullyqualifiedFile) + "." + packageFileExtension;
                result = AddFolder(Path.Combine(folder, packageName), folder);
                result.PackagePath = Path.Combine(folder, packageName);
            }
            return result;
        }

       
        public static Result PackageJustFile(string fullyqualifiedFile, string packageExt)
        {
            Result result = new Result();
            try
            {
               
                if (File.Exists(fullyqualifiedFile))
                {
                    string folder = Path.GetDirectoryName(fullyqualifiedFile);
                    string packageName = Path.Combine(folder, Path.GetFileNameWithoutExtension(fullyqualifiedFile) + "." + packageExt);
                    using (Package zip = System.IO.Packaging.Package.Open(packageName, FileMode.OpenOrCreate))
                    {
                        string destFilename = ".\\" + Path.GetFileName(fullyqualifiedFile);
                        Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                        if (zip.PartExists(uri))
                        {
                            zip.DeletePart(uri);
                        }
                        PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                        using (FileStream fileStream = new FileStream(fullyqualifiedFile, FileMode.Open, FileAccess.Read))
                        {
                            using (Stream dest = part.GetStream())
                            {
                                CopyStream(fileStream, dest);
                            }
                        }
                    }
                    result.PackagePath = Path.Combine(folder, packageName);
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                if (ex.InnerException != null)
                {
                    result.Message += ". Inner Exception Message- " + ex.InnerException.Message;
                }
            }
            return result;
        }

       
        public static Result Package(string xamlName, Stream xamlStream, string dependencyFolderPath="")
        {
            Result result = null;
            if (!string.IsNullOrEmpty(xamlName) && xamlStream.Length > 0)
            {
               
                string xamlString = StreamToString(xamlStream);
                xamlStream.Close();

               
                if (string.IsNullOrEmpty(dependencyFolderPath))
                {
                    dependencyFolderPath = GetAppPath();
                   
                    string newFolder= Path.Combine(dependencyFolderPath, "iapw");
                    if(System.IO.Directory.Exists(newFolder))
                        System.IO.Directory.Delete(newFolder);
                    System.IO.Directory.CreateDirectory(newFolder);                    
                    dependencyFolderPath = newFolder;
                }
               
                string destination = Path.Combine(dependencyFolderPath, xamlName + ".xaml");
                if (File.Exists(destination))
                    File.Delete(destination);
                
                File.WriteAllText(destination, xamlString);

                startingFolder = dependencyFolderPath;
                string packageName = xamlName + ".iapw";
                result = AddFolder(Path.Combine(dependencyFolderPath, packageName), dependencyFolderPath);
                result.PackagePath = Path.Combine(dependencyFolderPath, packageName);
            }
            else
            {
                result = new Result();
                result.IsSuccess = false;
                result.Message = "Invalid XAML name or/and stream";
            }
            return result;
        }

       
        public static Stream ExtractFile(Stream iapdOrIapwStream, string filenameWithrelativepath)
        {
            Stream file = null;
            iapdOrIapwStream.Position = 0;
            zip1 = System.IO.Packaging.Package.Open(iapdOrIapwStream);
            foreach (PackagePart part in zip1.GetParts())
            {
                string pathPart = part.Uri.ToString().Replace(@"/", "\\");
                if (pathPart.ToLower() == filenameWithrelativepath.ToLower())
                {
                    file = part.GetStream();
                    break;
                }
            }            
            return file;
        }

        
        public static Stream ExtractFile(string iapdFileWithpath, string filenameWithrelativepath)
        {
            Stream file = ExtractFile(File.OpenRead(iapdFileWithpath), filenameWithrelativepath);
            return file;
        }

        public static void ClosePackage()
        {
            if (zip1 != null)
                zip1.Close();
        }
       
       
        public static Result Unpackage(string package, string extractToFolder)
        {
            bool overWriteExistingFile = true;
            Result result = new Result();
            result.IsSuccess = true;
            try
            {
                using (Package zip = System.IO.Packaging.Package.Open(package, FileMode.Open))
                {
                    
                    extractToFolder = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(package));
                    foreach (PackagePart part in zip.GetParts())
                    {
                       
                        string destination = extractToFolder + Uri.UnescapeDataString(part.Uri.ToString().Replace(@"/", "\\"));
                        string directoryName = Path.GetDirectoryName(destination);
                        if (!Directory.Exists(directoryName))
                        {
                            var dir = Directory.CreateDirectory(directoryName);
                            
                        }

                        if (File.Exists(destination) && !overWriteExistingFile)
                            continue;
                        string fileName = Path.GetFileName(destination);
                        using (FileStream destFile = new FileStream(destination, FileMode.OpenOrCreate))
                        {
                            CopyStream(part.GetStream(), destFile);
                           
                        }
                    }

                    result.PackagePath = extractToFolder;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                if (ex.InnerException != null)
                {
                    result.Message += ". Inner Exception Message- " + ex.InnerException.Message;
                }
            }
            return result;
        }

        
        private static Result AddFile(string targetCompressedFileName, string fileToBeAdded, string relativeTargetFolder)
        {
            Result result = new Result();
            result.IsSuccess = true;
            try
            {
                using (Package zip = System.IO.Packaging.Package.Open(targetCompressedFileName, FileMode.OpenOrCreate))
                {
                    string destFilename = "";
                    if (!string.IsNullOrEmpty(relativeTargetFolder))
                        destFilename = "." + relativeTargetFolder + "\\" + Path.GetFileName(fileToBeAdded);
                    else
                        destFilename = ".\\" + Path.GetFileName(fileToBeAdded);
                    Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                    if (zip.PartExists(uri))
                    {
                        zip.DeletePart(uri);
                    }
                    PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);

                    using (FileStream fileStream = new FileStream(fileToBeAdded, FileMode.Open, FileAccess.Read))
                    {
                        using (Stream dest = part.GetStream())
                        {
                            CopyStream(fileStream, dest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                if (ex.InnerException != null)
                {
                    result.Message += ". Inner Exception Message- " + ex.InnerException.Message;
                }
            }
            return result;
        }

        
        private static Result AddFolder(string targetCompressedFileName, string folderToBeAdded)
        {
            Result result = new Result();
            result.IsSuccess = true;
            try
            {
                
                string relativeTargetFolder = folderToBeAdded.Replace(startingFolder, "");
                if (Directory.Exists(folderToBeAdded))
                {
                    foreach (string file in Directory.GetFiles(folderToBeAdded))
                    {
                        if (!Path.GetExtension(file).Equals(".iapd") || !Path.GetExtension(file).Equals(".iapw")) 
                            result = AddFile(targetCompressedFileName, file, relativeTargetFolder);
                    }
                    foreach (string folder in Directory.GetDirectories(folderToBeAdded))
                    {
                        result = AddFolder(targetCompressedFileName, folder);
                    }
                }
            }
            catch (Exception ex)
            {

                result.IsSuccess = false;
                result.Message = ex.Message;
                if (ex.InnerException != null)
                {
                    result.Message += ". Inner Exception Message- " + ex.InnerException.Message;
                }
            }
            return result;
        }

       
        private static void CopyStream(System.IO.Stream inputStream, System.IO.Stream outputStream)
        {
            inputStream.Position = 0;
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0; long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }

       
        private static string GetAppPath()
        {
            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (path.Contains(@"file:\\"))
            {
                path = path.Replace(@"file:\\", "");
            }

            else if (path.Contains(@"file:\"))
            {
                path = path.Replace(@"file:\", "");
            }

            return path;
        }

        private static string StreamToString(Stream fileContent)
        {
            fileContent.Position = 0;
            StreamReader reader = new StreamReader(fileContent);
            string fileString = reader.ReadToEnd();
            return fileString;
        }

        

       
        public static List<Stream> ExtractFiles(Stream iapdOrIapwStream, string extension)
        {
            List<Stream> resourceFiles = new List<Stream>(); 
            iapdOrIapwStream.Position = 0;
            zip1 = System.IO.Packaging.Package.Open(iapdOrIapwStream);
            foreach (PackagePart part in zip1.GetParts())
            {
                string pathPart = part.Uri.ToString().Replace(@"/", "\\");
                if (Path.GetExtension(pathPart.ToLower()).Equals(extension.ToLower()))
                    resourceFiles.Add(part.GetStream());
            }
            return resourceFiles;
        }
    }

    public class Result
    {
        bool isSuccess;

        public bool IsSuccess
        {
            get { return isSuccess; }
            set { isSuccess = value; }
        }

        string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public string PackagePath { get; set; }

        public Stream PackageStream { get {
            return new FileStream(PackagePath, FileMode.Open, FileAccess.Read);
        } }
    }
}
