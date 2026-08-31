using YamlDotNet.Serialization;

namespace DockerFactor.Engine.Manifests;

[YamlStaticContext]
[YamlSerializable(typeof(YamlApplicationManifest))]
[YamlSerializable(typeof(YamlManifestMetadata))]
[YamlSerializable(typeof(YamlApplicationSpec))]
public partial class DockerFactorYamlContext : StaticContext;
