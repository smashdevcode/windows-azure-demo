using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WindowsAzureDemo.Shared.Enums;

namespace WindowsAzureDemo.Shared.Entities
{
	public class Video
	{
		public Video()
		{
			this.Assets = new List<VideoAsset>();
		}

		public int VideoID { get; set; }
		public string FileName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime AddedOn { get; set; }
		public int VideoStatus { get; set; }
		public VideoStatus VideoStatusEnum
		{
			get { return (VideoStatus)this.VideoStatus; }
			set { this.VideoStatus = (int)value; }
		}
		public Stream FileData { get; set; }
		public string MediaServicesJobID { get; set; }

		public List<VideoAsset> Assets { get; set; }
	}
}
