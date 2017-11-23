using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebSocket
{
	public class ClientComponentSpawn : MonoBehaviour
	{
		public GameObject prefab;

		private ClientComponent component = null;
		public ClientComponent Component
		{
			get
			{
				return this.component;
			}
		}

		void Start()
		{
			this.component = GameObject.Instantiate(this.prefab).GetComponent<ClientComponent>();
		}
	}
}
