using System.Collections.Generic;

namespace HumanCastle {
	class AutoDictionary<K,V> : Dictionary<K,V> where V : new() {
		public new V this[ K key ] { get {
			if (!ContainsKey(key)) Add(key,new V());
			return base[key];
		} set {
			if (!ContainsKey(key)) Add(key,value);
			else base[key] = value;
		}}
	}
}
