param(
    [string]$JsonPath = (Join-Path $PSScriptRoot "..\Data\SeedData\skills.json")
)

$resolvedJsonPath = Resolve-Path $JsonPath -ErrorAction Stop
$skillsData = Get-Content $resolvedJsonPath -Raw | ConvertFrom-Json

$links = New-Object System.Collections.Generic.List[object]

foreach ($source in $skillsData.refreshWorkflow.sources) {
    $links.Add([pscustomobject]@{
            Label = $source.name
            Kind = $source.sourceType
            Url = $source.url
        })
}

foreach ($category in $skillsData.categories) {
    foreach ($skill in $category.skills) {
        if ($skill.guideUrl) {
            $links.Add([pscustomobject]@{
                    Label = $skill.name
                    Kind = "Skill docs"
                    Url = $skill.guideUrl
                })
        }

        if ($skill.repositoryUrl) {
            $links.Add([pscustomobject]@{
                    Label = $skill.name
                    Kind = "Reference repo"
                    Url = $skill.repositoryUrl
                })
        }
    }
}

$uniqueLinks = $links | Sort-Object Url -Unique

$results = foreach ($link in $uniqueLinks) {
    $ok = $false
    $statusCode = $null
    $lastError = ""

    foreach ($method in @("Head", "Get")) {
        try {
            $response = Invoke-WebRequest -Uri $link.Url -Method $method -MaximumRedirection 5 -UseBasicParsing -ErrorAction Stop
            $statusCode = [int]$response.StatusCode
            $ok = $true
            $lastError = $method
            break
        }
        catch {
            $exception = $_.Exception
            if ($exception.Response -and $exception.Response.StatusCode) {
                $statusCode = [int]$exception.Response.StatusCode
            }

            $lastError = $exception.Message
        }
    }

    [pscustomobject]@{
        Status = if ($ok) { "OK" } else { "FAIL" }
        Code = $statusCode
        Kind = $link.Kind
        Label = $link.Label
        Url = $link.Url
        CheckedWith = $lastError
    }
}

$results | Sort-Object Status, Kind, Label | Format-Table -AutoSize

if ($results.Status -contains "FAIL") {
    exit 1
}
