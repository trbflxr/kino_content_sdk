﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// DO NOT EDIT THIS FILE
namespace Editor {
	public abstract class BaseBuilder<T, V> where T : BaseBuilderMeta<T> where V : BaseEntryMeta<V> {
		public void Build(BuildTarget target, T meta, AuthorMeta author, IReadOnlyCollection<V> entries) {
			if (!Validate(author, entries, out string error)) {
				Debug.LogError($"Kino: Unable to start build, validation error: {error}");
				return;
			}

			Debug.Log($"Kino: Build started for '{target}'");

			meta.Validate();

			foreach (var entry in entries) {
				if (!entry.SelectedToBuild) {
					continue;
				}

				BuildPackBundle(target, meta, author, entry);
			}
		}

		protected abstract void BuildPackBundle(BuildTarget target, T builder, AuthorMeta author, V pack);

		protected virtual bool Validate(AuthorMeta author, IReadOnlyCollection<V> entries, out string error) {
			error = string.Empty;
			return true;
		}
	}
}