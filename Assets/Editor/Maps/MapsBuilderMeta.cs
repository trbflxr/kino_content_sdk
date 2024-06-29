namespace Editor {
	public class MapsBuilderMeta : BaseBuilderMeta<MapsBuilderMeta> {
		public override string AssetName => "__maps_builder_meta.asset";
		public override string BaseFolder => "Content/Maps";

		public override string Ext => ".knmap";
		public override string DefaultBuildFolder => "Build/Maps";
	}
}