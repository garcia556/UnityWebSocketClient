using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket
{
	public class Client
	{
		public const string GAMEOBJECT_NAME = "_WebSocketClient_";
		public const string ERR_SOCKET_CLOSED = "Socket is closed";

		public static int CLIENT_ID = 1;

		public GameObject ComponentPrefab;

		// input
		private string url;
		private bool stopTime;
		private System.Action<string> onMessage;
		private int reconnectTriesMax;
		private int reconnectInterval;
		private bool debug;

		// socket
		private WebSocket Server;
		public bool IsError { get { return !string.IsNullOrEmpty(this.ErrorMessage); } }
		public string ErrorMessage
		{
			get
			{
				string err = this.Server.error;
				if (!string.IsNullOrEmpty(err))
					return err;

				// no explicit socket error but connection is closed
				if (!this.Server.IsOpen)
					return ERR_SOCKET_CLOSED;

				return null;
			}
		}

		// internal
		private ClientComponent comp;

		public Client(
			string url,
			System.Action<string> onMessage,
			bool freezeTimeReconnecting,
			int reconnectTriesMax,
			int reconnectInterval,
			bool debug
			)
		{
			this.url = url;
			this.stopTime = freezeTimeReconnecting;
			this.onMessage = onMessage;
			this.reconnectTriesMax = reconnectTriesMax;
			this.reconnectInterval = reconnectInterval;
			this.debug = debug;
		}

		public IEnumerator Connect()
		{
			var compSpawn = new GameObject().AddComponent<ClientComponentSpawn>();
			while ((this.comp = compSpawn.Component) == null) // wait while component is created
				yield return new WaitForEndOfFrame();
			if (this.debug)
				this.comp.EnableUI(this.url, this.Send);
			GameObject.Destroy(compSpawn.gameObject); // cleanup

			// networking
			this.Server = new WebSocket(this.url);
			this.SetStatus("Connecting ...");
			yield return this.Server.Connect();
			if (this.IsError)
			{
				this.SetErrorStatus("Connection failed", this.ErrorMessage);
				yield break;
			}

			this.SetErrorStatus("Connected", "");
			this.comp.StartCoroutine(this.RecvLoop());
		}

		public void SendBinary(byte[] data)
		{
			if (!this.Server.IsOpen) return;
			this.Server.Send(data);
		}

		public void Send(string data)
		{
			if (!this.Server.IsOpen) return;
			this.Server.SendString(data);
		}

		private void SetStatus(string status) { this.comp.SetStatus(status); }
		private void SetError(string status) { this.comp.SetError(status); }
		private void SetErrorStatus(string status, string error) { this.SetStatus(status); this.SetError(error); }

		private IEnumerator RecvLoop()
		{
			while (true)
			{
				this.SetStatus("Recv: Receiving data from socket ...");
				string data = this.Server.RecvString();

				if (this.IsError)
				{
					this.SetErrorStatus("Recv: Error while receiving data", this.ErrorMessage);
					this.comp.StartCoroutine(this.Reconnect());
					yield break;
				}

				if (!string.IsNullOrEmpty(data))
					this.onMessage.Invoke(data);

				yield return new WaitForEndOfFrame();
			}
		}

		public IEnumerator Reconnect()
		{
			float timeScaleOld = Time.timeScale;
			if (this.stopTime)
				Time.timeScale = 0.0f;

			// tries
			for (int i = 1; i <= this.reconnectTriesMax; i++)
			{
				// wait for the next try
				for (int sec = this.reconnectInterval; sec > 0; sec--)
				{
					this.SetStatus(string.Format("Reconnect: try {0} / {1} in {2} seconds ...", i, this.reconnectTriesMax, sec));
					yield return new WaitForSecondsRealtime(1);
				}

				yield return this.Connect();
				if (!this.IsError) // reconnected successfully
				{
					if (this.stopTime)
						Time.timeScale = timeScaleOld;
					yield break;
				}
			}

			this.SetErrorStatus("Client stopped", "Reconnect tries failed");
		}
	}
}
