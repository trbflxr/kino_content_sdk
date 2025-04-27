namespace Editor {
	public class GarageBuilderMeta  : BaseBuilderMeta<GarageBuilderMeta> {
		public override string AssetName => "__garage_builder_meta.asset";
		public override string BaseFolder => "Content/Garage";

		public override string Ext => ".kngarage";
		public override string DefaultBuildFolder => "Build/Garage";
	}
}