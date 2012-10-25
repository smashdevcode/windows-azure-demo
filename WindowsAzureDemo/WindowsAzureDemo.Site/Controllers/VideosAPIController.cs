using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WindowsAzureDemo.Shared.Data;
using WindowsAzureDemo.Shared.Media;

namespace WindowsAzureDemo.Site.Controllers
{
	public class VideosAPIController : ApiController
	{
		#region Inner Classes
		public class MediaPlayerSource
		{
			public string src { get; set; }
			public string type { get; set; }
		}
		#endregion

		#region Private Fields
		private Repository _repository = null;
		private MediaServices _mediaServices = null;
		#endregion

		#region Constructors
		public VideosAPIController()
		{
			_repository = new Repository();
			_mediaServices = new MediaServices();
		}
		#endregion

		#region Methods
		public List<MediaPlayerSource> GetVideoMediaPlayerSources(int videoID)
		{
			var mediaPlayerSources = new List<MediaPlayerSource>();
			var videoAssets = _repository.GetVideoAssets(videoID);
			foreach (var videoAsset in videoAssets)
			{
				var asset = _mediaServices.GetAsset(videoAsset.MediaServicesAssetID);
				var mediaPlayerSource = new MediaPlayerSource()
				{
					src = _mediaServices.GetAssetSasUrl(asset, videoAsset.FileTypeExtension),
					type = videoAsset.MediaPlayerType
				};
				mediaPlayerSources.Add(mediaPlayerSource);
			}
			return mediaPlayerSources;
		}
		#endregion
	}
}
