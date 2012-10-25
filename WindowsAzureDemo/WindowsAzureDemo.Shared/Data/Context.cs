using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using WindowsAzureDemo.Shared.Entities;

namespace WindowsAzureDemo.Shared.Data
{
	public class Context : DbContext
	{
		public Context()
			: base(CloudConfigurationManager.GetSetting("DatabaseConnectionString"))
		{
		}

		public DbSet<Video> Videos { get; set; }
		public DbSet<VideoAsset> VideoAssets { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

			var videoEntity = modelBuilder.Entity<Video>();
			videoEntity.Property(v => v.FileName).HasMaxLength(255).IsRequired();
			videoEntity.Property(v => v.Title).HasMaxLength(100).IsRequired();
			videoEntity.Property(v => v.MediaServicesJobID).HasMaxLength(50);
			videoEntity.Ignore(v => v.VideoStatusEnum);
			videoEntity.Ignore(v => v.FileData);

			var videoAssetEntity = modelBuilder.Entity<VideoAsset>();
			videoAssetEntity.Ignore(va => va.FileTypeEnum);
			videoAssetEntity.Property(v => v.MediaServicesAssetID).HasMaxLength(50).IsRequired();
		}
	}
}
