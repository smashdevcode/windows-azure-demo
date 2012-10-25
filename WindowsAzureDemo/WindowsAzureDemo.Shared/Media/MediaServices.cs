using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WindowsAzureDemo.Shared.Storage;

namespace WindowsAzureDemo.Shared.Media
{
	public class MediaServices : IDisposable
	{
		private CloudMediaContext _context = null;

		public MediaServices()
		{
			var accountName = CloudConfigurationManager.GetSetting("MediaServicesAccountName");
			var accountKey = CloudConfigurationManager.GetSetting("MediaServicesAccountKey");
			_context = new CloudMediaContext(accountName, accountKey);
		}

		#region Assets
		public IAsset GetAsset(string assetID)
		{
			var asset = (from a in _context.Assets
						 where a.Id == assetID
						 select a).FirstOrDefault();
			if (asset == null)
				throw new ApplicationException("Unknown asset: " + assetID);
			return asset;
		}
		public string GetAssetSasUrl(IAsset asset, string fileExtension)
		{
			return GetAssetSasUrl(asset, fileExtension, new TimeSpan(1, 0, 0));
		}
		public string GetAssetSasUrl(IAsset asset, string fileExtension, TimeSpan accessPolicyTimeout)
		{
			var file = (from f in asset.Files
						where f.Name.EndsWith(fileExtension)
						select f).FirstOrDefault();
			if (file != null)
				return GetAssetSasUrl(asset, file, accessPolicyTimeout);
			else
				return null;
		}
		public string GetAssetSasUrl(IAsset asset, IFileInfo file)
		{
			return GetAssetSasUrl(asset, file, new TimeSpan(1, 0, 0));
		}
		public string GetAssetSasUrl(IAsset asset, IFileInfo file, TimeSpan accessPolicyTimeout)
		{
			// check to see if a locator is already available
			// (that doesn't expire for another 30 minutes)
			var locator = (from l in asset.Locators
						   orderby l.ExpirationDateTime descending
						   where l.ExpirationDateTime > DateTime.UtcNow.AddMinutes(30)
						   select l).FirstOrDefault();
			if (locator == null)
			{
				// create a policy for the asset
				IAccessPolicy readPolicy = _context.AccessPolicies.Create("ReadPolicy", accessPolicyTimeout, AccessPermissions.Read);
				locator = _context.Locators.CreateSasLocator(asset, readPolicy, DateTime.UtcNow.AddMinutes(-5));
			}
			Trace.WriteLine("Locator path: " + locator.Path);

			// now take the locator path, add the file name, and build a complete SAS URL to browse to the asset
			var uriBuilder = new UriBuilder(locator.Path);
			uriBuilder.Path += "/" + file.Name;
			Trace.WriteLine("Full URL to file: " + uriBuilder.Uri.AbsoluteUri);

			// return the url
			return uriBuilder.Uri.AbsoluteUri;
		}
		#endregion
		#region Jobs
		public IJob GetJob(string jobID)
		{
			var job = (from j in _context.Jobs
					   where j.Id == jobID
					   select j).FirstOrDefault();
			if (job == null)
				throw new ApplicationException("Unknown job: " + jobID);
			return job;
		}
		public string CreateEncodingJob(string jobIdentifer, string fileName)
		{
			// create an empty asset
			IAsset asset = _context.Assets.CreateEmptyAsset(
				string.Format("Asset_", jobIdentifer), AssetCreationOptions.None);

			// create a locator to get the SAS (shared access signature) URL
			IAccessPolicy writePolicy = _context.AccessPolicies.Create("WriteListPolicy", TimeSpan.FromMinutes(30),
				AccessPermissions.Write | AccessPermissions.List);
			ILocator destinationLocator = _context.Locators.CreateSasLocator(asset, writePolicy, DateTime.UtcNow.AddMinutes(-5));

			// create the reference to the destination container
			var destinationFileUrl = new Uri(destinationLocator.Path);
			var destinationContainerName = destinationFileUrl.Segments[1];

			// get and validate the source blob
			var sourceFileBlob = BlobStorage.GetBlob(VideoProcessor.VIDEOS_CONTAINER, fileName);
			sourceFileBlob.FetchAttributes();
			var sourceLength = sourceFileBlob.Properties.Length;
			Debug.Assert(sourceLength > 0);

			// if we got here then we can assume the source is valid and accessible

			// create destination blob for copy, in this case, we choose to rename the file
			var destinationFileBlob = BlobStorage.GetBlob(destinationContainerName, fileName);
			destinationFileBlob.CopyFromBlob(sourceFileBlob);  // will fail here if project references are bad (the are lazy loaded)

			// check destination blob
			destinationFileBlob.FetchAttributes();
			System.Diagnostics.Debug.Assert(sourceFileBlob.Properties.Length == sourceLength);

			// if we got here then the copy worked

			// publish the asset
			asset.Publish();

			// refresh the asset
			asset = GetAsset(asset.Id);

			// declare a new job
			var job = _context.Jobs.Create(string.Format("EncodingJob_{0}", jobIdentifer));

			// get a media processor reference, and pass to it the name of the 
			// processor to use for the specific task
			var processor = GetMediaProcessorByName("Windows Azure Media Encoder");

			// create a task with the encoding details, using a string preset
			var task = job.Tasks.AddNew(
				string.Format("EncodingTask_MP4_{0}", jobIdentifer),
				processor,
				"H.264 256k DSL CBR",
				TaskCreationOptions.None);

			// Specify the input asset to be encoded
			task.InputMediaAssets.Add(asset);

			// add an output asset to contain the results of the job
			task.OutputMediaAssets.AddNew(string.Format("{0} H264", fileName),
				true, AssetCreationOptions.None);

			// submit the job
			job.Submit();

			// return the job id
			return job.Id;
		}
		#endregion
		#region MediaProcessors
		private IMediaProcessor GetMediaProcessorByName(string mediaProcessorName)
		{
			var mediaProcessor = (from p in _context.MediaProcessors
								  where p.Name == mediaProcessorName
								  select p).FirstOrDefault();
			if (mediaProcessor == null)
				throw new ArgumentException(string.Format("Unknown media processor: {0}", mediaProcessorName));
			return mediaProcessor;
		}
		#endregion

		public void Dispose()
		{
			_context.DetachAll();
		}
	}
}
