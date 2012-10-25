using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WindowsAzureDemo.Shared.Entities;

namespace WindowsAzureDemo.Shared.Data
{
	public class Repository : IDisposable
	{
		#region Private Fields
		private Context _context;
		#endregion

		#region Constructors
		public Repository()
		{
			_context = new Context();
		}
		#endregion

		#region Methods
		#region Videos
		public List<Video> GetVideos()
		{
			return _context.Videos.OrderByDescending(v => v.AddedOn).ToList();
		}
		public List<Video> GetVideos(Enums.VideoStatus videoStatus)
		{
			return _context.Videos.Where(v => v.VideoStatus == (int)videoStatus).ToList();
		}
		public Video GetVideo(int videoID)
		{
			return _context.Videos.Find(videoID);
		}
		public void InsertOrUpdateVideo(Video video)
		{
			_context.Entry(video).State = video.VideoID == 0 ? EntityState.Added : EntityState.Modified;
			foreach (var videoAsset in video.Assets)
				_context.Entry(videoAsset).State = videoAsset.VideoAssetID == 0 ? EntityState.Added : EntityState.Modified;
			_context.SaveChanges();
		}
		public void DeleteVideo(int videoID)
		{
			var video = new Video() { VideoID = videoID };
			_context.Videos.Attach(video);
			_context.Videos.Remove(video);
			_context.SaveChanges();
		}
		public void DeleteVideo(Video video)
		{
			_context.Videos.Remove(video);
			_context.SaveChanges();
		}
		#endregion
		#region Video Assets
		public List<VideoAsset> GetVideoAssets(int videoID)
		{
			return _context.VideoAssets.Where(va => va.VideoID == videoID).ToList();
		}
		public void InsertOrUpdateVideoAsset(VideoAsset videoAsset)
		{
			_context.Entry(videoAsset).State = videoAsset.VideoAssetID == 0 ? EntityState.Added : EntityState.Modified;
			_context.SaveChanges();
		}
		#endregion
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			_context.Dispose();
		}
		#endregion
	}
}
