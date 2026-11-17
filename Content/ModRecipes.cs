using CUCoreLib.Registries;

namespace TonnyOverhaul.Content;

public static class ModRecipes {
	public static void Register() {
		RecipeRegistry.Register(new Recipe {
			INT = 7,
			category = Recipes.RecipeCategory.Utilities,
			result = new RecipeResult {
				id = "toPortableLamp",
				amount = 1,
				isLiquid = false,
				resultCondition = 0,
			},
			items = [
				new RecipeItem(0) {specific = true, specificId = "lightbulb"},
				new RecipeItem(0.25f) {specific = true, specificId = "bundleofwires"},
				new RecipeItem {specific = true, specificId = "flexiglass"},
			],
		});
	}
}
