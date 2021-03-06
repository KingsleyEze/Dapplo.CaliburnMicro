#tool "xunit.runner.console"
#tool "OpenCover"
#tool "docfx.console"
#tool "coveralls.io"
#addin "Cake.DocFx"
#addin "Cake.Coveralls"
#addin "Cake.FileHelpers"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "release");

// Used to publish NuGet packages
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NuGetApiKey"));

// Used to publish coverage report
var coverallsRepoToken = Argument("coverallsRepoToken", EnvironmentVariable("CoverallsRepoToken"));

// where is our solution located?
var solutionFilePath = GetFiles("src/*.sln").First();
var solutionName = solutionFilePath.GetDirectory().GetDirectoryName();

// Check if we are in a pull request, publishing of packages and coverage should be skipped
var isPullRequest = !string.IsNullOrEmpty(EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"));

// Check if the commit is marked as release
var isRelease = Argument<bool>("isRelease", string.Compare("[release]", EnvironmentVariable("appveyor_repo_commit_message_extended"), true) == 0);

Task("Default")
	.IsDependentOn("Publish");

// Publish taks depends on publish specifics
Task("Publish")
	.IsDependentOn("PublishPackages")
	.IsDependentOn("PublishCoverage")
	.IsDependentOn("Documentation")
	.WithCriteria(() => !BuildSystem.IsLocalBuild);

// Publish the coveralls report to Coveralls.io
Task("PublishCoverage")
	.IsDependentOn("Coverage")
	.WithCriteria(() => !BuildSystem.IsLocalBuild)
	.WithCriteria(() => !string.IsNullOrEmpty(coverallsRepoToken))
	.WithCriteria(() => !isPullRequest)
	.Does(()=>
{
	CoverallsIo("./artifacts/coverage.xml", new CoverallsIoSettings
	{
		RepoToken = coverallsRepoToken
	});
});

// Publish the Artifacts of the Package Task to NuGet
Task("PublishPackages")
	.IsDependentOn("Coverage")
	.WithCriteria(() => !BuildSystem.IsLocalBuild)
	.WithCriteria(() => !string.IsNullOrEmpty(nugetApiKey))
	.WithCriteria(() => !isPullRequest)
	.WithCriteria(() => isRelease)
	.Does(()=>
{
	var settings = new NuGetPushSettings {
		Source = "https://www.nuget.org/api/v2/package",
		ApiKey = nugetApiKey
	};

	var packages = GetFiles("./src/**/*.nupkg").Where(p => !p.FullPath.ToLower().Contains("symbols"));
	NuGetPush(packages, settings);
});

// Build the DocFX documentation site
Task("Documentation")
	.Does(() =>
{
	// Run DocFX
	DocFxMetadata("./doc/docfx.json");
	DocFxBuild("./doc/docfx.json");
	
	CreateDirectory("artifacts");
	// Archive the generated site
	Zip("./doc/_site", "./artifacts/site.zip");
});

// Run the XUnit tests via OpenCover, so be get an coverage.xml report
Task("Coverage")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory("artifacts");

	var openCoverSettings = new OpenCoverSettings() {
		// Forces error in build when tests fail
		ReturnTargetCodeOffset = 0
	};

	// Build a list of projects to include or exclude from the coverage report
	var projectFilePaths = GetFiles("./**/*.csproj")
		.Where(p => !p.FullPath.ToLower().Contains("demo"))
		.Where(p => !p.FullPath.ToLower().Contains("packages"))
		.Where(p => !p.FullPath.ToLower().Contains("tools"))
		.Where(p => !p.FullPath.ToLower().Contains("example"));

	var baseDirectory = solutionFilePath.GetDirectory().FullPath;
	foreach(var projectFile in projectFilePaths)
	{
		var projectName = projectFile.GetDirectory().GetDirectoryName();
		if (projectName.ToLower().Contains("test") || !projectFile.FullPath.StartsWith(baseDirectory)) {
			Information("Excluding from coverage: " + projectFile.FullPath);
			openCoverSettings.WithFilter("-["+projectName+"]*");
		}
		else {
			Information("Including to coverage: " + projectFile.FullPath);
			openCoverSettings.WithFilter("+["+projectName+"]*");
		}
	}

	var testDllGob = "./**/bin/" + configuration + "/net4*/*.Tests.dll";
	Information("Searching for test DLL with: " + testDllGob);
	var testDlls = GetFiles(testDllGob);
	foreach(var testDll in testDlls)
    {
		Information("Found test DLL at: " + testDll.FullPath);
	}
	
	var xunit2Settings = new XUnit2Settings {
		// Add AppVeyor output, this "should" take care of a report inside AppVeyor
		ArgumentCustomization = args => {
			if (!BuildSystem.IsLocalBuild) {
				args.Append("-appveyor");
			}
			return args;
		},
		ShadowCopy = false,
		XmlReport = true,
		HtmlReport = true,
		ReportName = solutionName,
		OutputDirectory = "./artifacts",
		WorkingDirectory = "./src"
	};

	// Make XUnit 2 run via the OpenCover process
	var testDllPath = testDlls.First().FullPath;
	Information("Running coverage on test DLL: " + testDllPath);
	OpenCover(
		// The test tool Lamdba
		tool => tool.XUnit2(testDllPath, xunit2Settings),
		// The output path
		new FilePath("./artifacts/coverage.xml"),
		// Settings
		openCoverSettings
	);
});

// This starts the actual MSBuild
Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
{
	MSBuild(solutionFilePath.FullPath, new MSBuildSettings {
		Verbosity = Verbosity.Minimal,
		ToolVersion = MSBuildToolVersion.VS2017,
		Configuration = configuration,
		Restore = true,
		PlatformTarget = PlatformTarget.MSIL
	});
});

Task("DNC30Only")
    .Does(() =>
{
    ReplaceRegexInFiles("./**/*.csproj", "<TargetFrameworks>net471;netcoreapp3.0</TargetFrameworks>", "<TargetFramework>netcoreapp3.0</TargetFramework><!-- net471;netcoreapp3.0 -->");
});


// Clean all unneeded files, so we build on a clean file system
Task("Clean")
	.Does(() =>
{
	CleanDirectories("./**/obj");
	CleanDirectories("./**/bin");
	CleanDirectories("./artifacts");
});

RunTarget(target);