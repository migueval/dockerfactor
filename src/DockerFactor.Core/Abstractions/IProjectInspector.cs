using DockerFactor.Core.Inspection;

namespace DockerFactor.Core.Abstractions;

public interface IProjectInspector
{
    ProjectInspection Inspect(string projectDirectory);
}
