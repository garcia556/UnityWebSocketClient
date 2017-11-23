using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
	public string url = "ws://localhost:8080/";

	private void OnMessage(string data)
	{
		Debug.Log("Got data from WebSocket: " + data);
	}

	void Awake() { this.StartCoroutine(this.AwakeWorker()); }
	private IEnumerator AwakeWorker()
	{
		UnityWebSocket.Client client = new UnityWebSocket.Client(
			this.url,		// 
			this.OnMessage,	// data handler
			true,			// freeze time while reconnecting
			3,				// max reconnect tries
			5,				// reconnect interval in seconds
			true			// show debug UI
			);
		yield return client.Connect();
	}
}
