Param(
    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$tagname,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$releaseName,

    [Parameter(mandatory=$False, ValueFromPipeline=$False)]
    [string]$commitId = $null,

    [Parameter(mandatory=$False, ValueFromPipeline=$False)]
    [switch]$IsPreRelease,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$releaseNotes,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$artifactDirectory,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$artifactFileName,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$gitHubUsername,

    [Parameter(mandatory=$True, ValueFromPipeline=$False)]
    [string]$gitHubRepository
)


function GitHub-Release($tagname, $releaseName, $commitId, $IsPreRelease, $releaseNotes, $artifactDirectory, $artifact, $gitHubUsername, $gitHubRepository, $gitHubApiKey)
{
    <#
        .SYNOPSIS
        Creates a tag and release on GitHub.

        .DESCRIPTION
        Creates a tag and release on GitHub and optionally uploads release artifacts.
        Based on https://gist.github.com/JanJoris/ee4c7f9b4289016b2216

        .PARAMETER tagname
        Name of the tag the release should be associated with.  Required.
        See commitId (below) for details of where tag is created.
        An error occurs if the tag already exists.

        .PARAMETER releaseName
        The name of the release

        .PARAMETER commitId
        The hash value the tag should be placed on.
            If commitID is blank, the tag is created on master.
            If commitID is a commit hash, the tag is created on the commit.
            If commitID is null, and the tag doesn't already exist, the tag is created on master, else the existing
                tag is used.

        .PARAMETER IsPreRelease
        Boolean value, set to $True to mark the release as a pre-release, $False to
        mark it as a finalized release.

        .PARAMETER releaseNotes
        Description of the release.

        .PARAMETER artifactDirectory
        Path to where the artifact to be uploaded may be found.

        .PARAMETER artifact
        Name of the file to be uploaded. This value also becomes the name of the artifact
        file to be downloaded.

        .PARAMETER gitHubUsername
        User or organization who owns the remote repository

        .PARAMETER gitHubRepository
        Name of the remote repository

        .PARAMETER gitHubApiKey
        GitHub personal access token with repo full control permission.
        https://github.com/settings/tokens

    #>

    $draft = $FALSE

    $releaseData = @{
       tag_name = $tagname
       name = $releaseName;
       body = $releaseNotes;
       draft = $draft;
       prerelease = $IsPreRelease;
    }

    # Don't want the target_commitish element unless it's set to something.
    if ($commitId -ne $null) {
        $releaseData.target_commitish = $commitId;
    }

    $auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($gitHubApiKey + ":x-oauth-basic"));

    $releaseParams = @{
       Uri = "https://api.github.com/repos/$gitHubUsername/$gitHubRepository/releases";
       Method = 'POST';
       Headers = @{
         Authorization = $auth;
       }
       ContentType = 'application/json';
       Body = (ConvertTo-Json $releaseData -Compress)
    }

    $result = Invoke-RestMethod @releaseParams
    $uploadUri = $result | Select -ExpandProperty upload_url
    Write-Host $uploadUri
    $uploadUri = $uploadUri -creplace '\{\?name,label\}'  #, "?name=$artifact"
    $uploadUri = $uploadUri + "?name=$artifact"
    $uploadFile = Join-Path -path $artifactDirectory -childpath $artifact

    $uploadParams = @{
      Uri = $uploadUri;
      Method = 'POST';
      Headers = @{
        Authorization = $auth;
      }
      ContentType = 'application/zip';
      InFile = $uploadFile
    }
    $result = Invoke-RestMethod @uploadParams
}

Try {
	GitHub-Release $tagname $releaseName $commitId ($IsPreRelease -eq $True)  $releaseNotes $artifactDirectory $artifactFileName $gitHubUsername $gitHubRepository $env:GITHUB_TOKEN
}
Catch {
	# Explicitly exit with an error.
	Write-Error "An error has occured $_"
	Exit 1
}

