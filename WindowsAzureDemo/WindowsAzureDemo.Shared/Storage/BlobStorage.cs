using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsAzureDemo.Shared.Storage
{
	public class BlobStorage
	{
		#region Methods
		#region GetClient
		public static CloudBlobClient GetClient()
		{
			// get a reference to the storage account
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting("StorageConnectionString"));

			// return the blob client
			return storageAccount.CreateCloudBlobClient();
		}
		#endregion
		#region GetContainer
		public static CloudBlobContainer GetContainer(string containerName)
		{
			var client = GetClient();

			// get a reference to the container
			var container = client.GetContainerReference(containerName);

			// create the container if it doesn't exist
			container.CreateIfNotExist();

			// set the permissions on the container so that blobs are visible to the public
			container.SetPermissions(new BlobContainerPermissions()
			{
				PublicAccess = BlobContainerPublicAccessType.Blob
			});

			return container;
		}
		#endregion
		#region GetBlob
		public static CloudBlob GetBlob(string containerName, string fileName)
		{
			return GetContainer(containerName).GetBlobReference(fileName);
		}
		#endregion
		#region GetNewBlob
		public static CloudBlob GetNewBlob(string containerName, string fileName, out string newFileName)
		{
			var fileExtension = Path.GetExtension(fileName);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			newFileName = string.Format("{0} {1:yyyyMMddhhmmss}{2}",
				fileNameWithoutExtension, DateTime.Now, fileExtension);
			return GetBlob(containerName, newFileName);
		}
		#endregion
		#region DeleteBlob
		public static void DeleteBlob(string containerName, string fileName)
		{
			var blob = GetBlob(containerName, fileName);
			try
			{
				blob.Delete();
			}
			catch (StorageClientException exc)
			{
				Trace.WriteLine(exc.Message);
			}
		}
		#endregion
		#region UploadBlob
		public static string UploadBlob(string containerName, string fileName, Stream fileData)
		{
			// retrieve reference to the blob
			string newFileName = null;
			var blob = BlobStorage.GetNewBlob(containerName, fileName, out newFileName);

			// create the blob
			blob.UploadFromStream(fileData);

			// return the new file name
			return newFileName;
		}
		#endregion
		#endregion
	}
}
