using System;
using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Common.Models
{
	public class PlayerModel : Observable
	{
		private const string IdSaveKey = "Id";
		private const string NameSaveKey = "Name";
		
		public string Id
		{
			get
			{
				if (string.IsNullOrEmpty(_id))
				{
					if (PlayerPrefs.HasKey(IdSaveKey))
						_id = PlayerPrefs.GetString(IdSaveKey);
					else
					{
						_id = Guid.NewGuid().ToString();
						PlayerPrefs.SetString(IdSaveKey, _id);
					}
				}
					
				return _id;
			}
			set => ChangeProperty(ref _id, value);
		}
		private string _id;

		public string Name
		{
			get => _name;
			set
			{
				ChangeProperty(ref _name, value);
				PlayerPrefs.SetString(NameSaveKey, _name);
			}
		}
		private string _name;

		public PlayerModel()
		{
			if (PlayerPrefs.HasKey(NameSaveKey))
				_name = PlayerPrefs.GetString(NameSaveKey);
		}
	}
}