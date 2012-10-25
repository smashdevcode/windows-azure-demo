using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsAzureDemo.Shared.Enums;

namespace WindowsAzureDemo.Shared.Entities
{
	public class VideoAsset
	{
		public int VideoAssetID { get; set; }
		public int VideoID { get; set; }
		public Video Video { get; set; }
		public int FileType { get; set; }
		public FileType FileTypeEnum
		{
			get { return (FileType)this.FileType; }
			set { this.FileType = (int)value; }
		}
		#region FileTypeExtension
		public string FileTypeExtension
		{
			get
			{
				return FileTypeHelper.GetFileTypeExtension(this.FileTypeEnum);
			}
		}
		#endregion
		#region MediaPlayerType
		public string MediaPlayerType
		{
			get
			{
				switch (this.FileTypeEnum)
				{
					case Enums.FileType.MP4:
						return "video/mp4; codecs=\"h.264\"";
					case Enums.FileType.Unknown:
					case Enums.FileType.WMV:
					case Enums.FileType.XML:
						return null;
					default:
						throw new ApplicationException("Unexpected FileType enum value: " + this.FileTypeEnum.ToString());
				}
			}
		}
		#endregion
		public string MediaServicesAssetID { get; set; }
	}
}
