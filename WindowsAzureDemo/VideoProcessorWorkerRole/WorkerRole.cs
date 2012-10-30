using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using WindowsAzureDemo.Shared.Data;
using WindowsAzureDemo.Shared.Media;
using WindowsAzureDemo.Shared.Enums;
using Microsoft.WindowsAzure.MediaServices.Client;
using WindowsAzureDemo.Shared.Entities;
using WindowsAzureDemo.Shared.Queue;

namespace VideoProcessorWorkerRole
{
	public class WorkerRole : RoleEntryPoint
	{
		private Repository _repository = null;
		private QueueClient _queueClient = null;
		private MediaServices _mediaServices = null;
		private bool _isStopped = true;

		public override void Run()
		{
			while (!_isStopped)
			{
				try
				{
					// get a list of videos whose status is "processing"
					var videos = _repository.GetVideos(VideoStatus.Processing);

					// for each video...
					foreach (var video in videos)
					{
						VideoStatus? newVideoStatus = null;
						var job = _mediaServices.GetJob(video.MediaServicesJobID);

						Trace.WriteLine(string.Format(string.Format("VideoID {0}: Job state is '{1}'",
							video.VideoID, job.State.ToString())));

						switch (job.State)
						{
							case JobState.Canceled:
							case JobState.Canceling:
								newVideoStatus = VideoStatus.ProcessingCancelled;
								break;
							case JobState.Error:
								newVideoStatus = VideoStatus.ProcessingFailed;
								break;
							case JobState.Finished:
								newVideoStatus = VideoStatus.Processed;
								break;
							case JobState.Processing:
							case JobState.Queued:
							case JobState.Scheduled:
								// nothing to update at this time
								break;
						}
						if (newVideoStatus != null)
						{
							Trace.WriteLine(string.Format(string.Format("VideoID {0}: Has a new status of '{1}'",
								video.VideoID, newVideoStatus.Value.ToString())));

							// update the video's status
							video.VideoStatusEnum = newVideoStatus.Value;

							// if the video has been processed then add the video assets
							if (video.VideoStatusEnum == VideoStatus.Processed)
							{
								// for each job output media asset...
								foreach (var asset in job.OutputMediaAssets)
								{
									var videoAsset = new VideoAsset()
									{
										VideoID = video.VideoID,
										MediaServicesAssetID = asset.Id,
										FileTypeEnum = GetAssetFileType(asset)
									};
									video.Assets.Add(videoAsset);

									Trace.WriteLine(string.Format("VideoID {0}: Added video asset (AssetID: {1}, FileType: {2})",
										video.VideoID, videoAsset.MediaServicesAssetID, videoAsset.FileTypeEnum.ToString()));
								}
							}

							// update the video
							_repository.InsertOrUpdateVideo(video);
						}
					}

					// receive and process all pending messages
					var queueHasMessages = true;
					while (queueHasMessages)
					{
						BrokeredMessage receivedMessage = null;
						receivedMessage = _queueClient.Receive();
						if (receivedMessage != null)
						{
							Trace.WriteLine("Processing message...", receivedMessage.SequenceNumber.ToString());

							// get the video
							var videoID = receivedMessage.GetBody<int>();
							var video = _repository.GetVideo(videoID);
							if (video != null)
							{
								Trace.WriteLine(string.Format("VideoID {0}: Creating encoding job...", video.VideoID));

								// create the encoding job
								var jobID = _mediaServices.CreateEncodingJob(string.Format("VideoID_{0}", video.VideoID), video.FileName);

								// update the video's job ID and set its status to "processing"
								video.MediaServicesJobID = jobID;
								video.VideoStatusEnum = VideoStatus.Processing;
								_repository.InsertOrUpdateVideo(video);

								Trace.WriteLine(string.Format(string.Format("VideoID {0}: Updated video (JobID: '{1}', VideoStatus: {2})",
									video.VideoID, video.MediaServicesJobID, video.VideoStatusEnum.ToString())));

								// mark the message as "complete"
								receivedMessage.Complete();
							}
							else
								// we couldn't find a video... so abandon the message
								receivedMessage.Abandon();
						}
						else
							queueHasMessages = false;
					}

					// sleep the thread
					Thread.Sleep(20000);
				}
				catch (MessagingException e)
				{
					if (!e.IsTransient)
					{
						Trace.WriteLine(e.Message);
						throw;
					}
					Thread.Sleep(10000);
				}
				catch (OperationCanceledException e)
				{
					if (!_isStopped)
					{
						Trace.WriteLine(e.Message);
						throw;
					}
				}
			}
		}
		private FileType GetAssetFileType(IAsset asset)
		{
			var files = asset.Files.ToList().Where(f => FileTypeHelper.GetFileType(f.Name) != FileType.XML).ToList();
			if (files.Count == 1)
				return FileTypeHelper.GetFileType(files[0].Name);
			else
				return FileType.Unknown;
		}

		public override bool OnStart()
		{
			// set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			_repository = new Repository();
			_queueClient = QueueConnector.GetQueueClient();
			_mediaServices = new MediaServices();
			_isStopped = false;
			return base.OnStart();
		}
		public override void OnStop()
		{
			_isStopped = true;
			_repository.Dispose();
			_queueClient.Close();
			_mediaServices.Dispose();
			base.OnStop();
		}
	}
}
