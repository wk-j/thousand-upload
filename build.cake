#addin "wk.StartProcess"

using PS = StartProcess.Processor;

var project = "src/ThousandUpload";

Task("Watch").Does(() => {
    PS.StartProcess($"dotnet watch --project {project} run");
});

var target = Argument("target", "Watch");
RunTarget(target);
