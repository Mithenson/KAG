using System;
using KAG.Shared;
using KAG.Unity.Common.Observables;
using UnityEngine;

using Random = UnityEngine.Random;

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
				if (!string.IsNullOrEmpty(_id))
					return _id;
				
				#if KAG_DEV

				_id = Guid.NewGuid().ToString();
				PlayerPrefs.SetString(IdSaveKey, _id);
				
				#else

				if (PlayerPrefs.HasKey(IdSaveKey))
					_id = PlayerPrefs.GetString(IdSaveKey);
				else
				{
					_id = Guid.NewGuid().ToString();
					PlayerPrefs.SetString(IdSaveKey, _id);
				}
				
				#endif

				return _id;
			}
			set => ChangeProperty(ref _id, value);
		}
		private string _id;

		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(_name))
					return _name;
				
				#if KAG_DEV
				
				_name = UnityConstants.Names.Placeholders[Random.Range(0, UnityConstants.Names.Placeholders.Length)];
				PlayerPrefs.SetString(NameSaveKey, _name);
				
				#else
				
				if (PlayerPrefs.HasKey(NameSaveKey))
					_name = PlayerPrefs.GetString(NameSaveKey);
				
				#endif
				
				return _name;
			}
			set
			{
				ChangeProperty(ref _name, value);
				PlayerPrefs.SetString(NameSaveKey, _name);
			}
		}
		private string _name;
	}
}