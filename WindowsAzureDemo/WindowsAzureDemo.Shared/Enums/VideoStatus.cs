using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsAzureDemo.Shared.Enums
{
	public enum VideoStatus
	{
		Pending = 0,
		Processing = 1,
		Processed = 2,
		ProcessingFailed = 3,
		ProcessingCancelled = 4
	}
}
