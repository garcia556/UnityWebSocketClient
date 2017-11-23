using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket
{
	public class ClientComponent : MonoBehaviour
	{
		public UnityEngine.RectTransform panel;

		public UnityEngine.UI.Text lblUrl;
		public UnityEngine.UI.Text lblStatus;
		public UnityEngine.UI.Text lblError;

		private System.Action<string> sendData;

		void Awake()
		{
			this.panel.gameObject.SetActive(false);
		}

		public void EnableUI(string url, System.Action<string> sendData)
		{
			this.lblUrl.text = url;
			this.sendData = sendData;
			this.panel.gameObject.SetActive(true);
		}

		public void SetStatus(string status)
		{
			this.lblStatus.text = status;
		}

		public void SetError(string error)
		{
			this.lblError.text = error;
		}

		public void SendSampleData()
		{
			this.sendData.Invoke("from client");
		}
	}
}
