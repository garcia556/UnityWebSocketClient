using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket.Demo
{
	public class Bootstrap : MonoBehaviour
	{
		public string url = "ws://localhost:8080/";

		private void OnMessage(string data)
		{
			Debug.Log("Got data from WebSocket: " + data);
		}

		private void OnError(string error)
		{
			Debug.LogError("Error: " + error);
		}

		private UnityWebSocket.State stateCurrent;
		private void OnStateChanged(UnityWebSocket.State state)
		{
			Debug.LogWarningFormat("State changed from {0} to {1}", this.stateCurrent.ToString(), state.ToString());
			this.stateCurrent = state;			
		}

		void Awake() { this.StartCoroutine(this.AwakeWorker()); }
		private IEnumerator AwakeWorker()
		{
			UnityWebSocket.Client client = new UnityWebSocket.Client(
				this.url,
				this.OnStateChanged,
				this.OnMessage,
				this.OnError,
				true,	// freeze time while reconnecting
				3,		// max reconnect tries
				5		// reconnect interval in seconds
				);
			this.stateCurrent = client.State;
			yield return client.Start();
		}
	}
}
