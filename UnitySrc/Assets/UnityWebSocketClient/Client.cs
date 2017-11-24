using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket
{
	public enum State
	{
		NotStarted,
		Connecting,
		Working,
		ErrorRaised,
		FatalError
	}

	public class Client
	{
		public const string GAMEOBJECT_NAME = "_WebSocketClient_";
		public const string ERR_SOCKET_CLOSED = "Socket is closed";

		public static int CLIENT_ID = 1;

#region Input
		private string url;
		private bool freezeTimeDuringReconnect;
		private System.Action<State> onStageChanged;
		private System.Action<string> onMessage;
		private System.Action<string> onError;
		private int reconnectTriesMax;
		private int reconnectInterval;
#endregion

#region Socket related
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
#endregion

#region Internal
		private float timeScaleSaved;
		private State state = State.NotStarted;
#endregion

#region API
		public State State { get { return this.state; } }

		public Client(
			string url,
			System.Action<State> onStageChanged,
			System.Action<string> onMessage,
			System.Action<string> onError,
			bool freezeTimeReconnecting,
			int reconnectTriesMax,
			int reconnectInterval
			)
		{
			this.url = url;
			this.onStageChanged = onStageChanged;
			this.onMessage = onMessage;
			this.onError = onError;
			this.freezeTimeDuringReconnect = freezeTimeReconnecting;
			this.reconnectTriesMax = reconnectTriesMax;
			this.reconnectInterval = reconnectInterval;
		}

		public IEnumerator Start()
		{
			this.Server = new WebSocket(this.url);
			this.SetState(State.Connecting);
			yield return this.Server.Connect();
			yield return this.ReceivingLoop();
		}

		public void Send(string data)
		{
			if (!this.Server.IsOpen) return;
			this.Server.SendString(data);
		}

		public void SendBinary(byte[] data)
		{
			if (!this.Server.IsOpen) return;
			this.Server.Send(data);
		}
#endregion

		private IEnumerator ReceivingLoop()
		{
			// start receiving loop
			while (true)
			{
				string data = this.Server.RecvString(); // try read from socket
				if (!this.IsError) // socket working fine
				{
					this.SetState(State.Working);
					if (!string.IsNullOrEmpty(data))
					{
#if UNITYWEBSOCKET_DEBUG
						Debug.Log("Recv: Got incoming data");
#endif
						this.onMessage.Invoke(data); // process data
					}
					yield return new WaitForEndOfFrame();
					continue;
				}

				// error processing
#if UNITYWEBSOCKET_DEBUG
				Debug.LogError(this.ErrorMessage);
#endif
				this.SetState(State.ErrorRaised);
				this.onError.Invoke(this.ErrorMessage);

				yield return this.TryReconnect();
				if (!this.IsError) // connection successful -> proceed with receiving loop
					continue;
#if UNITYWEBSOCKET_DEBUG
				Debug.LogWarning("Reconnect failed, client stopped");
#endif
				this.SetState(State.FatalError);
				break;
			}
		}

		private IEnumerator TryReconnect()
		{
			this.FreezeTime();
			for (int i = 1; i <= this.reconnectTriesMax; i++) // reconnect loop
			{
				this.SetState(State.Connecting);
				yield return this.WaitForReconnectDelay(i);
				yield return this.Server.Connect();
				if (!this.IsError) // connection successful -> exit from reconnect loop
					break;
			}
			this.UnfreezeTime();
		}

		private void SetState(State state)
		{
			if (this.state == state)
				return;
			this.state = state;
			this.onStageChanged.Invoke(this.state);
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
