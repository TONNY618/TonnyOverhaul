using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TonnyOverhaul.Core;

public static class DevHelpers {
	public static readonly List<string> CustomItems = [];

	public static IEnumerator SortConsoleAutofillRoutine() {
		yield return new WaitUntil(() => ConsoleScript.Commands is not null && ConsoleScript.Commands.Count > 0);

		var spawnCommand = ConsoleScript.Commands.Find(c => c.name == "spawn");

		if (spawnCommand?.argAutofill is null || !spawnCommand.argAutofill.TryGetValue(0, out List<string> itemList)) yield break;
		var sorted = itemList
			.OrderByDescending(id => CustomItems.Contains(id))
			.ThenBy(x => x)
			.ToList();
		itemList.Clear();
		itemList.AddRange(sorted);
	}

	private static IEnumerator GiveStartingItemsRoutine(string[] items) {
		yield return new WaitUntil(() => PlayerCamera.main != null && PlayerCamera.main.body != null);

		var player = PlayerCamera.main.body;
		var pos = player.transform.position;

		for (var i = 0; i < items.Length; i++) {
			var go = Utils.Create(items[i], pos, 0f);
			if (go == null) continue;
			var item = go.GetComponent<Item>();
			if (item != null)
				player.PickUpItem(item, i, true);
		}
	}
}
