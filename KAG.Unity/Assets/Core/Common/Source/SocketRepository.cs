using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KAG.Unity.Common
{
	public sealed class SocketRepository : MonoBehaviour
	{
		#region Nested types

		[Serializable]
		private class Entry
		{
			public Socket Socket => 
				_socket;

			public Transform Target => 
				_target;
			
			[SerializeField]
			private Socket _socket;

			[SerializeField]
			private Transform _target;
		}

		#endregion

		public Transform this[Socket socket] => 
			_map[socket];

		[SerializeField]
		[HideInPlayMode]
		private Entry[] _entries;

		[ShowInInspector]
		[HideInEditorMode]
		[ReadOnly]
		private Dictionary<Socket, Transform> _map;

		private void Awake()
		{
			_map = _entries.ToDictionary(entry => entry.Socket, entry => entry.Target);
			_entries = null;
		}

		public bool TryGet(Socket socket, Transform target) => 
			_map.TryGetValue(socket, out target);
	}
}