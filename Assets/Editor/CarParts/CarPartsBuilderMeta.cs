namespace Editor {
	public class CarPartsBuilderMeta : BaseBuilderMeta<CarPartsBuilderMeta> {
		public override string AssetName => "__parts_builder_meta.asset";
		public override string BaseFolder => "Content/CarParts";

		public override string Ext => ".knpp";
		public override string DefaultBuildFolder => "Build/CarParts";
	}
}