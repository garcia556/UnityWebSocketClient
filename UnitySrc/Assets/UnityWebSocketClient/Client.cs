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

		// input
		private string url;
		private bool freezeTimeDuringReconnect;
		private System.Action<string> onMessage;
		private int reconnectTriesMax;
		private int reconnectInterval;

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
		private float timeScaleSaved;

		public Client(
			string url,
			System.Action<string> onMessage,
			bool freezeTimeReconnecting,
			int reconnectTriesMax,
			int reconnectInterval
			)
		{
			this.url = url;
			this.onMessage = onMessage;
			this.freezeTimeDuringReconnect = freezeTimeReconnecting;
			this.reconnectTriesMax = reconnectTriesMax;
			this.reconnectInterval = reconnectInterval;
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

		public IEnumerator Start()
		{
			this.Server = new WebSocket(this.url); // create socket instance
			yield return this.Server.Connect();

			// start receiving loop
			while (true)
			{
#if UNITYWEBSOCKET_DEBUG
				Debug.Log("Recv: Receiving data from socket ...");
#endif
				string data = this.Server.RecvString(); // try read from socket
				if (!this.IsError) // got data
				{
					if (!string.IsNullOrEmpty(data))
						this.onMessage.Invoke(data); // process data
					yield return new WaitForEndOfFrame();
					continue;
				}

#if UNITYWEBSOCKET_DEBUG
				Debug.LogError(this.ErrorMessage); // error processing
#endif
				this.FreezeTime();
				for (int i = 1; i <= this.reconnectTriesMax; i++) // reconnect loop
				{
					yield return this.WaitForReconnectDelay(i);
					yield return this.Server.Connect();
					if (!this.IsError) // connection successful -> exit from reconnect loop
						break;
				}
				this.UnfreezeTime();

				if (!this.IsError) // connection successful -> proceed with receiving loop
					continue;
#if UNITYWEBSOCKET_DEBUG
				Debug.LogWarning("Reconnect failed, client stopped");
#endif
				break;
			}
		}

		private IEnumerator WaitForReconnectDelay(int tryNum)
		{
			// wait for the next try
			for (int sec = this.reconnectInterval; sec > 0; sec--)
			{
#if UNITYWEBSOCKET_DEBUG
				Debug.Log(string.Format("Reconnect: try {0} / {1} in {2} seconds ...", tryNum, this.reconnectTriesMax, sec));
#endif
				yield return new WaitForSecondsRealtime(1);
			}
		}

		private void FreezeTime()
		{
			if (!this.freezeTimeDuringReconnect)
				return;

			this.timeScaleSaved = Time.timeScale;
			Time.timeScale = 0.0f;
		}

		private void UnfreezeTime()
		{
			if (!this.freezeTimeDuringReconnect)
				return;

			Time.timeScale = this.timeScaleSaved;
		}
	}
}
