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
		public class MediaPlayerSource
		{
			public string src { get; set; }
			public string type { get; set; }
		}

		private Repository _repository = null;
		private MediaServices _mediaServices = null;

		public VideosAPIController()
		{
			_repository = new Repository();
			_mediaServices = new MediaServices();
		}

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
	}
}
