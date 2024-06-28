using UnityEngine;

namespace Editor {
	public abstract class BaseBuilderMeta : ScriptableObject {
		public abstract bool Validate();
	}
}