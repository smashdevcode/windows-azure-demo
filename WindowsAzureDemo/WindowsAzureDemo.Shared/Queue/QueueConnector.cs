using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsAzureDemo.Shared.Queue
{
	public static class QueueConnector
	{
		#region Properties
		public static QueueClient QueueClient { get; set; }
		#endregion

		#region Methods
		#region GetQueueClient
		public static QueueClient GetQueueClient()
		{
			Initialize();
			return QueueClient;
		}
		#endregion
		#region CreateNamespaceManager
		public static NamespaceManager CreateNamespaceManager()
		{
			// get the service bus connection string
			var serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

			// get the namespace manager
			var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

			return namespaceManager;
		}
		#endregion
		#region Initialize
		public static void Initialize()
		{
			if (QueueClient != null)
				return;

			// get the queue name
			var queueName = CloudConfigurationManager.GetSetting("ServiceBusQueueName");

			// create the queue if it doesn't exist
			var namespaceManager = CreateNamespaceManager();
			if (!namespaceManager.QueueExists(queueName))
				namespaceManager.CreateQueue(queueName);

			// initialize the queue client
			var messagingFactory = MessagingFactory.Create(
				namespaceManager.Address,
				namespaceManager.Settings.TokenProvider);
			QueueClient = messagingFactory.CreateQueueClient(queueName);
		}
		#endregion
		#region SendMessage
		public static void SendMessage(object serializableObject)
		{
			GetQueueClient().Send(new BrokeredMessage(serializableObject));
		}
		#endregion
		#endregion
	}
}
