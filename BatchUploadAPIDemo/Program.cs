using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatchUploadAPIDemo
{
    class Program
    {
        private static bool selfSigned = true; // Target server is a self-signed server
        private static bool hasBeenInitialized = false;
        private static long DEFAULT_PARTSIZE = 1048576; // Size of each upload in the multipart upload process
        private static string extensionString = ".avi;.asf;.wmv;.mpg;.mpeg;.ps;.ts;.m2v;.mp2;.mod;.mp4;.m4v;.mov;.qt;.3gp;.flv;.f4v;.mp3;.wma;.m2a;.m4a;.f4a"; // File types allowed for upload
        private static string[] files = new string[] { "foobar.mp4" };
        private static string userID = "foo";
        private static string userKey = "bar";
        private static string folderID = "foo-bar";
        private static string fileLocation = "dn";


        public static void Main(string[] args)
        {
            Console.WriteLine();

            // Error handling
            if (args.Length <= 5)
            {
                Usage();
                Environment.Exit(1);
            }

            // Parse parameters
            Common.SetServer(args[0]);
            userID = args[1];
            userKey = args[2];
            folderID = args[3];
            fileLocation = args[4];
            files = new string[args.Length - 5];
            for (int i = 5; i < args.Length; i++)
            {
                files[i - 5] = args[i];
            }

            if (selfSigned)
            {
                // For self-signed servers
                EnsureCertificateValidation();
            }

            try
            {
                // Upload each file given
                for (int i = 0; i < files.Length; i++)
                {
                    string absoluteFilePath = ObtainFile(fileLocation, files[i]);
                    if (absoluteFilePath == null)
                    {
                        Usage();
                        Environment.Exit(1);
                    }

                    string sessionName = RemoteFileAccess.GetFileName(absoluteFilePath);

                    string[] sessionNameSplit = sessionName.Split('.');
                    FileTypeCheck("." + sessionNameSplit[sessionNameSplit.Length - 1]);

                    UploadAPIWrapper.UploadFile(userID, userKey, folderID, sessionName, absoluteFilePath, DEFAULT_PARTSIZE);
                    Console.WriteLine("Upload {0} Complete.", i + 1);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program encountered an error:");
                Console.WriteLine(ex.Message);
            }

            // Delete temp files
            RemoteFileAccess.CleanUp();

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Display parameters of this program to user
        /// </summary>
        public static void Usage()
        {
            Console.WriteLine("Invalid input.");
            Console.WriteLine();
            Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName + " SERVER USERNAME USERPASSWORD FOLDERID FILELOCATION FILEPATH [FILEPATH]");
            Console.WriteLine();
            Console.WriteLine("Upload file at FILELOCATION with path as FILEPATH to session with same name as file in folder with ID as FOLDERID on SERVER logged in with USERNAME and USERPASSWORD");
            Console.WriteLine();
            Console.WriteLine("\tSERVER: Target server");
            Console.WriteLine("\tUSERNAME: User name to access the server");
            Console.WriteLine("\tUSERPASSWORD: Password for the user");
            Console.WriteLine("\tFOLDERID: ID of the destination folder");
            Console.WriteLine("\tFILELOCATION: Location of file (dn - Disk and Network; ftp - FTP Server; w - HTTP URL)");
            Console.WriteLine("\tFILEPATH: Target upload file uri (Can input multiple files, all arguments from here on are taken as file paths");
        }

        /// <summary>
        /// Downloads the file if necessary and returns the file's path
        /// </summary>
        /// <param name="location">Location of the file (Disk, HTTP, FTP, Network, SharePoint)</param>
        /// <param name="uri">URI of file</param>
        /// <returns>Path of obtained file's temporary location</returns>
        public static string ObtainFile(string location, string uri)
        {
            if (location.ToLower().Equals("dn"))
            {
                return Path.GetFullPath(uri);
            }
            else if (location.ToLower().Equals("ftp"))
            {
                return RemoteFileAccess.GetFTPFile(uri);
            }
            else if (location.ToLower().Equals("w"))
            {
                return RemoteFileAccess.GetHTTPFile(uri);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if file extension given is part of legal extensions
        /// </summary>
        /// <param name="fileExt">Extension of file to check</param>
        private static void FileTypeCheck(string fileExt)
        {
            string[] extensions = extensionString.Split(';');

            foreach (string ext in extensions)
            {
                if (ext.Equals(fileExt))
                {
                    return;
                }
            }

            throw new System.IO.InvalidDataException("File Type Not Supported");
        }

        //========================= Needed to use self-signed servers

        /// <summary>
        /// Ensures that our custom certificate validation has been applied
        /// </summary>
        public static void EnsureCertificateValidation()
        {
            if (!hasBeenInitialized)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(CustomCertificateValidation);
                hasBeenInitialized = true;
            }
        }

        /// <summary>
        /// Ensures that server certificate is authenticated
        /// </summary>
        private static bool CustomCertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
    }
}
