using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
//using iText.Kernel.Pdf;
using File = Google.Apis.Drive.v3.Data.File;

namespace DriveManip
{
    public class GoogleHandler
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";
        UserCredential credential;
        private DriveService service;


        public GoogleHandler()
        {

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public List<File> FetchFiles()
        {
            List<File> output = new List<File>();
            string pageToken = null;
            do
            {
                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.Q = "mimeType = 'application/vnd.google-apps.spreadsheet'";
                listRequest.PageSize = 1000;
                listRequest.Fields = "nextPageToken, files(id, name, mimeType)";
                var result = listRequest.Execute();
                output.AddRange(result.Files);
                pageToken = result.NextPageToken;
            } while (pageToken != null);

            // List files.
            return output;

        }

        public PdfDocument GetPdfDocuments(List<File> Files)
        {
            var outputDoc = new PdfDocument();
            foreach (var file in Files)
            {
                FilesResource.ExportRequest request = service.Files.Export(file.Id, "application/pdf");
                var stream = new System.IO.MemoryStream();
                // Add a handler which will be notified on progress changes.
                // It will notify on each chunk download and when the
                // download is completed or failed.
                request.MediaDownloader.ProgressChanged +=
                    (IDownloadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                            {
                                Console.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                            case DownloadStatus.Completed:
                            {
                                Console.WriteLine("Download complete.");
                                var n = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                                var page1 = n.Pages[0];
                                outputDoc.AddPage(page1);
                                //var n = new PdfDocument(new PdfReader(stream));
                                break;
                            }
                            case DownloadStatus.Failed:
                            {
                                Console.WriteLine("Download failed.");
                                break;
                            }
                        }
                    };
                request.DownloadWithStatus(stream);
            }
            return outputDoc;
        }
    }
}
