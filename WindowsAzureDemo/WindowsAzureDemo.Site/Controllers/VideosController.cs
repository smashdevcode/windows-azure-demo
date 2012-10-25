using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WindowsAzureDemo.Shared.Data;
using WindowsAzureDemo.Shared.Entities;
using WindowsAzureDemo.Shared.Media;

namespace WindowsAzureDemo.Site.Controllers
{
    public class VideosController : Controller
	{
		#region Private Fields
		private Repository _repository;
		#endregion

		#region Constructors
		public VideosController()
		{
			_repository = new Repository();
		}
		#endregion

		#region Methods
		#region Index
		public ActionResult Index()
        {
            return View(_repository.GetVideos());
        }
		#endregion
		#region Upload
		public ActionResult Upload()
		{
			return View(new Video());
		}
		[HttpPost]
		public ActionResult Upload(Video video, HttpPostedFileBase file)
		{
			video.FileName = file.FileName;
			video.FileData = file.InputStream;
			video.AddedOn = DateTime.UtcNow;
			video.VideoStatusEnum = Shared.Enums.VideoStatus.Pending;

			var videoProcessor = new VideoProcessor(_repository);
			videoProcessor.SaveVideo(video);

			return RedirectToAction("Index");
		}
		public ActionResult Delete(int videoID)
		{
			var videoProcessor = new VideoProcessor(_repository);
			videoProcessor.DeleteVideo(videoID);
			return RedirectToAction("Index");
		}
		#endregion
		#endregion
	}
}
